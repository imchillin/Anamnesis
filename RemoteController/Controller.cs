// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController;

using RemoteController.Drivers;
using RemoteController.Interop;
using RemoteController.Interop.Types;
using RemoteController.IPC;
using RemoteController.Memory;
using Serilog;
using SharedMemoryIPC;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[RequiresUnreferencedCode("This class uses reflection-based hook invocation.")]
[RequiresDynamicCode("This class uses dynamic code for hook invocation.")]
public class Controller
{
	private const string BUF_SHMEM_OUTGOING = "Local\\ANAM_SHMEM_CTRL_TO_MAIN";
	private const string BUF_SHMEM_INCOMING = "Local\\ANAM_SHMEM_MAIN_TO_CTRL";
	
	private const int PROCESS_MONITOR_JOIN_TIMEOUT_MS = 500;
	private const int ACTIVE_WRITERS_DRAIN_TIMEOUT_MS = 500;
	private const int IPC_TIMEOUT_MS = 100;
	private const int IPC_FAST_FAIL_TIMEOUT_MS = 16;
	private const int BATCH_INVOKE_BUFFER_STARTSIZE = 1024;
	private const int STACKALLOC_THRESHOLD = 256;

	private const string FASM_RESOURCE_NAME = "FASMX64";
	private const string FASM_RESOURCE_FILENAME = $"{FASM_RESOURCE_NAME}.dll";
	private const string FASM_RESOURCE_SEARCH_PATTERN = $"{FASM_RESOURCE_NAME}.*dll";

	private static readonly InProcessMemoryReader s_memoryReader = new();
	private static readonly int s_sizeOfInt = sizeof(int);
	private static readonly Func<uint, byte, byte> s_incrementFunc = static (_, v) => (byte)((v + 1) & 0xFF);
	private static readonly byte[] s_emptyPayload = [];
	private static readonly ArrayPool<byte> s_bufferPool = ArrayPool<byte>.Shared;
	private static readonly ConcurrentDictionary<uint, PendingRequest<byte[]>> s_pendingHooks = new();
	private static readonly ConcurrentDictionary<uint, ushort> s_sequenceCounters = new(); // Key: Packed hook ID, Value: Sequence counter
	private static readonly ConcurrentDictionary<EventId, int> s_eventSubscriptionRefCount = new();
	private static readonly ObjectPool<PendingRequest<byte[]>> s_pendingRequestPool = new(maxSize: 128);

	private static readonly WorkPipeline s_workPipeline = new(Math.Max(Environment.ProcessorCount / 2, 1));
	private static readonly ObjectPool<WorkItem<(uint, byte[], int)>> s_workItemPool = new(maxSize: 128);

	private static readonly Dictionary<DriverCommand, Func<ReadOnlySpan<byte>, byte[]>> s_commandHandlers = new()
	{
		// Posing driver commands
		[DriverCommand.GetPosingEnabled] = DriverCommandHandler.ConditionalGetter(
			() => PosingDriver.IsInitialized,
			() => PosingDriver.Instance.PosingEnabled),

		[DriverCommand.SetPosingEnabled] = DriverCommandHandler.ConditionalSetter<bool>(
			() => PosingDriver.IsInitialized,
			v => PosingDriver.Instance.PosingEnabled = v),

		[DriverCommand.GetFreezePhysics] = DriverCommandHandler.ConditionalGetter(
			() => PosingDriver.IsInitialized,
			() => PosingDriver.Instance.FreezePhysics),

		[DriverCommand.SetFreezePhysics] = DriverCommandHandler.ConditionalSetter<bool>(
			() => PosingDriver.IsInitialized,
			v => PosingDriver.Instance.FreezePhysics = v),

		[DriverCommand.GetFreezeWorldVisualState] = DriverCommandHandler.ConditionalGetter(
			() => PosingDriver.IsInitialized,
			() => PosingDriver.Instance.FreezeWorldVisualState),

		[DriverCommand.SetFreezeWorldVisualState] = DriverCommandHandler.ConditionalSetter<bool>(
			() => PosingDriver.IsInitialized,
			v => PosingDriver.Instance.FreezeWorldVisualState = v),

		// Gpose driver commands
		[DriverCommand.GetIsInGpose] = DriverCommandHandler.ConditionalGetter(
			() => GposeDriver.IsInitialized,
			() => GposeDriver.Instance.IsInGpose),

		// Actor driver commands
		[DriverCommand.UpdateActorDrawData] = HandleUpdateActorDrawData,
	};

#pragma warning disable CA2211
	public static SignatureScanner? Scanner = null;
	public static SignatureResolver? SigResolver = null;
#pragma warning restore CA2211

