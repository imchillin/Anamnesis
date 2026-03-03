// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Services;
using RemoteController;
using RemoteController.Interop;
using RemoteController.IPC;
using RemoteController.Memory;
using Serilog;
using SharedMemoryIPC;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// An interface for batch result placeholders.
/// </summary>
public interface IBatchResult
{
	void SetResult(ReadOnlySpan<byte> data);
}

/// <summary>
/// Represents a handle to a registered hook.
/// Returned upon hook registration via <see cref="ControllerService"/>.
/// </summary>
public sealed class HookHandle : IDisposable
{
	private readonly ControllerService service;
	private bool disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="HookHandle"/> class.
	/// </summary>
	/// <param name="service">
	/// A reference to the controller service that created this hook.
	/// </param>
	/// <param name="hookId">
	/// The unique identifier of the hook.
	/// </param>
	/// <param name="delegateKey">
	/// A unique identifier for the delegate type associated with this hook.
	/// </param>
	internal HookHandle(ControllerService service, uint hookId, string delegateKey)
	{
		this.service = service;
		this.HookId = hookId;
		this.DelegateKey = delegateKey;
	}

	/// <summary>
	/// Gets the unique identifier of the hook.
	/// </summary>
	public uint HookId { get; private set; }

	/// <summary>
	/// Gets the unique identifier for the delegate type associated with this hook.
	/// </summary>
	public string DelegateKey { get; }

	/// <summary>
	/// Gets a value indicating whether the hook handle is still valid.
	/// </summary>
	public bool IsValid => this.HookId != 0 && !this.disposed;

	/// <summary>
	/// Disposes the hook handle, unregistering the associated hook.
	/// </summary>
	public void Dispose()
	{
		if (this.disposed)
			return;

		if (this.HookId != 0)
		{
			this.service.UnregisterHookInternal(this.HookId);
		}

		this.disposed = true;
	}

	/// <summary>
	/// Sets the hook identifier.
	/// </summary>
	/// <remarks>
	/// Intended to be used only by <see cref="ControllerService"/> during hook registration.
	/// in cases where the remote controller is not able to register the hook immediately.
	/// </remarks>
	internal void SetHookId(uint id)
	{
		this.HookId = id;
	}
}

/// <summary>
/// A placeholder for a batched invocation result.
/// </summary>
/// <typeparam name="T">
/// The expected type of the return value.
/// </typeparam>
public class BatchResult<T> : IBatchResult
	where T : unmanaged
{
	public T Value { get; private set; }
	public bool HasValue { get; private set; }

	public static implicit operator T(BatchResult<T> result) => result.Value;

	public void SetResult(ReadOnlySpan<byte> data)
	{
		this.HasValue = true;

		if (data.Length == 0) // void result
			return;

		if (data.Length < Unsafe.SizeOf<T>())
		{
			Log.Warning($"BatchResult<{typeof(T).Name}> data length mismatch. Expected at least {Unsafe.SizeOf<T>()} bytes, got {data.Length} bytes.");
		}

		this.Value = MarshalUtils.Deserialize<T>(data);
	}
}

/// <summary>
/// A scope for batching multiple hook invocations into a single IPC call.
/// </summary>
/// <param name="service">
/// The controller service used to execute the batch invocation.
/// </param>
public class BatchInvokeScope(ControllerService service) : IDisposable
{
	private static readonly int s_sizeOfInt = sizeof(int);

	private readonly ControllerService service = service;
	private readonly ArrayBufferWriter<byte> buffer = new();
	private readonly List<IBatchResult> results = new();
	private int hookCount = 0;
	private bool isDisposed = false;

	public DispatchMode Mode { get; set; } = DispatchMode.Immediate;
	public int TimeoutMs { get; set; } = ControllerService.IPC_TIMEOUT_MS;

	/// <summary>
	/// Appends a wrapper hook invocation request to the batch invoke scope.
	/// </summary>
	/// <typeparam name="T">
	/// The return type of the hooked function.
	/// </typeparam>
	/// <param name="handle">
	/// A handle to the registered wrapper hook.
	/// </param>
	/// <param name="args">
	/// The serialized arguments to pass to the hooked function.
	/// </param>
	/// <returns>
	/// A placeholder result via a wrapper <see cref="BatchResult{T}"/> that is
	/// updated when the batch invocation is executed.
	/// </returns>
	public BatchResult<T> AddCall<T>(HookHandle handle, params object[] args)
		where T : unmanaged
	{
		if (!handle.IsValid)
			throw new ArgumentException("Invalid hook handle.", nameof(handle));

		int size = MarshalUtils.ComputeArgsSize(args);
		byte[] poolBuffer = ArrayPool<byte>.Shared.Rent(size);
		try
		{
			Span<byte> buffer = poolBuffer.AsSpan(0, size);
			MarshalUtils.SerializeArgs(buffer, args);
			return this.AddCall<T>(handle.HookId, buffer);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(poolBuffer);
		}
	}

	/// <summary>
	/// Appends a wrapper hook invocation request to the batch invoke scope.
	/// </summary>
	/// <typeparam name="T">
	/// The return type of the hooked function.
	/// </typeparam>
	/// <param name="hookId">
	/// The identifier of the registered wrapper hook.
	/// </param>
	/// <param name="args">
	/// The serialized arguments to pass to the hooked function.
	/// </param>
	/// <returns>
	/// A placeholder result via a wrapper <see cref="BatchResult{T}"/> that is
	/// updated when the batch invocation is executed.
	/// </returns>
	public BatchResult<T> AddCall<T>(uint hookId, ReadOnlySpan<byte> args)
		where T : unmanaged
	{
		this.hookCount++;

		// Pack hook identifier
		Span<byte> hookIdSpan = this.buffer.GetSpan(s_sizeOfInt);
		MarshalUtils.WriteToSpan(hookIdSpan, hookId);
		this.buffer.Advance(s_sizeOfInt);

		// Pack payload length
		Span<byte> lengthSpan = this.buffer.GetSpan(s_sizeOfInt);
		MarshalUtils.WriteToSpan(lengthSpan, args.Length);
		this.buffer.Advance(s_sizeOfInt);

		// Pack payload
		this.buffer.Write(args);

		// Create a result placeholder
		var result = new BatchResult<T>();
		this.results.Add(result);
		return result;
	}

