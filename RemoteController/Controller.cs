// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController;

using RemoteController.Interop;
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
	private const uint WATCHDOG_TIMEOUT_MS = 60000;
	private const uint WATCHDOG_TIMER_INTERVAL_MS = 15000;
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
	private static readonly ConcurrentDictionary<uint, byte> s_sequenceCounters = new(); // Key: Packed hook ID, Value: Sequence counter
	private static readonly ObjectPool<PendingRequest<byte[]>> s_pendingRequestPool = new(maxSize: 128);

	private static readonly WorkPipeline s_workPipeline = new(Math.Max(Environment.ProcessorCount / 2, 1));
	private static readonly ObjectPool<WorkItem<(uint, byte[], int)>> s_workItemPool = new(maxSize: 128);

	public static SignatureScanner? Scanner = null;
	public static SignatureResolver? SigResolver = null;
	private static FrameworkDriver? s_frameworkDriver = null;

	private static Endpoint? s_outgoingEndpoint = null;
	private static Endpoint? s_incomingEndpoint = null;

	private static long s_heartbeatTimestamp = 0;
	private static Timer? s_watchdogTimer;
	private static volatile bool s_running = true;
	private static bool s_dllImportResolverSet = false;

	[RequiresDynamicCode("Requires dynamic code")]
	private static unsafe void* NativePtr() => (delegate* unmanaged<void>)&RemoteControllerEntry;

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
		if (!s_running || s_outgoingEndpoint == null)
			return s_emptyPayload;

		byte seq = GetNextSequence(hookIndex);
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
			if (!s_outgoingEndpoint.Write(header, argsPayload, IPC_TIMEOUT_MS))
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

	/// <summary>
	/// Sends a framework tick handle request to the host application and waits for a response.
	/// </summary>
	/// <returns>
	/// True if if the framework should continue propagating tick detours; false otherwise.
	/// </returns>
	[RequiresDynamicCode("MarshalUtils.Serialize requires dynamic code")]
	public static bool SendFrameworkRequest()
	{
		if (!s_running || s_outgoingEndpoint == null)
			return false;

		byte seq = GetNextSequence(HookMessageId.FRAMEWORK_SYSTEM_ID);
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
				sent = s_outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS);
			}
			else
			{
				rented = ArrayPool<byte>.Shared.Rent(payloadSize);
				Span<byte> rentedSpan = rented.AsSpan(0, payloadSize);
				MarshalUtils.Write(rentedSpan, in data);
				sent = s_outgoingEndpoint.Write(header, rentedSpan, IPC_TIMEOUT_MS);
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

	/// <summary>
	/// The entry point for the remote controller.
	/// </summary>
	/// <remarks>
	/// This function is invoked remotely via unmanaged code.
	/// </remarks>
	[UnmanagedCallersOnly(EntryPoint = "RemoteControllerEntry")]
	[RequiresUnreferencedCode("This code snippet is not trimming-safe.")]
	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	public static void RemoteControllerEntry()
	{
		Cleanup(); // Ensure that no previous state lingers
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
			s_heartbeatTimestamp = Environment.TickCount64;
			s_watchdogTimer = new Timer(CheckWatchdog, null, WATCHDOG_TIMER_INTERVAL_MS, WATCHDOG_TIMER_INTERVAL_MS);

			// Main loop
			while (s_running)
			{
				ProcessIncomingMessages();
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

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void Cleanup()
	{
		Log.Information("Running shutdown sequence...");
		HookRegistry.Instance.UnregisterAll();
		s_frameworkDriver?.Dispose();
		s_frameworkDriver = null;
		s_watchdogTimer?.Dispose();
		s_outgoingEndpoint?.Dispose();
		s_outgoingEndpoint = null;
		s_incomingEndpoint?.Dispose();
		s_incomingEndpoint = null;

		Log.Information("Shutdown complete. Remember us...Remember...that we once lived.");
		Logger.Deinitialize(); // Keep the logger as the last step
		GC.Collect(2, GCCollectionMode.Forced, true);
		GC.WaitForPendingFinalizers();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CheckWatchdog(object? state)
	{
		if (Environment.TickCount64 - s_heartbeatTimestamp > WATCHDOG_TIMEOUT_MS)
		{
			Log.Warning("No heartbeat received for 60 seconds. Controller shutdown is requested.");
			s_running = false;
		}
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
						if (HookMessageId.GetHookId(header.Id) != HookMessageId.FRAMEWORK_SYSTEM_ID)
						{
							HandleWrapperInvoke(header.Id, payload);
						}
						else
						{
							HandleFrameworkCommand(header.Id, payload);
						}
						break;

					case PayloadType.Blob:
						HandleHookResponse(header.Id, payload);
						break;

					case PayloadType.Bye:
						HandleGoodbyeMessage(header.Id);
						break;

					case PayloadType.Heartbeat:
						s_heartbeatTimestamp = Environment.TickCount64;
						Log.Verbose("Received heartbeat message.");
						break;

					case PayloadType.Register:
						Log.Verbose("Received register hook message.");
						HandleHookRegister(header.Id, payload);
						break;

					case PayloadType.Unregister:
						Log.Verbose("Received unregister hook message.");
						HandleHookUnregister(header.Id);
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

		var regPayload = MemoryMarshal.Read<HookRegistrationData>(data);
		string delegateKey = regPayload.GetKey();

		if (regPayload.HookType == HookType.System)
		{
			// Handle system hook registration
			nint address = SigResolver?.Resolve(delegateKey) ?? 0;
			try
			{
				if (address == 0)
					throw new Exception($"Failed to resolve address for delegate key: {delegateKey}.");

				if (regPayload.HookId == HookMessageId.FRAMEWORK_SYSTEM_ID)
				{
					s_frameworkDriver?.Dispose();
					s_frameworkDriver = new FrameworkDriver(address);
					Log.Information($"Framework driver registered at 0x{address:X}.");
					byte[] payload = BitConverter.GetBytes(HookMessageId.FRAMEWORK_SYSTEM_ID);
					var header = new MessageHeader(requestId, PayloadType.Ack, (ulong)payload.Length);
					s_outgoingEndpoint?.Write(header, payload, IPC_TIMEOUT_MS);
				}
				else
				{
					Log.Warning($"Unknown system hook ID: {regPayload.HookId}. Failed registration.");
					SendResponse(requestId, PayloadType.NAck);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to register system hook with request ID {requestId}.");
				SendResponse(requestId, PayloadType.NAck);
			}
		}
		else
		{
			// Handle standard hook registration
			uint hookId = HookRegistry.Instance.RegisterHook(regPayload);

			if (hookId != 0)
			{
				byte[] payload = BitConverter.GetBytes(hookId);
				var header = new MessageHeader(requestId, PayloadType.Ack, (ulong)payload.Length);
				s_outgoingEndpoint?.Write(header, payload, IPC_TIMEOUT_MS);
			}
			else
			{
				SendResponse(requestId, PayloadType.NAck);
			}
		}
	}

	[RequiresDynamicCode("HookRegistry requires dynamic code")]
	private static void HandleHookUnregister(uint hookId)
	{
		bool success = HookRegistry.Instance.UnregisterHook(hookId);
		SendResponse(hookId, success ? PayloadType.Ack : PayloadType.NAck);
	}

	private static void SendResponse(uint msgId, PayloadType responseType)
	{
		if (s_outgoingEndpoint == null)
			return;

		var header = new MessageHeader(msgId, responseType);
		s_outgoingEndpoint.Write(header, IPC_TIMEOUT_MS);
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

			if (mode == DispatchMode.FrameworkTick)
			{
				result = FrameworkDriver.EnqueueAndWait(ExecuteLogic);
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
			if (s_frameworkDriver != null)
			{
				s_frameworkDriver.IsSyncEnabled = true;
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

	private static void HandleGoodbyeMessage(uint msgId)
	{
		try
		{
			Log.Information("Received goodbye message from host. Shutting down controller...");
			SendResponse(msgId, PayloadType.Ack);
		}
		finally
		{
			s_running = false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetNextSequence(uint hookIndex)
	{
		return s_sequenceCounters.AddOrUpdate(hookIndex, 1, s_incrementFunc);
	}
}