	private static DriverManager? s_driverManager = null;
	private static Endpoint? s_outgoingEndpoint = null;
	private static Endpoint? s_incomingEndpoint = null;
	private static volatile bool s_running = true;
	private static bool s_dllImportResolverSet = false;
	private static IntPtr s_mainProcessHandle = IntPtr.Zero;
	private static Thread? s_processMonitorThread = null;
	private static CancellationTokenSource s_shutdownCts= new();
	private static int s_activeWriters = 0;

	[RequiresDynamicCode("Requires dynamic code")]
	private static unsafe void* NativePtr() => (delegate* unmanaged<IntPtr, void>)&RemoteControllerEntry;

	/// <summary>
	/// Sends an intercept request to the host application and waits for a response.
	/// </summary>
	/// <param name="hookIndex">
	/// The index of the hook that is being intercepted.
	/// </param>
	/// <param name="argsPayload">
	/// The serialized arguments payload for the intercept request.
	/// </param>
	/// <returns>
	/// The serialized response payload from the host application.
	/// </returns>
	/// <remarks>
	/// For <see cref="HookBehavior.After"/> hooks, the request payload
	/// layout is as follows: [Int32 ArgsLength] [Args Data] [Result Data].
	/// You can use the utility function <see cref="HookUtils.DeserializeAfterPayload"/> 
	/// to parse this layout.
	/// </remarks>
	public static byte[] SendInterceptRequest(uint hookIndex, ReadOnlySpan<byte> argsPayload)
	{
		if (!s_running)
			return s_emptyPayload;

		Interlocked.Increment(ref s_activeWriters);
		try
		{
			var endpoint = s_outgoingEndpoint;
			if (!s_running || endpoint == null)
				return s_emptyPayload;

			ushort seq = GetNextSequence(hookIndex);
			uint msgId = HookMessageId.Pack(hookIndex, seq);

			var pending = s_pendingRequestPool.Get();
			if (!s_pendingHooks.TryAdd(msgId, pending))
			{
				s_pendingRequestPool.Return(pending);
				return s_emptyPayload;
			}

			try
			{
				var header = new MessageHeader(msgId, PayloadType.Request, (ulong)argsPayload.Length);
				if (!endpoint.Write(header, argsPayload, IPC_TIMEOUT_MS))
				{
					Log.Warning($"Failed to send intercept request for hook {hookIndex}");
					return s_emptyPayload;
				}

				// Wait for response
				if (!pending.Wait(IPC_TIMEOUT_MS))
				{
					Log.Warning($"Timeout waiting for intercept response for hook {hookIndex} (MsgId: {msgId}).");
					return s_emptyPayload;
				}

				return pending.TryGetResult(out byte[]? result) ? result ?? [] : [];
			}
			finally
			{
				s_pendingHooks.TryRemove(msgId, out _);
				pending.Reset();
				s_pendingRequestPool.Return(pending);
			}
		}
		finally
		{
			Interlocked.Decrement(ref s_activeWriters);
		}
	}

	/// <summary>
	/// Sends a framework tick handle request to the host application and waits for a response.
	/// </summary>
	/// <returns>
	/// True if if the framework should continue propagating tick detours; false otherwise.
	/// </returns>
	[RequiresDynamicCode("MarshalUtils.Serialize requires dynamic code")]
	public static bool SendFrameworkRequest()
	{
		if (!s_running)
			return false;

		Interlocked.Increment(ref s_activeWriters);
		try
		{
			var endpoint = s_outgoingEndpoint;
			if (!s_running || endpoint == null)
				return false;

			ushort seq = GetNextSequence(HookMessageId.FRAMEWORK_SYSTEM_ID);
			uint msgId = HookMessageId.Pack(HookMessageId.FRAMEWORK_SYSTEM_ID, seq);

			var pending = s_pendingRequestPool.Get();
			if (!s_pendingHooks.TryAdd(msgId, pending))
			{
				s_pendingRequestPool.Return(pending);
				return false;
			}

			byte[]? rented = null;
			int payloadSize = Unsafe.SizeOf<FrameworkMessageData>();
			var data = new FrameworkMessageData { Type = FrameworkMessageType.TickSyncRequest };
			var header = new MessageHeader(msgId, PayloadType.Request, (ulong)payloadSize);
			bool sent;

			try
			{
				if (payloadSize <= STACKALLOC_THRESHOLD)
				{
					Span<byte> payload = stackalloc byte[payloadSize];
					MarshalUtils.Write(payload, in data);
					sent = endpoint.Write(header, payload, IPC_TIMEOUT_MS);
				}
				else
				{
					rented = ArrayPool<byte>.Shared.Rent(payloadSize);
					Span<byte> rentedSpan = rented.AsSpan(0, payloadSize);
					MarshalUtils.Write(rentedSpan, in data);
					sent = endpoint.Write(header, rentedSpan, IPC_TIMEOUT_MS);
				}

				if (!sent)
				{
					Log.Warning("Failed to send framework tick sync request.");
					return false;
				}

				if (!pending.Wait(IPC_FAST_FAIL_TIMEOUT_MS))
					return false;

				if (pending.TryGetResult(out byte[]? result) && result != null && result.Length > 0)
					return result[0] != 0;

				return false;
			}
			finally
			{
				if (rented != null)
					ArrayPool<byte>.Shared.Return(rented);

				s_pendingHooks.TryRemove(msgId, out _);
				pending.Reset();
				s_pendingRequestPool.Return(pending);
			}
		}
		finally
		{
			Interlocked.Decrement(ref s_activeWriters);
		}
	}