	/// <summary>
	/// Appends a wrapper hook invocation request to the batch invoke scope.
	/// </summary>
	/// <typeparam name="T">
	/// The return type of the hooked function.
	/// </typeparam>
	/// <param name="handle">
	/// A handle to the registered wrapper hook.
	/// </param>
	/// <param name="args">
	/// The serialized arguments to pass to the hooked function.
	/// </param>
	/// <returns>
	/// A placeholder result via a wrapper <see cref="BatchResult{T}"/> that is
	/// updated when the batch invocation is executed.
	/// </returns>
	public BatchResult<T> AddCall<T>(HookHandle handle, ReadOnlySpan<byte> args)
		where T : unmanaged
	{
		if (!handle.IsValid)
			throw new ArgumentException("Invalid hook handle.", nameof(handle));

		return this.AddCall<T>(handle.HookId, args);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this.hookCount == 0 || this.isDisposed)
			return;

		// [Int32 HookCount][(Int32 HookId, Int32 PayloadLength, byte[] Payload),...]
		ulong totalSize = (ulong)(s_sizeOfInt + this.buffer.WrittenCount);
		Span<byte> finalBuffer = new byte[totalSize];
		MarshalUtils.WriteToSpan(finalBuffer[..s_sizeOfInt], this.hookCount);
		this.buffer.WrittenSpan.CopyTo(finalBuffer[s_sizeOfInt..]);

		byte[]? response = this.service.InvokeHookRaw(HookMessageId.BATCH_HOOK_ID, finalBuffer, this.Mode, this.TimeoutMs);

		try
		{
			if (response == null || response.Length == 0)
			{
				Log.Warning("Batch invocation failed or returned no response.");
				return;
			}

			ReadOnlySpan<byte> responseSpan = response;
			int offset = 0;

			for (int i = 0; i < this.results.Count; i++)
			{
				if (offset + s_sizeOfInt > responseSpan.Length)
				{
					Log.Warning($"Batch invocation response size mismatch. Cannot read result length for call {i}.");
					return;
				}

				var payloadLength = MarshalUtils.Deserialize<int>(responseSpan.Slice(offset, s_sizeOfInt));
				var resultPayload = responseSpan.Slice(offset + s_sizeOfInt, payloadLength);
				this.results[i].SetResult(resultPayload);
				offset += s_sizeOfInt + payloadLength;
			}
		}
		finally
		{
			this.buffer.Clear();
			ControllerService.ClearActiveBatch();
			this.isDisposed = true;
			GC.SuppressFinalize(this);
		}
	}
}

/// <summary>
/// A service that communicates with the remote controller that we inject into the game process.
/// The service is responsible for sending and receiving messages to and from the controller, including
/// watchdog heartbeats, and function hook communication.
/// </summary>
public class ControllerService : ServiceBase<ControllerService>
{
	internal const int IPC_TIMEOUT_MS = 100;

	private const string BUF_SHMEM_OUTGOING = "Local\\ANAM_SHMEM_MAIN_TO_CTRL";
	private const string BUF_SHMEM_INCOMING = "Local\\ANAM_SHMEM_CTRL_TO_MAIN";
	private const uint BUF_BLK_COUNT = 128;
	private const ulong BUF_BLK_SIZE = 8192;

	private const int IPC_REGISTER_TIMEOUT_MS = 1000;
	private const int HEARTBEAT_INTERVAL_MS = 15_000;
	private const int STACKALLOC_THRESHOLD = 256;
	private const int CONN_CHECK_DELAY_MS = 1000;

	private static readonly Func<uint, byte, byte> s_incrementFunc = static (_, v) => (byte)((v + 1) & 0xFF);

	[ThreadStatic]
	private static BatchInvokeScope? s_currentBatch;

	private readonly ConcurrentDictionary<string, HookRegistrationInfo> allHooks = new();
	private readonly ConcurrentQueue<HookRegistrationInfo> registrationQueue = new();
	private readonly ConcurrentDictionary<uint, PendingRequest<byte[]>> pendingHookRequests = new();
	private readonly ConcurrentDictionary<uint, PendingRequest<uint>> pendingRegistrations = new();
	private readonly ConcurrentDictionary<uint, PendingRequest<bool>> pendingUnregistrations = new();
	private readonly ConcurrentDictionary<uint, Func<ReadOnlySpan<byte>, byte[]>> handlers = new();
	private readonly ConcurrentDictionary<uint, byte> sequenceCounters = new();
	private readonly ConcurrentDictionary<string, uint> delegateKeyToHookId = new();
	private readonly ObjectPool<PendingRequest<byte[]>> wrapperRequestPool = new(maxSize: 128);
	private readonly PendingRequest<bool> pendingByeMessage = new();

	private readonly WorkPipeline workPipeline = new(Math.Max(Environment.ProcessorCount / 2, 1));
	private readonly ObjectPool<WorkItem<(ControllerService, uint, byte[], int)>> workItemPool = new(maxSize: 128);

	private readonly WorkQueue frameworkQueue = new();
	private FrameworkService? framework;

	private uint nextRequestId = 0;

	private Endpoint? outgoingEndpoint = null;
	private Endpoint? incomingEndpoint = null;
	private Timer? heartbeatTimer;
	private bool isConnected = false;