	/// <summary>
	/// Publishes an event to the main application if at least one subscriber exists.
	/// </summary>
	/// <typeparam name="T">The event payload type.</typeparam>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="data">The event payload data.</param>
	public static void PublishEvent<T>(EventId eventId, T data)
		where T : unmanaged
	{
		if (!s_running)
			return;

		// Check if anyone is subscribed
		if (!s_eventSubscriptionRefCount.TryGetValue(eventId, out int refCount) || refCount <= 0)
			return;

		Interlocked.Increment(ref s_activeWriters);
		try
		{
			var endpoint = s_outgoingEndpoint;
			if (!s_running || endpoint == null)
				return;

			int payloadSize = Unsafe.SizeOf<T>();
			bool result = false;
			byte[]? rented = null;
			var header = new MessageHeader((uint)eventId, PayloadType.Event, (ulong)payloadSize);

			try
			{
				if (payloadSize <= STACKALLOC_THRESHOLD)
				{
					Span<byte> payload = stackalloc byte[payloadSize];
					MarshalUtils.Write(payload, in data);
					result = endpoint.Write(header, payload, IPC_TIMEOUT_MS);
				}
				else
				{
					rented = ArrayPool<byte>.Shared.Rent(payloadSize);
					Span<byte> rentedSpan = rented.AsSpan(0, payloadSize);
					MarshalUtils.Write(rentedSpan, in data);
					result = endpoint.Write(header, rentedSpan, IPC_TIMEOUT_MS);
				}

				if (!result)
				{
					Log.Verbose($"Failed to publish event {eventId}.");
				}
			}
			finally
			{
				if (rented != null)
					ArrayPool<byte>.Shared.Return(rented);
			}
		}
		finally
		{
			Interlocked.Decrement(ref s_activeWriters);
		}
	}

	/// <summary>
	/// The entry point for the remote controller.
	/// </summary>
	/// <remarks>
	/// This function is invoked remotely via unmanaged code.
	/// </remarks>
	[UnmanagedCallersOnly(EntryPoint = "RemoteControllerEntry")]
	[RequiresUnreferencedCode("This code snippet is not trimming-safe.")]
	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	public static void RemoteControllerEntry(IntPtr mainProcessHandle)
	{
		Cleanup();
		s_mainProcessHandle = mainProcessHandle;
		var workerThread = new Thread(Main)
		{
			IsBackground = true,
			Name = "RemoteController.Main",
		};
		workerThread.Start();
	}