	/// <summary>
	/// Gets the framework service used for executing
	/// work on the game process' framework tick.
	/// </summary>
	public FrameworkService Framework => this.framework
		?? throw new InvalidOperationException("Framework is not initialized");

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [GameService.Instance];

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		this.framework = new FrameworkService(this.frameworkQueue, this.SendFrameworkSyncRequest);
		await base.Initialize();
	}

	/// <inheritdoc/>
	public override async Task Shutdown()
	{
		this.SendShutdownMessage();

		foreach (var kvp in this.pendingHookRequests)
		{
			kvp.Value.Dispose();
		}

		foreach (var kvp in this.pendingRegistrations)
		{
			kvp.Value.Dispose();
		}

		foreach (var kvp in this.pendingUnregistrations)
		{
			kvp.Value.Dispose();
		}

		foreach (var kvp in this.allHooks)
		{
			kvp.Value.Handle.Dispose();
		}

		this.pendingHookRequests.Clear();
		this.pendingRegistrations.Clear();
		this.pendingUnregistrations.Clear();
		this.allHooks.Clear();
		this.outgoingEndpoint?.Dispose();
		this.incomingEndpoint?.Dispose();
		await base.Shutdown();
	}

	/// <summary>
	/// Invokes a registered hook synchronously and returns the raw byte result.
	/// The original function will be called by the remote controller.
	/// </summary>
	/// <param name="hookId">
	/// The ID of the registered hook.
	/// </param>
	/// <param name="argsPayload">
	/// The serialized arguments to pass to the hooked function.
	/// </param>
	/// <param name="mode">
	/// The context in which to dispatch the hook invocation.
	/// See <see cref="DispatchMode"/> for more details.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds for the invocation to return.
	/// </param>
	/// <returns>
	/// The raw byte array result from the hooked function, or <c>null</c> if the invocation failed or timed out.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the controller service is not initialized.
	/// </exception>
	/// <remarks>
	/// This method is intended for advanced scenarios where the caller needs direct access to the raw result bytes.
	/// For strongly-typed results, use <see cref="InvokeHook{TResult}(uint, ReadOnlySpan{byte}, DispatchMode, int)"/>.
	/// </remarks>
	public byte[]? InvokeHookRaw(uint hookId, ReadOnlySpan<byte> argsPayload, DispatchMode mode = DispatchMode.Immediate, int timeoutMs = IPC_TIMEOUT_MS)
	{
		if (this.outgoingEndpoint == null)
			throw new InvalidOperationException("Controller service not initialized.");

		if (hookId == 0)
		{
			Log.Warning($"Attempted to invoke hook {hookId} before registration completed.");
			return default;
		}

		byte seq = this.GetNextSequence(hookId);
		uint msgId = HookMessageId.Pack(hookId, seq);

		var pending = this.wrapperRequestPool.Get();
		this.pendingHookRequests[msgId] = pending;

		int argsLength = argsPayload.Length + 1;
		Span<byte> packedPayload = stackalloc byte[argsLength];
		packedPayload[0] = (byte)mode;
		argsPayload.CopyTo(packedPayload[1..]);

		try
		{
			var header = new MessageHeader(msgId, PayloadType.Request, (ulong)argsLength);
			try
			{
				if (!this.outgoingEndpoint.Write(header, packedPayload, IPC_TIMEOUT_MS))
				{
					Log.Warning($"Hook[ID: {hookId}] invocation failed to send.");
					return default;
				}
			}
			catch (NullReferenceException ex)
			{
				Log.Warning(ex, "IPC endpoint not available. Returning default message on invoke.");
				return default;
			}
			catch (ObjectDisposedException ex)
			{
				Log.Warning(ex, "IPC endpoint disposed. Returning default message on invoke.");
				return default;
			}

			if (!pending.Wait(timeoutMs))
			{
				Log.Warning($"Hook[ID: {hookId}] invocation timed out.");
			}

			if (!pending.TryGetResult(out byte[]? result) || result == null || result.Length == 0)
			{
				return default;
			}

			return result;
		}
		finally
		{
			this.pendingHookRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.wrapperRequestPool.Return(pending);
		}
	}

	/// <summary>
	/// Invokes the registered wrapper hook synchronously.
	/// The original function will be called by the remote controller.
	/// </summary>
	/// <typeparam name="TResult">The expected return type of the hooked function.</typeparam>
	/// <param name="handle">
	/// The handle of the registered hook.
	/// </param>
	/// <param name="mode">
	/// The context in which to dispatch the hook invocation.
	/// See <see cref="DispatchMode"/> for more details.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds for the invocation to return.
	/// </param>
	/// <param name="args">
	/// The arguments to pass to the hooked function.
	/// </param>
	/// <returns>
	/// A typed result from the hooked function, or the default value of <c>TResult</c>
	/// if the invocation failed or timed out.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// Thrown if the provided hook handle is invalid.
	/// </exception>
	public TResult? InvokeHook<TResult>(HookHandle handle, DispatchMode mode = DispatchMode.Immediate, int timeoutMs = IPC_TIMEOUT_MS, params object[] args)
		where TResult : unmanaged
	{
		if (handle == null || !handle.IsValid)
			throw new ArgumentException("Invalid hook handle.", nameof(handle));

		int size = MarshalUtils.ComputeArgsSize(args);
		if (size <= STACKALLOC_THRESHOLD)
		{
			Span<byte> buffer = stackalloc byte[size];
			MarshalUtils.SerializeArgs(buffer, args);

			// In this case, the placeholder result is discarded
			// If you need the result, use AddCall directly.
			if (s_currentBatch != null)
			{
				s_currentBatch.AddCall<TResult>(handle.HookId, buffer);
				return default;
			}

			return this.InvokeHook<TResult>(handle.HookId, buffer, mode, timeoutMs);
		}
		else
		{
			byte[] poolBuffer = ArrayPool<byte>.Shared.Rent(size);
			try
			{
				Span<byte> buffer = poolBuffer.AsSpan(0, size);
				MarshalUtils.SerializeArgs(buffer, args);

				// In this case, the placeholder result is discarded
				// If you need the result, use AddCall directly.
				if (s_currentBatch != null)
				{
					s_currentBatch.AddCall<TResult>(handle.HookId, buffer);
					return default;
				}

				return this.InvokeHook<TResult>(handle.HookId, buffer, mode, timeoutMs);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(poolBuffer);
			}
		}
	}

	/// <summary>
	/// Invokes the registered wrapper hook synchronously.
	/// The original function will be called by the remote controller.
	/// </summary>
	/// <typeparam name="TResult">
	/// The expected return type of the hooked function.
	/// </typeparam>
	/// <param name="hookId">
	/// The ID of the registered hook.
	/// </param>
	/// <param name="argsPayload">
	/// The timeout in milliseconds for the invocation to return.
	/// </param>
	/// <param name="mode">
	/// The context in which to dispatch the hook invocation.
	/// See <see cref="DispatchMode"/> for more details.
	/// </param>
	/// <param name="timeoutMs">
	/// The serialized arguments to pass to the hooked function.
	/// </param>
	/// <returns>
	/// A typed result from the hooked function, or the default value of <c>TResult</c>
	/// if the invocation failed or timed out.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// Thrown if the provided hook handle is invalid.
	/// </exception>
	public TResult? InvokeHook<TResult>(uint hookId, ReadOnlySpan<byte> argsPayload, DispatchMode mode = DispatchMode.Immediate, int timeoutMs = IPC_TIMEOUT_MS)
		where TResult : unmanaged
	{
		// In this case, the placeholder result is discarded
		// If you need the result, use AddCall directly.
		if (s_currentBatch != null)
		{
			s_currentBatch.AddCall<TResult>(hookId, argsPayload);
			return default;
		}

		return MarshalUtils.Deserialize<TResult>(this.InvokeHookRaw(hookId, argsPayload, mode, timeoutMs) ?? []);
	}

	/// <summary>
	/// Creates a new batch invoke scope for grouping multiple hook invocations.
	/// </summary>
	/// <returns>
	/// A new <see cref="BatchInvokeScope"/> instance for batching hook invocations.
	/// </returns>
	/// <remarks>
	/// The intent is to use the returned scope within a <c>using</c> statement to ensure proper disposal.
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// Thrown if a batch invoke scope is already active on the current thread.
	/// </exception>
	public BatchInvokeScope CreateBatchInvoke()
	{
		if (s_currentBatch != null)
			throw new InvalidOperationException("A batch invoke scope is already active on this thread.");

		s_currentBatch = new BatchInvokeScope(this);
		return s_currentBatch;
	}

	/// <summary>
	/// Registers a function wrapper around a native function.
	/// </summary>
	/// <typeparam name="TDelegate">
	/// The delegate type representing the function signature to hook.
	/// </typeparam>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds for the registration to complete.
	/// </param>
	/// <returns>
	/// Returns a <see cref="HookHandle"/> if the
	/// registration was successful; otherwise, <c>null</c>.
	/// </returns>
	public HookHandle? RegisterWrapper<TDelegate>(int timeoutMs = IPC_REGISTER_TIMEOUT_MS)
		where TDelegate : Delegate
	{
		// Hook behavior here doesn't matter as we ignore it
		return this.RegisterHook<TDelegate>(HookType.Wrapper, HookBehavior.Before, null, timeoutMs);
	}

	/// <summary>
	/// Registers a function interceptor hook.
	/// </summary>
	/// <typeparam name="TDelegate">
	/// The delegate type representing the function signature to hook.
	/// </typeparam>
	/// <param name="behavior">
	/// The behavior of the interceptor (Before, After, Replace).
	/// </param>
	/// <param name="handler">
	/// A function that will be called as part of the hook detour.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds for the registration to complete.
	/// </param>
	/// <returns>
	/// Returns a <see cref="HookHandle"/> if the
	/// registration was successful; otherwise, <c>null</c>.
	/// </returns>
	public HookHandle? RegisterInterceptor<TDelegate>(
		HookBehavior behavior,
		Func<ReadOnlySpan<byte>, byte[]> handler,
		int timeoutMs = IPC_REGISTER_TIMEOUT_MS)
		where TDelegate : Delegate
	{
		return this.RegisterHook<TDelegate>(HookType.Interceptor, behavior, handler, timeoutMs);
	}

	/// <summary>
	/// Unregisters a previously registered hook.
	/// </summary>
	/// <param name="handle">
	/// The handle of the registered hook to unregister.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds for the unregistration to complete.
	/// </param>
	/// <returns>
	/// True if the unregistration was successful; otherwise, false.
	/// </returns>
	/// <remarks>
	/// Hooks are unregistered automatically when the <see cref="HookHandle"/> is
	/// disposed or when <see cref="ControllerService.Shutdown"/> is called.
	/// </remarks>
	public bool UnregisterHook(HookHandle handle, int timeoutMs = IPC_TIMEOUT_MS)
	{
		if (!handle.IsValid || !this.isConnected)
			return false;

		return this.UnregisterHookById(handle.HookId, timeoutMs);
	}

	internal static void ClearActiveBatch()
	{
		s_currentBatch = null;
	}

	internal void UnregisterHookInternal(uint hookId)
	{
		if (!this.isConnected)
			return;

		this.UnregisterHookById(hookId, IPC_TIMEOUT_MS);
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();

		try
		{
			this.outgoingEndpoint = new Endpoint(BUF_SHMEM_OUTGOING, BUF_BLK_COUNT, BUF_BLK_SIZE);
			this.incomingEndpoint = new Endpoint(BUF_SHMEM_INCOMING, BUF_BLK_COUNT, BUF_BLK_SIZE);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to initialize IPC endpoints.");
			this.outgoingEndpoint?.Dispose();
			this.outgoingEndpoint = null;
			this.incomingEndpoint?.Dispose();
			this.incomingEndpoint = null;
			throw;
		}

		_ = Task.Factory.StartNew(
			() => this.ConnectionMonitorLoop(this.CancellationToken),
			this.CancellationToken,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

		this.BackgroundTask = Task.Factory.StartNew(
			() => this.ProcessIncomingMessages(this.CancellationToken),
			this.CancellationToken,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

		await base.OnStart();
	}

	private static void ProcessHookRequestInternal((ControllerService Ctrl, uint MsgId, byte[] Data, int Length) state)
	{
		try
		{
			state.Ctrl.HandleHookRequest(state.MsgId, state.Data.AsSpan(0, state.Length));
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(state.Data);
		}
	}

	/// <summary>
	/// An encapsulation of the IPC request registration logic.
	/// </summary>
	/// <param name="handle">
	/// The handle of the hook to register.
	/// </param>
	/// <param name="data">
	/// The registration data for the hook.
	/// </param>
	/// <param name="handler">
	/// The optional handler function for interceptors.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds for the registration to complete.
	/// </param>
	/// <returns>
	/// True if the registration was successful; otherwise, false.
	/// </returns>
	private bool RegisterHookIPC(
		HookHandle handle,
		HookRegistrationData data,
		Func<ReadOnlySpan<byte>, byte[]>? handler,
		int timeoutMs)
	{
		if (this.outgoingEndpoint == null)
			return false;

		byte[] payload = MarshalUtils.Serialize(data);
		uint requestId = this.GetNextRequestId();

		using var pending = new PendingRequest<uint>();
		this.pendingRegistrations[requestId] = pending;
		try
		{
			var header = new MessageHeader(requestId, PayloadType.Register, (ulong)payload.Length);

			if (!this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS))
			{
				Log.Error($"Failed to send hook registration request for: {handle.DelegateKey}");
				return false;
			}

			if (!pending.Wait(timeoutMs))
			{
				Log.Error($"Hook registration timed out for: {handle.DelegateKey}");
				return false;
			}

			if (!pending.TryGetResult(out uint hookId) || hookId == 0)
			{
				Log.Error($"Hook registration failed for: {handle.DelegateKey}");
				return false;
			}

			if (handler != null)
			{
				this.handlers[hookId] = handler;
			}

			handle.SetHookId(hookId);
			this.delegateKeyToHookId[handle.DelegateKey] = hookId;

			Log.Information($"Registered hook[ID: {hookId}] for: {handle.DelegateKey}");
			return true;
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Exception during IPC registration for {handle.DelegateKey}");
			return false;
		}
		finally
		{
			this.pendingRegistrations.TryRemove(requestId, out _);
		}
	}

	private HookHandle? RegisterHook<TDelegate>(
		HookType hookType,
		HookBehavior behavior,
		Func<ReadOnlySpan<byte>, byte[]>? handler = null,
		int timeoutMs = IPC_REGISTER_TIMEOUT_MS)
		where TDelegate : Delegate
	{
		if (this.outgoingEndpoint == null)
			throw new InvalidOperationException("Controller service not initialized.");

		string delegateKey = HookUtils.GetKey(typeof(TDelegate));

		if (this.delegateKeyToHookId.ContainsKey(delegateKey))
		{
			Log.Warning($"Hook already registered for: {delegateKey}");
			return null;
		}

		var registerPayload = new HookRegistrationData
		{
			HookType = hookType,
			HookBehavior = behavior,
			DelegateKeyLength = delegateKey.Length,
		};
		registerPayload.SetKey(delegateKey);

		var handle = new HookHandle(this, 0, delegateKey);
		var pending = new HookRegistrationInfo(handle, registerPayload, handler);
		this.allHooks[delegateKey] = pending;

		if (this.isConnected)
		{
			if (this.RegisterHookIPC(handle, registerPayload, handler, timeoutMs))
				return handle;

			return null; // Registration failed
		}
		else
		{
			Log.Information($"Remote controller not available. Queueing hook[Key: {delegateKey}] for later registration.");
			this.registrationQueue.Enqueue(pending);
			return handle;
		}
	}

	private bool UnregisterHookById(uint hookId, int timeoutMs)
	{
		if (this.outgoingEndpoint == null)
			return false;

		using var pending = new PendingRequest<bool>();
		this.pendingUnregistrations[hookId] = pending;

		try
		{
			var header = new MessageHeader(hookId, PayloadType.Unregister);
			if (!this.outgoingEndpoint.Write(header, IPC_TIMEOUT_MS))
			{
				Log.Error($"Failed to send hook unregistration request for hook ID: {hookId}");
				return false;
			}

			if (!pending.Wait(timeoutMs))
			{
				Log.Error($"Hook unregistration timed out for hook ID: {hookId}");
				return false;
			}

			if (!pending.TryGetResult(out bool success))
			{
				return false;
			}

			if (success)
			{
				this.handlers.TryRemove(hookId, out _);

				var keyToRemove = this.delegateKeyToHookId.FirstOrDefault(kvp => kvp.Value == hookId).Key;
				if (keyToRemove != null)
				{
					this.allHooks.TryRemove(keyToRemove, out _);
					this.delegateKeyToHookId.TryRemove(keyToRemove, out _);
				}

				Log.Information($"Unregistered hook[ID: {hookId}].");
			}

			return success;
		}
		finally
		{
			this.pendingUnregistrations.TryRemove(hookId, out _);
		}
	}

	private void InvalidateAllHooks()
	{
		foreach (var kvp in this.allHooks)
		{
			var handle = kvp.Value.Handle;
			if (handle.IsValid)
			{
				handle.SetHookId(0);
				Log.Debug($"Invalidated hook[Key: {handle.DelegateKey}].");
			}
		}

		this.delegateKeyToHookId.Clear();
	}

	private void RequeueAllInvalidatedHooks()
	{
		foreach (var kvp in this.allHooks)
		{
			var regInfo = kvp.Value;
			if (regInfo.Handle.HookId == 0 && !this.registrationQueue.Contains(regInfo))
			{
				this.registrationQueue.Enqueue(regInfo);
				Log.Debug($"Re-queued previously invalidated hook[Key: {regInfo.Handle.DelegateKey}].");
			}
		}
	}

	private Task SendPendingRegistrations()
	{
		// Process the entire queue
		while (this.registrationQueue.TryPeek(out _))
		{
			if (!this.registrationQueue.TryDequeue(out var pendingItem))
				break;

			// Skip already registered hooks
			if (pendingItem.Handle.HookId != 0)
				continue;

			// Perform registration synchronously on this thread
			bool success = this.RegisterHookIPC(
				pendingItem.Handle,
				pendingItem.Data,
				pendingItem.Handler,
				IPC_REGISTER_TIMEOUT_MS);

			if (!success)
			{
				Log.Error($"Queued registration failed for {pendingItem.Handle.DelegateKey}");
			}
		}

		return Task.CompletedTask;
	}

	private async Task ConnectionMonitorLoop(CancellationToken ct)
	{
		Log.Information("Started task to track remote controller availability.");

		while (!ct.IsCancellationRequested)
		{
			try
			{
				if (!GameService.Injected)
				{
					if (this.isConnected)
					{
						// NOTE: The heartbeat timer is stopped not to flood the ring buffer
						// while the remote controller is not available.
						Log.Warning("Connection lost with remote controller.");
						this.heartbeatTimer?.Dispose();
						this.heartbeatTimer = null;

						// Deactivate system hooks
						if (this.framework != null)
						{
							this.framework.Active = false;
						}

						this.isConnected = false;
						this.InvalidateAllHooks();
					}

					await Task.Delay(CONN_CHECK_DELAY_MS, ct);
					continue;
				}

				if (!this.isConnected)
				{
					Log.Information("Connection established with remote controller.");
					this.RequeueAllInvalidatedHooks();
					await this.SendPendingRegistrations();
					this.heartbeatTimer ??= new Timer(this.SendHeartbeat, null, HEARTBEAT_INTERVAL_MS, HEARTBEAT_INTERVAL_MS);

					// Register system hooks
					if (this.framework != null)
					{
						this.framework.Active = true;
						this.RegisterFrameworkHook();
					}

					this.isConnected = true;
				}

				await Task.Delay(CONN_CHECK_DELAY_MS, ct);
			}
			catch (TaskCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error in connection handshake.");
			}
		}
	}

	private void ProcessIncomingMessages(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			if (this.incomingEndpoint == null)
				return;

			try
			{
				while (this.incomingEndpoint.Read(out MessageHeader header, out ReadOnlySpan<byte> payload, IPC_TIMEOUT_MS))
				{
					switch (header.Type)
					{
						case PayloadType.Ack:
							this.HandleAck(header.Id, payload);
							break;

						case PayloadType.NAck:
							this.HandleNAck(header.Id);
							break;

						case PayloadType.Request when payload.Length > 0:
							this.EnqueueHookRequest(header.Id, payload);
							break;

						case PayloadType.Blob:
							this.HandleHookReturn(header.Id, payload);
							break;

						default:
							Log.Warning($"Unexpected message type from controller: {header.Type}");
							break;
					}
				}
			}
			catch (NullReferenceException ex)
			{
				Log.Warning(ex, "IPC endpoint closed unexpectedly (shared memory was disposed). Stopping message processing.");
				this.CancellationTokenSource?.Cancel();
			}
			catch (ObjectDisposedException ex)
			{
				Log.Warning(ex, "IPC endpoint disposed. Stopping message processing.");
				this.CancellationTokenSource?.Cancel();
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop
				break;
			}
		}
	}

	private void SendHeartbeat(object? state)
	{
		try
		{
			if (this.outgoingEndpoint != null && this.IsInitialized)
			{
				var heartbeatHeader = new MessageHeader(type: PayloadType.Heartbeat);
				this.outgoingEndpoint.Write(heartbeatHeader, IPC_TIMEOUT_MS);
			}
		}
		catch (Exception ex)
		{
			Log.Verbose(ex, "Failed to send heartbeat to remote controller.");
		}
	}

	private void HandleAck(uint msgId, ReadOnlySpan<byte> payload)
	{
		if (this.pendingRegistrations.TryGetValue(msgId, out var regPending))
		{
			if (payload.Length >= sizeof(uint))
			{
				uint hookId = BitConverter.ToUInt32(payload);
				regPending.SetResult(hookId);
			}
			else
			{
				Log.Warning($"Registration ACK missing hookId payload for request: {msgId}");
				regPending.SetResult(0);
			}

			return;
		}

		if (this.pendingUnregistrations.TryGetValue(msgId, out var unregPending))
		{
			unregPending.SetResult(true);
			return;
		}

		if (msgId == HookMessageId.GOODBYE_MESSAGE_ID)
		{
			this.pendingByeMessage.SetResult(true);
			return;
		}
	}

	private void HandleNAck(uint msgId)
	{
		if (this.pendingRegistrations.TryGetValue(msgId, out var regPending))
		{
			regPending.SetResult(0); // 0 indicates failure
			return;
		}

		if (this.pendingUnregistrations.TryGetValue(msgId, out var unregPending))
		{
			unregPending.SetResult(false);
			return;
		}

		if (msgId == HookMessageId.GOODBYE_MESSAGE_ID)
		{
			this.pendingByeMessage.SetResult(false);
			return;
		}
	}

	private void HandleHookReturn(uint msgId, ReadOnlySpan<byte> resultData)
	{
		if (this.pendingHookRequests.TryGetValue(msgId, out var pending))
		{
			pending.SetResult(resultData.ToArray());
		}
		else
		{
			Log.Warning($"Received hook result for unknown request: {msgId}");
		}
	}

	private void EnqueueHookRequest(uint msgId, ReadOnlySpan<byte> payload)
	{
		byte[] payloadCopy = ArrayPool<byte>.Shared.Rent(payload.Length);
		int length = payload.Length;
		payload.CopyTo(payloadCopy);

		var workItem = this.workItemPool.Get();
		unsafe
		{
			workItem.Initialize(this.workItemPool, &ProcessHookRequestInternal, (this, msgId, payloadCopy, length));
		}

		this.workPipeline.Enqueue(workItem);
	}

	private void HandleHookRequest(uint msgId, ReadOnlySpan<byte> payload)
	{
		uint hookId = HookMessageId.GetHookId(msgId);
		byte[] response;

		if (this.handlers.TryGetValue(hookId, out var handler))
		{
			try
			{
				response = handler(payload);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error in before handler for hook {hookId}");
				response = [];
			}
		}
		else
		{
			Log.Warning($"No handler registered for hook request: {hookId}");
			response = [];
		}

		this.SendResponse(msgId, response);
	}

	private void SendResponse(uint msgId, byte[] response)
	{
		if (this.outgoingEndpoint == null)
			return;

		var header = new MessageHeader(msgId, PayloadType.Blob, (ulong)response.Length);
		this.outgoingEndpoint.Write(header, response, IPC_TIMEOUT_MS);
	}

	private void RegisterFrameworkHook()
	{
		if (this.outgoingEndpoint == null)
			throw new InvalidOperationException("Controller service not initialized.");

		Type delType = typeof(RemoteController.Interop.Delegates.Framework.Tick);
		string delegateKey = HookUtils.GetKey(delType);

		if (this.delegateKeyToHookId.ContainsKey(delegateKey))
		{
			Log.Warning($"Hook already registered for: {delegateKey}");
			return;
		}

		var registerPayload = new HookRegistrationData
		{
			HookType = HookType.System,
			HookBehavior = HookBehavior.After, // Ignored
			DelegateKeyLength = delegateKey.Length,
			HookId = HookMessageId.FRAMEWORK_SYSTEM_ID,
		};
		registerPayload.SetKey(delegateKey);

		byte[] payload = MarshalUtils.Serialize(registerPayload);
		uint requestId = this.GetNextRequestId();

		using var pending = new PendingRequest<uint>();
		this.pendingRegistrations[requestId] = pending;
		try
		{
			var header = new MessageHeader(requestId, PayloadType.Register, (ulong)payload.Length);

			if (!this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS))
			{
				Log.Error($"Failed to send hook registration request for: {delegateKey}");
				return;
			}

			if (!pending.Wait(IPC_REGISTER_TIMEOUT_MS))
			{
				Log.Error($"Hook registration timed out for: {delegateKey}");
				return;
			}

			if (!pending.TryGetResult(out uint hookId) || hookId == 0)
			{
				Log.Error($"Hook registration failed for: {delegateKey}");
				return;
			}

			// Payload: [1] = Keep Syncing, [0] = Stop Syncing
			this.handlers[HookMessageId.FRAMEWORK_SYSTEM_ID] = _ =>
			{
				if (this.framework == null)
					return [0];

				bool keepSyncing = this.framework?.ProcessTick() ?? false;
				return [keepSyncing ? (byte)1 : (byte)0];
			};

			this.delegateKeyToHookId[delegateKey] = hookId;
			Log.Information($"Registered hook[ID: {hookId}] for: {delegateKey}");
			return;
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Exception during IPC registration for {delegateKey}");
			return;
		}
		finally
		{
			this.pendingRegistrations.TryRemove(requestId, out _);
		}
	}

	private void SendFrameworkSyncRequest()
	{
		if (this.outgoingEndpoint == null)
			return;

		var data = new FrameworkMessageData { Type = FrameworkMessageType.EnableTickSync };
		int payloadSize = Unsafe.SizeOf<FrameworkMessageData>();
		var header = new MessageHeader(HookMessageId.FRAMEWORK_SYSTEM_ID, PayloadType.Request, (ulong)payloadSize);

		byte seq = this.GetNextSequence(HookMessageId.FRAMEWORK_SYSTEM_ID);
		uint msgId = HookMessageId.Pack(HookMessageId.FRAMEWORK_SYSTEM_ID, seq);
		var pending = new PendingRequest<byte[]>();
		this.pendingHookRequests[msgId] = pending;

		byte[]? rented = null;
		bool sent;

		try
		{
			if (payloadSize <= STACKALLOC_THRESHOLD)
			{
				Span<byte> payload = stackalloc byte[payloadSize];
				MarshalUtils.Write(payload, in data);
				sent = this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS);
			}
			else
			{
				rented = ArrayPool<byte>.Shared.Rent(payloadSize);
				Span<byte> rentedSpan = rented.AsSpan(0, payloadSize);
				MarshalUtils.Write(rentedSpan, in data);
				sent = this.outgoingEndpoint.Write(header, rentedSpan, IPC_TIMEOUT_MS);
			}

			if (!sent)
			{
				Log.Warning("Failed to send framework sync request");
				this.pendingHookRequests.TryRemove(msgId, out _);
			}
		}
		finally
		{
			if (rented != null)
			{
				ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	private uint GetNextRequestId()
	{
		return Interlocked.Increment(ref this.nextRequestId);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private byte GetNextSequence(uint hookIndex)
	{
		return this.sequenceCounters.AddOrUpdate(hookIndex, 1, s_incrementFunc);
	}

	private void SendShutdownMessage()
	{
		if (this.outgoingEndpoint == null)
			return;

		this.pendingByeMessage.Reset();
		var header = new MessageHeader(HookMessageId.GOODBYE_MESSAGE_ID, type: PayloadType.Bye);
		this.outgoingEndpoint.Write(header, IPC_TIMEOUT_MS);

		if (this.pendingByeMessage.Wait(IPC_TIMEOUT_MS))
			Log.Information("Remote controller acknowledged shutdown message.");
		else
			Log.Warning("No acknowledgment received for shutdown message.");

		// Regardless of the result, mark as disconnected
		// This will avoid hooks from attempting to use the IPC during disposal
		this.isConnected = false;

		if (this.pendingByeMessage.TryGetResult(out bool success) && success)
			Log.Information("Remote controller shutdown completed successfully.");
		else
			Log.Warning("Remote controller shutdown reported failure.");
	}

	private record HookRegistrationInfo(
		HookHandle Handle,
		HookRegistrationData Data,
		Func<ReadOnlySpan<byte>, byte[]>? Handler);
}

/// <summary>
/// Initializes a new instance of the <see cref="FrameworkService"/> class.
/// This service enables the scheduling and synchronously execution of actions
/// and functions in relation to the game framework's main loop.
/// </summary>
/// <remarks>
/// While the tasks are executed synchronously, they do not run on the
/// framework thread itself. Avoid the usage of the framework service
/// for native functions calls that internally use the Thread Local Storage (TLS).
/// </remarks>
/// <param name="queue">
/// The internal work queue used for task processing.
/// </param>
/// <param name="sendSyncRequest">
/// An action to trigger a syncrhonization request with the remote
/// controller's framework driver.
/// </param>
public class FrameworkService(WorkQueue queue, Action sendSyncRequest)
{
	private static readonly TimeSpan s_frameBudget = TimeSpan.FromMilliseconds(1);

	private readonly WorkQueue workQueue = queue;
	private readonly List<ConditionalTask> conditionalTasks = new();
	private readonly Action sendSyncRequest = sendSyncRequest;

	private bool active = false;

	private Header header;

	/// <summary>
	/// Gets a value indicating whether the framework service is active.
	/// </summary>
	public bool Active
	{
		get => this.active;
		internal set
		{
			this.workQueue.Enabled = value;
			this.active = value;
		}
	}

	/// <summary>
	/// Executes an action within the next available framework tick.
	/// </summary>
	public Task RunOnTick(Action action)
	{
		if (!this.Active)
		{
			Log.Warning("The framework service is inactive. Dropping action.");
			return Task.CompletedTask;
		}

		var task = this.workQueue.Enqueue(action);
		this.SignalWorkAdded();
		return task;
	}

	/// <summary>
	/// Delays execution for a specific time, then runs on framework tick.
	/// </summary>
	/// <param name="delay">
	/// A time span to wait before executing the action.
	/// </param>
	/// <param name="action">
	/// The action to execute after the delay.
	/// </param>
	/// <returns>
	/// A task that completes once the action has been executed.
	/// </returns>
	public async Task RunOnTick(TimeSpan delay, Action action)
	{
		if (!this.Active)
		{
			Log.Warning("The framework service is inactive. Dropping action.");
			return;
		}

		// NOTE: We wait locally to avoid blocking the framework thread
		await Task.Delay(delay);
		await this.RunOnTick(action);
	}

	/// <summary>
	/// Runs an action after a specific number of framework ticks have passed.
	/// </summary>
	/// <param name="ticks">
	/// The number of framework ticks to wait before executing the action.
	/// </param>
	/// <param name="action">
	/// The action to execute after the specified number of ticks.
	/// </param>
	public void RunAfterTicks(int ticks, Action action)
	{
		if (!this.Active)
		{
			Log.Warning("The framework service is inactive. Dropping action.");
			return;
		}

		lock (this.conditionalTasks)
		{
			long currentTick = this.header.TickCount;
			long targetTick = currentTick + ticks;
			this.conditionalTasks.Add(new(null, action, targetTick, currentTick, -1));
		}

		this.SignalWorkAdded();
	}

	/// <summary>
	/// Executes an action within the context of the framework tick
	/// once the provided condition evaluates to true.
	/// </summary>
	/// <param name="condition">
	/// The condition to evaluate on each tick.
	/// </param>
	/// <param name="deferTicks">
	/// The number of framework ticks to wait before checking the
	/// condition and running the action.
	/// </param>
	/// <param name="timeoutTicks">
	/// The maximum number of framework ticks to run the condition check
	/// for before giving up.
	/// </param>
	/// <param name="action">
	/// The action to execute once the condition is met.
	/// </param>
	/// <remarks>
	/// The framework service will check the condition on each tick until
	/// it evaluates to true. Ensure that the condition will eventually be
	/// met to avoid indefinite tick processing.
	/// </remarks>
	public void RunOnTickUntil(Func<bool> condition, int deferTicks = 0, int timeoutTicks = -1, Action? action = null)
	{
		if (!this.Active)
		{
			Log.Warning("The framework service is inactive. Dropping action.");
			return;
		}

		lock (this.conditionalTasks)
		{
			long currentTick = this.header.TickCount;
			long targetTick = currentTick + deferTicks;
			this.conditionalTasks.Add(new(condition, action ?? (() => { }), targetTick, currentTick, timeoutTicks));
		}

		this.SignalWorkAdded();
	}

	/// <summary>
	/// Executes an action within the context of the framework tick
	/// once the provided condition evaluates to true.
	/// </summary>
	/// <param name="condition">
	/// The condition to evaluate on each tick.
	/// </param>
	/// <param name="deferTicks">
	/// The number of framework ticks to wait before checking the
	/// condition and running the action.
	/// </param>
	/// <param name="timeoutTicks">
	/// The maximum number of framework ticks to run the condition check
	/// for before giving up.
	/// </param>
	/// <param name="action">
	/// The action to execute once the condition is met.
	/// </param>
	/// <returns>
	/// A task that completes once the action has been executed.
	/// </returns>
	/// <remarks>
	/// This is a variant of <see cref="RunOnTickUntil(Func{bool}, int, int, Action)"/>
	/// that provides a <see cref="Task"/> to await completion.
	/// </remarks>
	public Task RunOnTickUntilAsync(Func<bool> condition, int deferTicks = 0, int timeoutTicks = -1, Action? action = null)
	{
		if (!this.Active)
		{
			Log.Warning("The framework service is inactive. Dropping action.");
			return Task.CompletedTask;
		}

		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		lock (this.conditionalTasks)
		{
			long currentTick = this.header.TickCount;
			long targetTick = currentTick + deferTicks;
			this.conditionalTasks.Add(
				new ConditionalTask(
					condition,
					() =>
					{
						try
						{
							action?.Invoke();
							tcs.TrySetResult();
						}
						catch (Exception ex)
						{
							tcs.TrySetException(ex);
						}
					},
					targetTick,
					currentTick,
					timeoutTicks,
					tcs));
		}

		this.SignalWorkAdded();
		return tcs.Task;
	}

	/// <summary>
	/// Process pending tasks within the context of the framework tick.
	/// </summary>
	/// <returns>
	/// True if we need to keep hooking the next frame; False if we are idle.
	/// </returns>
	/// <remarks>
	/// This method is intended to be called by <see cref="ControllerService"/>.
	/// </remarks>
	internal bool ProcessTick()
	{
		this.header.TickCount++;
		bool remainingWork = false;

		lock (this.conditionalTasks)
		{
			for (int i = this.conditionalTasks.Count - 1; i >= 0; i--)
			{
				var task = this.conditionalTasks[i];

				if (this.header.TickCount - task.TriggerTick < 0)
				{
					remainingWork = true;
					continue;
				}

				try
				{
					bool isReady = task.Condition == null || task.Condition();

					if (isReady)
					{
						this.conditionalTasks.RemoveAt(i);
						task.Action();
						continue;
					}

					// Check timeout
					if (task.TimeoutFrames >= 0 && (this.header.TickCount - task.EnqueueTick) >= task.TimeoutFrames)
					{
						this.conditionalTasks.RemoveAt(i);
						task.Tcs?.TrySetCanceled();
						continue;
					}

					remainingWork = true;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error in framework service conditional task.");
					task.Tcs?.TrySetException(ex);
					this.conditionalTasks.RemoveAt(i);
				}
			}

			// If we still have conditions to check, keep syncing with framework tick
			remainingWork |= this.conditionalTasks.Count > 0;
		}

		remainingWork |= this.workQueue.ProcessPending(s_frameBudget);
		if (remainingWork)
			return true;

		// Mark as idle for now
		Interlocked.Exchange(ref this.header.LoopState, 0);

		if (this.HasWork())
		{
			Interlocked.Exchange(ref this.header.LoopState, 1);
			return true;
		}

		return false;
	}

	private bool HasWork()
	{
		if (!this.workQueue.IsEmpty)
			return true;

		lock (this.conditionalTasks)
		{
			return this.conditionalTasks.Count > 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SignalWorkAdded()
	{
		if (Interlocked.Exchange(ref this.header.LoopState, 1) == 0)
		{
			this.sendSyncRequest();
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct Header
	{
		[FieldOffset(0)] public long TickCount;
		[FieldOffset(64)] public int LoopState; // 0 = Idle, 1 = Active
	}

	private record struct ConditionalTask(Func<bool>? Condition, Action Action, long TriggerTick, long EnqueueTick, int TimeoutFrames, TaskCompletionSource? Tcs = null);
}