	[RequiresUnreferencedCode("This code snippet is not trimming-safe as it uses the hook manager.")]
	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void Main()
	{
		try
		{
			Logger.Initialize();
			Log.Information($"Starting remote controller on native thread ID {NativeFunctions.GetCurrentThreadId()}...");

			// Set assembler path so that Reloaded.Assembler can find the FASM DLL
			if (!SetupAssemblerPath())
			{
				Log.Error("Failed to set up assembler path.");
				return;
			}

			// Locate the last copied library to avoid linking Reloaded.Hooks to a mismatched resource
			string? fasmPath = FindFasmDll(Directory.GetCurrentDirectory());
			if (fasmPath == null)
				return;

			if (!s_dllImportResolverSet)
			{
				NativeLibrary.SetDllImportResolver(typeof(Reloaded.Hooks.ReloadedHooks).Assembly, (libName, asm, searchPath) =>
				{
					if (libName.Equals(FASM_RESOURCE_FILENAME, StringComparison.OrdinalIgnoreCase) || libName.Equals(FASM_RESOURCE_NAME, StringComparison.OrdinalIgnoreCase))
						return NativeLibrary.Load(fasmPath);

					return IntPtr.Zero;
				});
				s_dllImportResolverSet = true;
			}

			Process currentProcess = Process.GetCurrentProcess();
			if (currentProcess.MainModule == null)
			{
				Log.Error("Failed to get main module of the current process.");
				return;
			}
			Scanner = new SignatureScanner(currentProcess.MainModule, s_memoryReader);
			SigResolver = new SignatureResolver(Scanner, s_memoryReader);

			Log.Debug("Creating IPC endpoints...");
			try
			{
				s_outgoingEndpoint = new Endpoint(BUF_SHMEM_OUTGOING);
				s_incomingEndpoint = new Endpoint(BUF_SHMEM_INCOMING);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize IPC endpoints.");
				return;
			}

			Log.Debug("Successfully created IPC endpoints.");
			s_running = true;

			s_processMonitorThread = new Thread(MonitorMainAppProcess)
			{
				IsBackground = true,
				Name = "RemoteController.ProcessMonitor",
			};
			s_processMonitorThread.Start();

			s_driverManager = new DriverManager();
			s_driverManager.Initialize();

			// Main loop
			uint taskIndex = 0;
			IntPtr avrtHandle = NativeFunctions.AvSetMmThreadCharacteristics("Games", ref taskIndex);
			try
			{
				while (s_running)
				{
					ProcessIncomingMessages();
				}
			}
			finally
			{
				if (avrtHandle != IntPtr.Zero)
					NativeFunctions.AvRevertMmThreadCharacteristics(avrtHandle);
			}
		}
		catch (Exception ex)
		{
			// IMPORTANT: Don't throw to avoid crashing the game process
			// The intent is to catch all unhandled exceptions and terminate gracefully
			Log.Fatal(ex, "Unhandled exception.");
		}
		finally
		{
			Cleanup();

			/* Unload */
			// IMPORTANT: This MUST be the last action to execute in the entry point.
			// Otherwise, the code after this call will not execute as we self-unload and terminate the thread.
			unsafe
			{
				if (NativeFunctions.GetModuleHandleEx(
						(uint)NativeFunctions.GetModuleFlag.GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | (uint)NativeFunctions.GetModuleFlag.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
						(IntPtr)NativePtr(),
						out IntPtr hModule))
				{
					NativeFunctions.FreeLibraryAndExitThread(hModule, 0);
				}
			}
		}
	}

	/// <summary>
	/// Sets up the FASM assembler path for Reloaded.Hooks.
	/// Required when running in an injected context where assembly location is unavailable.
	/// </summary>
	[RequiresDynamicCode("Uses GetModulePath, which requires dynamic code")]
	private static bool SetupAssemblerPath()
	{
		string? modulePath = GetModulePath();
		if (string.IsNullOrEmpty(modulePath))
		{
			Log.Warning("Could not determine module path for FASM setup.");
			return false;
		}

		string? fasmDir = Path.GetDirectoryName(modulePath);
		if (string.IsNullOrEmpty(fasmDir))
		{
			Log.Warning("Could not determine FASM directory.");
			return false;
		}

		// Set current directory so Reloaded.Assembler finds FASM DLLs
		Directory.SetCurrentDirectory(fasmDir);
		Log.Debug($"Set assembler path to: {fasmDir}");
		return true;
	}

	[RequiresDynamicCode("Uses NativePtr, which requires dynamic code")]
	private static unsafe string? GetModulePath()
	{
		if (!NativeFunctions.GetModuleHandleEx(
			(uint)NativeFunctions.GetModuleFlag.GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
			(uint)NativeFunctions.GetModuleFlag.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
			(IntPtr)NativePtr(),
			out IntPtr hModule))
		{
			return null;
		}

		Span<char> buffer = stackalloc char[260];
		fixed (char* pBuffer = buffer)
		{
			uint length = NativeFunctions.GetModuleFileName(hModule, pBuffer, (uint)buffer.Length);
			return length > 0 ? new string(buffer[..(int)length]) : null;
		}
	}

	private static string? FindFasmDll(string directory)
	{
		var dirInfo = new DirectoryInfo(directory);
		var latestFasmDll = dirInfo.GetFiles(FASM_RESOURCE_SEARCH_PATTERN, SearchOption.TopDirectoryOnly)
			.OrderByDescending(f => f.LastWriteTimeUtc)
			.FirstOrDefault();

		if (latestFasmDll == null)
		{
			Log.Error($"Failed to locate FASMX64 library in {directory}");
			return null;
		}

		Log.Debug($"Discovered FASM DLL: {latestFasmDll.FullName}");
		return latestFasmDll.FullName;
	}

	private static void MonitorMainAppProcess()
	{
		Log.Information("Process monitor started.");

		if (s_mainProcessHandle == IntPtr.Zero)
		{
			Log.Warning("Handle is null. Cannot monitor.");
			s_running = false;
			return;
		}

		try
		{
			using var processWaitHandle = new ManualResetEvent(false)
			{
				SafeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(s_mainProcessHandle, ownsHandle: false)
			};

			// [0] The Process
			// [1] The Cancellation Token
			WaitHandle[] handles = { processWaitHandle, s_shutdownCts.Token.WaitHandle };
			Log.Debug($"Monitoring Handle: {s_mainProcessHandle}. Blocking thread until signal...");

			bool requestedShutdown = false;
			while (!requestedShutdown)
			{
				int signaledIndex = WaitHandle.WaitAny(handles);
				if (signaledIndex == 0) // The Process signaled (Index 0)
				{
					Log.Information("Main application process terminated. Initiating shutdown...");
					requestedShutdown = true;
				}
				else if (signaledIndex == 1) // Shutdown requested via cancellation token (Index 1)
				{
					Log.Debug("Monitor cancelled by user/shutdown request.");
					requestedShutdown = true;
				}
				else // Spurious wakeup, check if process is alive
				{
					if (NativeFunctions.GetExitCodeProcess(s_mainProcessHandle, out uint exitCode) && exitCode != NativeFunctions.PROCESS_STILL_ALIVE)
					{
						Log.Information($"[SPURIOUS WAKEUP] Main application terminated with code {exitCode}. Initiating shutdown...");
						requestedShutdown = false;
					}
				}
			}
		}
		catch (ArgumentException)
		{
			// Occurs if the process exits between GetProcessId and GetProcessById
			Log.Information("Process exited during initialization.");
		}
		finally
		{
			s_running = false;
		}

		Log.Debug("Process monitor exiting (shutdown requested).");
	}

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void Cleanup()
	{
		Log.Information("Running shutdown sequence...");

		s_shutdownCts.Cancel();
		s_running = false;

		if (s_processMonitorThread != null && s_processMonitorThread.IsAlive)
		{
			if (!s_processMonitorThread.Join(PROCESS_MONITOR_JOIN_TIMEOUT_MS))
				Log.Warning("Process monitor thread did not exit in time.");

			s_processMonitorThread = null;
		}

		if (s_mainProcessHandle != IntPtr.Zero)
		{
			NativeFunctions.CloseHandle(s_mainProcessHandle);
			s_mainProcessHandle = IntPtr.Zero;
		}

		s_shutdownCts.Dispose();
		s_shutdownCts = new CancellationTokenSource();

		if (!SpinWait.SpinUntil(() => Volatile.Read(ref s_activeWriters) == 0, ACTIVE_WRITERS_DRAIN_TIMEOUT_MS))
		{
			Log.Warning($"Active writers did not drain within {ACTIVE_WRITERS_DRAIN_TIMEOUT_MS}ms (remaining: {s_activeWriters}). Proceeding with disposal.");
		}
		else
		{
			Log.Debug("All active writers drained successfully.");
		}

		HookRegistry.Instance.UnregisterAll();
		s_driverManager?.Dispose();
		s_driverManager = null;

		s_outgoingEndpoint?.Dispose();
		s_outgoingEndpoint = null;
		s_incomingEndpoint?.Dispose();
		s_incomingEndpoint = null;

		s_pendingHooks.Clear();
		s_sequenceCounters.Clear();
		s_eventSubscriptionRefCount.Clear();

		Log.Information("Shutdown complete. Remember us...Remember...that we once lived.");
		Logger.Deinitialize(); // Keep the logger as the last step
		GC.Collect(2, GCCollectionMode.Forced, true);
		GC.WaitForPendingFinalizers();
	}

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void ProcessIncomingMessages()
	{
		if (s_incomingEndpoint == null)
			return;

		try
		{
			// NOTE: A non-zero timeout is recommended to avoid busy-waiting.
			while (s_incomingEndpoint.Read(out MessageHeader header, out ReadOnlySpan<byte> payload, IPC_TIMEOUT_MS))
			{
				switch (header.Type)
				{
					case PayloadType.Request:
						{
							uint hookId = HookMessageId.GetHookId(header.Id);
							if (hookId == HookMessageId.FRAMEWORK_SYSTEM_ID)
								HandleFrameworkCommand(header.Id, payload);
							else
								HandleWrapperInvoke(header.Id, payload);
						}
						break;

					case PayloadType.Command:
						HandleDriverCommand(header.Id, payload);
						break;

					case PayloadType.Blob:
						HandleHookResponse(header.Id, payload);
						break;

					case PayloadType.Bye:
						HandleGoodbyeMessage(header.Id);
						break;

					case PayloadType.Register:
						Log.Verbose("Received register hook message.");
						HandleHookRegister(header.Id, payload);
						break;

					case PayloadType.Unregister:
						Log.Verbose("Received unregister hook message.");
						HandleHookUnregister(header.Id);
						break;

					case PayloadType.EventSubscribe:
						Log.Verbose("Received event subscribe message.");
						HandleEventSubscribe(header.Id, payload);
						break;

					case PayloadType.EventUnsubscribe:
						Log.Verbose("Received event unsubscribe message.");
						HandleEventUnsubscribe(header.Id, payload);
						break;

					default:
						Log.Warning($"Received unhandled message[ID: {header.Id}, Type: {header.Type}, Length: {header.Length}].");
						break;
				}
			}
		}
		catch (NullReferenceException ex)
		{
			Log.Warning(ex, "IPC endpoint closed unexpectedly (shared memory was disposed). Stopping message processing.");
			s_running = false;
		}
		catch (ObjectDisposedException ex)
		{
			Log.Warning(ex, "IPC endpoint disposed. Stopping message processing.");
			s_running = false;
		}
	}

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void HandleHookRegister(uint requestId, ReadOnlySpan<byte> data)
	{
		if (data.Length < Unsafe.SizeOf<HookRegistrationData>())
		{
			SendResponse(requestId, PayloadType.NAck);
			return;
		}

		try
		{
			var regPayload = MemoryMarshal.Read<HookRegistrationData>(data);

			byte[] result = FrameworkDriver.RunOnTick(() =>
			{
			uint hookId = HookRegistry.Instance.RegisterHook(regPayload);
				if (hookId == 0)
					return []; // Failure

				return BitConverter.GetBytes(hookId);
			});

			if (result.Length > 0)
			{
				var header = new MessageHeader(requestId, PayloadType.Ack, (ulong)result.Length);
				s_outgoingEndpoint?.Write(header, result, IPC_TIMEOUT_MS);
			}
			else
			{
				Log.Error($"Failed to register hook for request {requestId} on framework thread.");
				SendResponse(requestId, PayloadType.NAck);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Unhandled exception when registering hook.");
		}
	}

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void HandleHookUnregister(uint hookId)
	{
		try
		{
			byte[] result = FrameworkDriver.RunOnTick(() =>
			{
		bool success = HookRegistry.Instance.UnregisterHook(hookId);
				return [(byte)(success ? 1 : 0)];
			});

			bool wasSuccessful = result.Length > 0 && result[0] == 1;
			SendResponse(hookId, wasSuccessful ? PayloadType.Ack : PayloadType.NAck);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to unregister hook {hookId} on framework thread.");
			SendResponse(hookId, PayloadType.NAck);
		}
	}

	private static void SendResponse(uint msgId, PayloadType responseType)
	{
		if (s_outgoingEndpoint == null)
			return;

		var header = new MessageHeader(msgId, responseType);
		s_outgoingEndpoint.Write(header, IPC_TIMEOUT_MS);
	}

	private static void SendResponse(uint msgId, ReadOnlySpan<byte> payload, PayloadType responseType = PayloadType.Blob)
	{
		if (s_outgoingEndpoint == null)
			return;
		
		var header = new MessageHeader(msgId, responseType, (ulong)payload.Length);
		s_outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS);
	}

	private static void ProcessInvokeInternal((uint MsgId, byte[] Data, int Length) state)
	{
		uint hookId = HookMessageId.GetHookId(state.MsgId);

		try
		{
			DispatchMode mode = (DispatchMode)state.Data[0];
			byte[] result;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			byte[] ExecuteLogic()
			{
				var args = new ReadOnlySpan<byte>(state.Data, 1, state.Length - 1);
				if (hookId != HookMessageId.BATCH_HOOK_ID)
					return HookRegistry.Instance.InvokeOriginal(hookId, args);
				else
					return ProcessInvokeBatch(args);
			}

			if (mode == DispatchMode.FrameworkTick && FrameworkDriver.IsInitialized)
			{
				result = FrameworkDriver.RunOnTick(ExecuteLogic);
			}
			else
			{
				result = ExecuteLogic();
			}

			SendWrapperResult(state.MsgId, result);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error invoking hook[ID: {hookId}]");
			SendWrapperResult(state.MsgId, []);
		}
		finally
		{
			s_bufferPool.Return(state.Data);
		}
	}

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void HandleWrapperInvoke(uint msgId, ReadOnlySpan<byte> argsPayload)
	{
		int payloadLength = argsPayload.Length;
		byte[] rentedBuffer = s_bufferPool.Rent(payloadLength);
		argsPayload.CopyTo(rentedBuffer);

		var workItem = s_workItemPool.Get();
		unsafe
		{
			workItem.Initialize(s_workItemPool, &ProcessInvokeInternal, (msgId, rentedBuffer, payloadLength));
		}

		s_workPipeline.Enqueue(workItem);
	}

	[RequiresDynamicCode("MarshalUtils requires dynamic code")]
	private static byte[] ProcessInvokeBatch(ReadOnlySpan<byte> batchArgs)
	{
		// Payload layout:
		// [Int32 HookCount][(Int32 HookId, Int32 PayloadLength, byte[] Payload),...]

		byte[] resultBuffer = s_bufferPool.Rent(BATCH_INVOKE_BUFFER_STARTSIZE);
		int writeOffset = 0;

		try
		{
			int count = MarshalUtils.Deserialize<int>(batchArgs[..s_sizeOfInt]);
			ReadOnlySpan<byte> cursor = batchArgs[s_sizeOfInt..];

			for (int i = 0; i < count; i++)
			{
				uint hId = MarshalUtils.Deserialize<uint>(cursor[..s_sizeOfInt]);
				int argLen = MarshalUtils.Deserialize<int>(cursor.Slice(s_sizeOfInt, s_sizeOfInt));
				ReadOnlySpan<byte> args = cursor.Slice(2 * s_sizeOfInt, argLen);

				// Invoke
				byte[] result = HookRegistry.Instance.InvokeOriginal(hId, args);

				// Resize if needed
				int needed = sizeof(int) + result.Length;
				if (writeOffset + needed > resultBuffer.Length)
				{
					var newBuffer = s_bufferPool.Rent(Math.Max(resultBuffer.Length * 2, writeOffset + needed));
					Buffer.BlockCopy(resultBuffer, 0, newBuffer, 0, writeOffset);
					s_bufferPool.Return(resultBuffer);
					resultBuffer = newBuffer;
				}

				// Write [Len][Data]
				MarshalUtils.Write(resultBuffer.AsSpan(writeOffset), result.Length);
				writeOffset += sizeof(int);

				if (result.Length > 0)
				{
					Buffer.BlockCopy(result, 0, resultBuffer, writeOffset, result.Length);
					writeOffset += result.Length;
				}

				cursor = cursor[((2 * s_sizeOfInt) + argLen)..];
			}

			return resultBuffer.AsSpan(0, writeOffset).ToArray();
		}
		finally
		{
			s_bufferPool.Return(resultBuffer);
		}
	}

	private static void SendWrapperResult(uint msgId, byte[] resultPayload)
	{
		if (s_outgoingEndpoint == null)
			return;

		var header = new MessageHeader(msgId, PayloadType.Blob, (ulong)resultPayload.Length);
		if (!s_outgoingEndpoint.Write(header, resultPayload, IPC_TIMEOUT_MS))
		{
			Log.Warning($"Failed to send wrapper result for message ID: {msgId}");
		}
	}

	private static void HandleHookResponse(uint msgId, ReadOnlySpan<byte> data)
	{
		if (s_pendingHooks.TryGetValue(msgId, out var pending))
		{
			pending.SetResult(data.ToArray());
		}
		else
		{
			Log.Warning($"Received response for unknown request with message ID: {msgId}.");
		}
	}

	private static void HandleFrameworkCommand(uint msgId, ReadOnlySpan<byte> payload)
	{
		if (payload.Length < Unsafe.SizeOf<FrameworkMessageData>())
		{
			SendResponse(msgId, PayloadType.NAck);
			return;
		}

		var msg = MemoryMarshal.Read<FrameworkMessageData>(payload);

		if (msg.Type == FrameworkMessageType.EnableTickSync)
		{
			if (FrameworkDriver.IsInitialized)
			{
				FrameworkDriver.Instance.IsSyncEnabled = true;
				SendResponse(msgId, PayloadType.Ack);
				return;
			}
		}
		else
		{
			Log.Warning($"Unhandled framework command: {msg.Type}");
		}

		SendResponse(msgId, PayloadType.NAck);
	}

	private static void HandleDriverCommand(uint msgId, ReadOnlySpan<byte> payload)
	{
		if (payload.Length < sizeof(int))
		{
			SendResponse(msgId, []);
			return;
		}

		DriverCommand commandId = MarshalUtils.Read<DriverCommand>(payload);
		ReadOnlySpan<byte> args = payload[sizeof(int)..];

		byte[] response;
		if (s_commandHandlers.TryGetValue(commandId, out var handler))
		{
			try
			{
				response = handler(args);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error executing driver command: 0x{commandId:X4}");
				response = [];
			}
		}
		else
		{
			Log.Warning($"Unknown driver command: 0x{commandId:X4}");
			response = [];
		}

		SendResponse(msgId, response);
	}

	private static void HandleEventSubscribe(uint messageId, ReadOnlySpan<byte> payload)
	{
		if (payload.Length < Unsafe.SizeOf<EventSubscriptionData>())
		{
			SendResponse(messageId, PayloadType.NAck);
			return;
		}

		try
		{
			var data = MemoryMarshal.Read<EventSubscriptionData>(payload);

			// Increment refcount
			int newCount = s_eventSubscriptionRefCount.AddOrUpdate(
				data.EventId,
				addValue: 1,
				updateValueFactory: (_, current) => ++current);

			Log.Information($"Event subscription added: {data.EventId} (refcount: {newCount})");
			SendResponse(messageId, PayloadType.Ack);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error handling event subscribe message.");
			SendResponse(messageId, PayloadType.NAck);
		}
	}

	private static void HandleEventUnsubscribe(uint messageId, ReadOnlySpan<byte> payload)
	{
		if (payload.Length < Unsafe.SizeOf<EventSubscriptionData>())
		{
			SendResponse(messageId, PayloadType.NAck);
			return;
		}

		try
		{
			var data = MemoryMarshal.Read<EventSubscriptionData>(payload);

			if (data.Flags.HasFlag(EventSubscriptionFlags.UnsubscribeAll))
			{
				s_eventSubscriptionRefCount.TryRemove(data.EventId, out int previousCount);
				Log.Information($"Unsubscribed all from event: {data.EventId} (cleared refcount from {previousCount})");
			}
			else
			{
				// Decrement refcount
				int newCount = s_eventSubscriptionRefCount.AddOrUpdate(
					data.EventId,
					addValue: 0,
					updateValueFactory: (_, current) => Math.Max(0, --current));

				if (newCount == 0)
				{
					s_eventSubscriptionRefCount.TryRemove(data.EventId, out _);
				}

				Log.Information($"Event subscription removed: {data.EventId} (refcount: {newCount})");
			}

			SendResponse(messageId, PayloadType.Ack);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error handling event unsubscribe message.");
			SendResponse(messageId, PayloadType.NAck);
		}
	}

	private static byte[] HandleUpdateActorDrawData(ReadOnlySpan<byte> args)
	{
		// Payload layout: [nint drawObjectAddress (8)] [byte skipEquipment (1)] [byte[] drawData (remaining)].
		const int headerSize = sizeof(long) + 1; // nint + skipEquipment byte

		if (!ActorDriver.IsInitialized || args.Length <= headerSize)
			return [0];

		nint drawObjectAddr = MemoryMarshal.Read<nint>(args);
		bool skipEquipment = args[sizeof(long)] != 0;
		byte[] drawData = args[headerSize..].ToArray();

		if (FrameworkDriver.IsInitialized)
		{
			return FrameworkDriver.RunOnTick(() =>
			{
				bool success = ActorDriver.Instance.UpdateDrawData(drawObjectAddr, drawData, skipEquipment);
				return [(byte)(success ? 1 : 0)];
			});
		}

		bool result = ActorDriver.Instance.UpdateDrawData(drawObjectAddr, drawData, skipEquipment);
		return [(byte)(result ? 1 : 0)];
	}

	private static void HandleGoodbyeMessage(uint msgId)
	{
		Log.Information("Received goodbye message from host. Shutting down controller...");
		s_running = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ushort GetNextSequence(uint hookIndex)
	{
		return (ushort)s_sequenceCounters.AddOrUpdate(hookIndex, 1, (_, v) => (ushort)((v + 1) & HookMessageId.MAX_SEQ_NUM));
	}
}
