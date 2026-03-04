// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
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

		byte[]? response = this.service.InvokeHookRaw(MessageId.BATCH_HOOK_ID, finalBuffer, this.Mode, this.TimeoutMs);

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
/// Represents a handle to an event subscription to a remote controller event.
/// </summary>
public sealed class EventSubscription : IDisposable
{
	private readonly ControllerService service;
	private readonly EventId eventId;
	private readonly Action<ReadOnlySpan<byte>> wrappedHandler;
	private bool isDisposed;

	internal EventSubscription(ControllerService service, EventId eventId, Action<ReadOnlySpan<byte>> wrappedHandler)
	{
		this.service = service;
		this.eventId = eventId;
		this.wrappedHandler = wrappedHandler;
	}

	/// <summary>
	/// Gets the event ID this subscription is for.
	/// </summary>
	public EventId EventId => this.eventId;

	/// <summary>
	/// Gets a value indicating whether this subscription is still active.
	/// </summary>
	public bool IsActive => !this.isDisposed;

	/// <summary>
	/// Unsubscribes from the event.
	/// </summary>
	public void Dispose()
	{
		if (this.isDisposed)
			return;

		this.service.UnsubscribeEventInternal(this.eventId, this.wrappedHandler);
		this.isDisposed = true;
	}
}

/// <summary>
/// A service that communicates with the remote controller that we inject into the game process.
/// The service is responsible for sending and receiving messages to and from the controller, including
/// function hook communication, driver commands, and events, driver commands.
/// </summary>
public class ControllerService : ServiceBase<ControllerService>
{
	internal const int IPC_TIMEOUT_MS = 100;

	private const string BUF_SHMEM_OUTGOING = "Local\\ANAM_SHMEM_MAIN_TO_CTRL";
	private const string BUF_SHMEM_INCOMING = "Local\\ANAM_SHMEM_CTRL_TO_MAIN";
	private const uint BUF_BLK_COUNT = 128;
	private const ulong BUF_BLK_SIZE = 8192;

	private const int THREAD_JOIN_TIMEOUT_MS = 500;
	private const int IPC_REGISTER_TIMEOUT_MS = 1000;
	private const int STACKALLOC_THRESHOLD = 256;
	private const int CONN_CHECK_DELAY_MS = 1000;

	private static readonly Func<uint, byte, byte> s_incrementFunc = static (_, v) => (byte)((v + 1) & 0xFF);

	[ThreadStatic]
	private static BatchInvokeScope? s_currentBatch;

	private readonly ConcurrentDictionary<string, HookRegistrationInfo> allHooks = new();
	private readonly ConcurrentQueue<HookRegistrationInfo> registrationQueue = new();
	private readonly ConcurrentDictionary<uint, Func<ReadOnlySpan<byte>, byte[]>> handlers = new();
	private readonly ConcurrentDictionary<uint, ushort> sequenceCounters = new();
	private readonly ConcurrentDictionary<string, uint> delegateKeyToHookId = new();
	private readonly ObjectPool<PendingRequest<byte[]>> msgRequestPool = new(maxSize: 256);
	private readonly ConcurrentDictionary<uint, PendingRequest<byte[]>> pendingMsgRequests = new();

	private readonly ConcurrentDictionary<EventId, HashSet<WeakReference<Action<ReadOnlySpan<byte>>>>> eventHandlers = new();
	private readonly Lock eventHandlersLock = new();

	private readonly ObjectPool<WorkItem<(ControllerService, uint, byte[], int)>> workItemPool = new(maxSize: 128);

	private readonly WorkQueue frameworkQueue = new();
	private FrameworkService? framework;

	private WorkPipeline? eventPipeline;
	private WorkPipeline? workPipeline;

	private Endpoint? outgoingEndpoint = null;
	private Endpoint? incomingEndpoint = null;
	private bool isConnected = false;
	private Thread? ipcMsgListener = null;

	/// <summary>
	/// Gets the framework service used for executing
	/// work on the game process' framework tick.
	/// </summary>
	public FrameworkService Framework => this.framework
		?? throw new InvalidOperationException("Framework is not initialized");

	/// <summary>
	/// Gets a value indicating whether the controller service is currently connected to the remote controller.
	/// </summary>
	public bool IsConnected => this.isConnected;

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
		this.isConnected = false;

		// IMPORTANT: Shut down the background task first to stop the incoming message processing loop
		// This should happen before we start disposing the object the loop depends on.
		await base.Shutdown();

		if (this.ipcMsgListener != null)
		{
			if (!this.ipcMsgListener.Join(THREAD_JOIN_TIMEOUT_MS))
				Log.Warning("Controller service message listener thread did not exit within the timeout period.");

			this.ipcMsgListener = null;
		}

		this.framework?.CancelPendingWork();

		this.workPipeline?.Dispose();
		this.workPipeline = null;
		this.eventPipeline?.Dispose();
		this.eventPipeline = null;

		// NOTE: We invalidate hooks but do not dispose of them here.
		// They can be used for re-initialization on follow-up controller service startups.
		this.InvalidateAllHooks();

		foreach (var kvp in this.pendingMsgRequests)
			kvp.Value.Dispose();

		// NOTE: No need to unsubscribe from events since the remote controller will
		// automatically clean up all subscriptions as part of its own shutdown process

		this.pendingMsgRequests.Clear();

		this.outgoingEndpoint?.Dispose();
		this.outgoingEndpoint = null;
		this.incomingEndpoint?.Dispose();
		this.incomingEndpoint = null;
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

		ushort seq = this.GetNextSequence(hookId);
		uint msgId = MessageId.Pack(hookId, seq);

		int argsLength = argsPayload.Length + 1;
		Span<byte> packedPayload = stackalloc byte[argsLength];
		packedPayload[0] = (byte)mode;
		argsPayload.CopyTo(packedPayload[1..]);

		var pending = this.msgRequestPool.Get();
		this.pendingMsgRequests[msgId] = pending;

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
			this.pendingMsgRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.msgRequestPool.Return(pending);
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

	/// <summary>
	/// Sends a driver command and returns a typed result.
	/// </summary>
	/// <typeparam name="TResult">The expected return type.</typeparam>
	/// <param name="command">The driver command.</param>
	/// <param name="timeout">The timeout in milliseconds for the command to complete.</param>
	/// <param name="args">Arguments to serialize and send.</param>
	/// <returns>The deserialized result, or null if the call failed.</returns>
	public TResult? SendDriverCommand<TResult>(DriverCommand command, int timeout = IPC_TIMEOUT_MS, params object[] args)
		where TResult : unmanaged
	{
		byte[] response = this.SendDriverCommandRaw(command, timeout, args: args);
		if (response.Length < Unsafe.SizeOf<TResult>())
			return null;

		return MarshalUtils.Deserialize<TResult>(response);
	}

	/// <summary>
	/// Sends a raw driver command with serialized arguments.
	/// </summary>
	/// <param name="command">The driver command.</param>
	/// <param name="timeout">The timeout in milliseconds for the command to complete.</param>
	/// <param name="args">Arguments to serialize.</param>
	/// <returns>Raw response bytes, or empty array on failure.</returns>
	public byte[] SendDriverCommandRaw(DriverCommand command, int timeout = IPC_TIMEOUT_MS, params object[] args)
	{
		if (this.outgoingEndpoint == null || !this.isConnected)
			return [];

		int argsSize = MarshalUtils.ComputeArgsSize(args);
		int payloadSize = sizeof(int) + argsSize;

		Span<byte> payload = payloadSize <= 256
			? stackalloc byte[payloadSize]
			: new byte[payloadSize];

		MarshalUtils.Write(payload, (int)command);
		if (argsSize > 0)
			MarshalUtils.SerializeArgs(payload[sizeof(int)..], args);

		return this.SendDriverCommandInternal(payload, timeout);
	}

	/// <summary>
	/// Sends a raw driver command with serialized arguments.
	/// </summary>
	/// <param name="command">The driver command.</param>
	/// <param name="serializedArgs">Arguments that have already been serialized to a byte array.</param>
	/// <param name="timeout">The timeout in milliseconds for the command to complete.</param>
	/// <returns>Raw response bytes, or empty array on failure.</returns>
	public byte[] SendDriverCommandRaw(DriverCommand command, ReadOnlySpan<byte> serializedArgs, int timeout = IPC_TIMEOUT_MS)
	{
		if (this.outgoingEndpoint == null || !this.isConnected)
			return [];

		int payloadSize = sizeof(int) + serializedArgs.Length;

		Span<byte> payload = payloadSize <= 256
			? stackalloc byte[payloadSize]
			: new byte[payloadSize];

		MarshalUtils.Write(payload, (int)command);
		serializedArgs.CopyTo(payload[sizeof(int)..]);

		return this.SendDriverCommandInternal(payload, timeout);
	}

	/// <summary>
	/// Subscribes to an event from the remote controller.
	/// </summary>
	/// <remarks>
	/// It is possible to subscribe multiple handlers to the same
	/// event, but their relative order of execution on trigger is not guaranteed.
	/// </remarks>
	/// <typeparam name="T">The event payload type.</typeparam>
	/// <param name="eventId">The event to subscribe to.</param>
	/// <param name="handler">The handler to invoke when the event is received.</param>
	/// <param name="timeoutMs">Timeout for the subscription acknowledgment.</param>
	/// <returns>
	/// An <see cref="EventSubscription"/> handle that can be disposed to unsubscribe,
	/// or <c>null</c> if the subscription failed.
	/// </returns>
	public EventSubscription? SubscribeEvent<T>(EventId eventId, Action<T> handler, int timeoutMs = IPC_TIMEOUT_MS)
		where T : unmanaged
	{
		void WrappedHandler(ReadOnlySpan<byte> payload)
		{
			if (payload.Length < Unsafe.SizeOf<T>())
			{
				Log.Warning($"Received event {eventId} with unexpected payload size. Expected at least {Unsafe.SizeOf<T>()} bytes, got {payload.Length} bytes. Corrupted payload?");
				return;
			}

			var data = MarshalUtils.Deserialize<T>(payload);
			handler(data);
		}

		return this.SubscribeEventInternal(eventId, WrappedHandler, timeoutMs);
	}

	/// <summary>
	/// Subscribes to an event that carries no payload data.
	/// </summary>
	/// <param name="eventId">The event to subscribe to.</param>
	/// <param name="handler">The handler to invoke when the event is received.</param>
	/// <param name="timeoutMs">Timeout for the subscription acknowledgment.</param>
	/// <returns>
	/// An <see cref="EventSubscription"/> handle that can be disposed to unsubscribe,
	/// or <c>null</c> if the subscription failed.
	/// </returns>
	public EventSubscription? SubscribeEvent(EventId eventId, Action handler, int timeoutMs = IPC_TIMEOUT_MS)
	{
#pragma warning disable SA1313
		void WrappedHandler(ReadOnlySpan<byte> _) => handler();
#pragma warning restore SA1313

		return this.SubscribeEventInternal(eventId, WrappedHandler, timeoutMs);
	}

	/// <summary>
	/// Unsubscribes all handlers from an event.
	/// </summary>
	/// <param name="eventId">The event to unsubscribe from.</param>
	/// <param name="timeoutMs">Timeout for the unsubscription acknowledgment.</param>
	/// <returns>True if unsubscription was successful.</returns>
	public bool UnsubscribeAllFromEvent(EventId eventId, int timeoutMs = IPC_TIMEOUT_MS)
	{
		int handlerCount;
		lock (this.eventHandlersLock)
		{
			if (!this.eventHandlers.TryRemove(eventId, out var handlers))
				return true;

			handlerCount = handlers.Count;
			handlers.Clear();
		}

		if (!this.isConnected || handlerCount == 0)
			return true;

		return this.SendEventSubscription(eventId, PayloadType.EventUnsubscribe, timeoutMs, EventSubscriptionFlags.UnsubscribeAll);
	}

	/// <summary>
	/// Sends a configuration set command to the remote controller.
	/// </summary>
	/// <param name="configId">The target configuration identifier.</param>
	/// <param name="value">The value to set for the configuration.</param>
	/// <param name="timeout">The timeout in milliseconds for the command to complete.</param>
	/// <returns>
	/// True if the command was sent successfully and acknowledged by the remote controller; otherwise, false.
	/// </returns>
	public bool SendConfigSet<T>(ConfigIdentifier configId, T value, int timeout = IPC_TIMEOUT_MS)
		where T : unmanaged
	{
		if (this.outgoingEndpoint == null || !this.isConnected)
			return false;

		int valueSize = Unsafe.SizeOf<T>();
		int payloadSize = sizeof(ConfigIdentifier) + valueSize;

		Span<byte> payload = stackalloc byte[payloadSize];
		MarshalUtils.Write(payload, (ConfigIdentifier)configId);
		MarshalUtils.Write(payload[sizeof(ConfigIdentifier)..], value);

		ushort seq = this.GetNextSequence(MessageId.CONFIG_COMMAND_ID);
		uint msgId = MessageId.Pack(MessageId.CONFIG_COMMAND_ID, seq);

		var pending = this.msgRequestPool.Get();
		this.pendingMsgRequests[msgId] = pending;

		try
		{
			var header = new MessageHeader(msgId, PayloadType.Config, (ulong)payload.Length);
			if (!this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS))
				return false;

			if (!pending.Wait(timeout))
				return false;

			if (!pending.TryGetResult(out byte[]? result) || result == null || result.Length == 0)
				return false;

			return result.Length == 1 && result[0] == 1;
		}
		finally
		{
			this.pendingMsgRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.msgRequestPool.Return(pending);
		}
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

	internal void UnsubscribeEventInternal(EventId eventId, Action<ReadOnlySpan<byte>> wrappedHandler)
	{
		int removedCount = 0;
		lock (this.eventHandlersLock)
		{
			if (!this.eventHandlers.TryGetValue(eventId, out var handlers))
				return;

			removedCount = handlers.RemoveWhere(wr =>
			{
				return wr.TryGetTarget(out var target) && target == wrappedHandler;
			});

			if (handlers.Count == 0)
				this.eventHandlers.TryRemove(eventId, out _);
		}

		if (removedCount > 0 && this.isConnected)
		{
			this.SendEventSubscription(eventId, PayloadType.EventUnsubscribe, IPC_TIMEOUT_MS);
		}
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

		this.eventPipeline = new WorkPipeline(1); // Just one worker to keep event order sequential
		this.workPipeline = new WorkPipeline(Math.Max(Environment.ProcessorCount / 2, 1));

		// If hooks exist (e.g. on game restart), requeue them for registration
		if (!this.allHooks.IsEmpty)
			this.RequeueAllInvalidatedHooks();

		_ = Task.Factory.StartNew(
			() => this.ConnectionMonitorLoop(this.CancellationToken),
			this.CancellationToken,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

		this.ipcMsgListener = new Thread(() => this.ProcessIncomingMessages(this.CancellationToken))
		{
			IsBackground = true,
			Priority = ThreadPriority.Highest,
			Name = "Anamnesis.CtrlIpcMsgListener",
		};
		this.ipcMsgListener.Start();

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

	private static void ProcessEventInternal((ControllerService Ctrl, uint EventId, byte[] Data, int Length) state)
	{
		try
		{
			state.Ctrl.BroadcastEventToSubscribers((EventId)state.EventId, state.Data.AsSpan(0, state.Length));
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
		ushort seq = this.GetNextSequence(MessageId.HOOK_REGISTRATION_ID);
		uint msgId = MessageId.Pack(MessageId.HOOK_REGISTRATION_ID, seq);

		var pending = this.msgRequestPool.Get();
		this.pendingMsgRequests[msgId] = pending;

		try
		{
			var header = new MessageHeader(msgId, PayloadType.Register, (ulong)payload.Length);

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

			if (!pending.TryGetResult(out byte[]? result) || result == null || result.Length < 4)
			{
				Log.Error($"Hook registration failed for key {handle.DelegateKey}: Invalid payload");
				return false;
			}

			uint hookId = MarshalUtils.Read<uint>(result);
			if (hookId == 0)
			{
				Log.Error($"Hook registration failed for key {handle.DelegateKey}: Invalid hook index");
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
			this.pendingMsgRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.msgRequestPool.Return(pending);
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

		string delegateKey = MessageUtils.GetKey(typeof(TDelegate));

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

		ushort seq = this.GetNextSequence(MessageId.HOOK_DEREGISTRATION_ID);
		uint msgId = MessageId.Pack(MessageId.HOOK_DEREGISTRATION_ID, seq);

		var pending = this.msgRequestPool.Get();
		this.pendingMsgRequests[msgId] = pending;

		try
		{
			Span<byte> payload = stackalloc byte[sizeof(uint)];
			MarshalUtils.Write(payload, hookId);

			var header = new MessageHeader(hookId, PayloadType.Unregister, (ulong)payload.Length);
			if (!this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS))
			{
				Log.Error($"Failed to send hook unregistration request for hook ID: {hookId}");
				return false;
			}

			if (!pending.Wait(timeoutMs))
			{
				Log.Error($"Hook unregistration timed out for hook ID: {hookId}");
				return false;
			}

			if (!pending.TryGetResult(out byte[]? result) || result == null || result.Length == 0)
				return false;

			bool success = result[0] == 1;
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
			this.pendingMsgRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.msgRequestPool.Return(pending);
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

	private byte[] SendDriverCommandInternal(ReadOnlySpan<byte> payload, int timeout = IPC_TIMEOUT_MS)
	{
#if DEBUG
		if (payload.Length < sizeof(DriverCommand))
			throw new ArgumentException("Driver command payload must start with a serialized DriverCommand enum value.");
#endif

		ushort seq = this.GetNextSequence(MessageId.DRIVER_COMMAND_ID);
		uint msgId = MessageId.Pack(MessageId.DRIVER_COMMAND_ID, seq);

		var pending = this.msgRequestPool.Get();
		this.pendingMsgRequests[msgId] = pending;

		try
		{
			var header = new MessageHeader(msgId, PayloadType.Command, (ulong)payload.Length);
			if (!this.outgoingEndpoint!.Write(header, payload, IPC_TIMEOUT_MS))
				return [];

			if (!pending.Wait(timeout))
				return [];

			if (!pending.TryGetResult(out byte[]? result) || result == null)
				return [];

			return result;
		}
		finally
		{
			this.pendingMsgRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.msgRequestPool.Return(pending);
		}
	}

	private EventSubscription? SubscribeEventInternal(EventId eventId, Action<ReadOnlySpan<byte>> wrappedHandler, int timeoutMs)
	{
		lock (this.eventHandlersLock)
		{
			if (!this.eventHandlers.TryGetValue(eventId, out var handlers))
			{
				handlers = [];
				this.eventHandlers[eventId] = handlers;
			}

			handlers.Add(new WeakReference<Action<ReadOnlySpan<byte>>>(wrappedHandler));
		}

		if (this.isConnected)
		{
			if (!this.SendEventSubscription(eventId, PayloadType.EventSubscribe, timeoutMs))
			{
				lock (this.eventHandlersLock)
				{
					if (this.eventHandlers.TryGetValue(eventId, out var handlers))
						handlers.RemoveWhere(wr => wr.TryGetTarget(out var target) && target == wrappedHandler);
				}

				return null;
			}
		}

		return new EventSubscription(this, eventId, wrappedHandler);
	}

	private bool SendEventSubscription(EventId eventId, PayloadType type, int timeoutMs, EventSubscriptionFlags flags = EventSubscriptionFlags.None)
	{
		if (this.outgoingEndpoint == null)
			return false;

		uint baseId = type switch
		{
			PayloadType.EventSubscribe => MessageId.EVENT_SUBSCRIBE_ID,
			PayloadType.EventUnsubscribe => MessageId.EVENT_UNSUBSCRIBE_ID,
			_ => throw new ArgumentException("Invalid payload type provided for event (un)subscription.", nameof(type)),
		};

		ushort seq = this.GetNextSequence(baseId);
		uint msgId = MessageId.Pack(baseId, seq);

		var data = new EventSubscriptionData { EventId = eventId, Flags = flags };
		int payloadSize = Unsafe.SizeOf<EventSubscriptionData>();
		Span<byte> payload = stackalloc byte[payloadSize];
		MarshalUtils.Write(payload, in data);

		var pending = this.msgRequestPool.Get();
		this.pendingMsgRequests[msgId] = pending;

		try
		{
			var header = new MessageHeader(msgId, type, (ulong)payloadSize);
			if (!this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS))
				return false;

			if (!pending.Wait(timeoutMs))
				return false;

			if (!pending.TryGetResult(out byte[]? result) || result == null || result.Length == 0)
				return false;

			return result[0] == 1;
		}
		finally
		{
			this.pendingMsgRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.msgRequestPool.Return(pending);
		}
	}

	private void ResendActiveEventSubscriptions()
	{
		List<(EventId EventId, int Count)> subscriptions;
		lock (this.eventHandlersLock)
		{
			subscriptions = this.eventHandlers
				.Where(kvp => kvp.Value.Count > 0)
				.Select(kvp => (kvp.Key, kvp.Value.Count))
				.ToList();
		}

		foreach (var (eventId, count) in subscriptions)
		{
			// Send one subscription per handler to restore correct refcount
			for (int i = 0; i < count; ++i)
			{
				if (!this.SendEventSubscription(eventId, PayloadType.EventSubscribe, IPC_REGISTER_TIMEOUT_MS))
				{
					Log.Warning($"Failed to re-subscribe to event {eventId} (subscription {i + 1}/{count}) after reconnection.");
				}
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
						Log.Warning("Connection lost with remote controller.");

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

					// Register system hooks
					if (this.framework != null)
					{
						this.framework.Active = true;
						this.handlers[MessageId.FRAMEWORK_SYNC_COMMAND_ID] = _ =>
						{
							if (this.framework == null)
								return [0];

							bool keepSyncing = this.framework?.ProcessTick() ?? false;
							return [keepSyncing ? (byte)1 : (byte)0];
						};
					}

					this.ResendActiveEventSubscriptions();
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

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private void ProcessIncomingMessages(CancellationToken cancellationToken)
	{
		uint taskIndex = 0;
		IntPtr avrtHandle = NativeFunctions.AvSetMmThreadCharacteristics("Games", ref taskIndex);

		try
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

							case PayloadType.Event:
								this.EnqueueEvent(header.Id, payload);
								break;

							case PayloadType.Request:
								this.EnqueueHookRequest(header.Id, payload);
								break;

							case PayloadType.Config:
								this.HandleConfigGetRequest(header.Id, payload);
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
		finally
		{
			if (avrtHandle != IntPtr.Zero)
				NativeFunctions.AvRevertMmThreadCharacteristics(avrtHandle);
		}
	}

	private void HandleAck(uint msgId, ReadOnlySpan<byte> payload)
	{
		if (this.pendingMsgRequests.TryGetValue(msgId, out var pending))
		{
			pending.SetResult(payload.ToArray());
			return;
		}
	}

	private void HandleNAck(uint msgId)
	{
		if (this.pendingMsgRequests.TryGetValue(msgId, out var pending))
		{
			pending.SetResult([]); // Failure
			return;
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

		this.workPipeline?.Enqueue(workItem);
	}

	private void EnqueueEvent(uint eventIdRaw, ReadOnlySpan<byte> payload)
	{
		EventId eventId = (EventId)eventIdRaw;
		lock (this.eventHandlersLock)
		{
			if (!this.eventHandlers.TryGetValue(eventId, out var handlers) || handlers.Count == 0)
				return; // Subscribers may have unsubscribed before the event signal; Abort
		}

		byte[] payloadCopy = ArrayPool<byte>.Shared.Rent(payload.Length);
		int length = payload.Length;
		payload.CopyTo(payloadCopy);

		var workItem = this.workItemPool.Get();
		unsafe
		{
			workItem.Initialize(this.workItemPool, &ProcessEventInternal, (this, eventIdRaw, payloadCopy, length));
		}

		this.eventPipeline?.Enqueue(workItem);
	}

	private void BroadcastEventToSubscribers(EventId eventId, ReadOnlySpan<byte> payload)
	{
		List<Action<ReadOnlySpan<byte>>> handlersSnapshot = [];
		lock (this.eventHandlersLock)
		{
			if (!this.eventHandlers.TryGetValue(eventId, out var handlers) || handlers.Count == 0)
				return; // Subscribers may have unsubscribed since enqueue; Abort

			var deadRefs = new List<WeakReference<Action<ReadOnlySpan<byte>>>>();
			foreach (var wr in handlers)
			{
				if (wr.TryGetTarget(out var target))
					handlersSnapshot.Add(target);
				else
					deadRefs.Add(wr);
			}

			foreach (var dead in deadRefs)
				handlers.Remove(dead);
		}

		foreach (var handler in handlersSnapshot)
		{
			try
			{
				handler(payload);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error in event handler for {eventId}");
			}
		}
	}

	private void HandleHookRequest(uint msgId, ReadOnlySpan<byte> payload)
	{
		uint hookId = MessageId.GetEmbeddedId(msgId);
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

		this.SendResponse(msgId, response, PayloadType.Ack);
	}

	private void HandleConfigGetRequest(uint msgId, ReadOnlySpan<byte> payload)
	{
		if (payload.Length < sizeof(ConfigIdentifier))
		{
			this.SendResponse(msgId, PayloadType.NAck);
			return;
		}

		byte[] responseData = [];
		ConfigIdentifier configId = MarshalUtils.Read<ConfigIdentifier>(payload);
		switch (configId)
		{
			case ConfigIdentifier.FpsLimiter:
				responseData = [(byte)(SettingsService.Current.OverrideFpsLimiter ? 1 : 0)];
				break;

			default:
				Log.Warning($"Received unknown config get request: {configId}");
				this.SendResponse(msgId, PayloadType.NAck);
				return;
		}

		this.SendResponse(msgId, responseData, PayloadType.Ack);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SendResponse(uint msgId, PayloadType responseType = PayloadType.Blob)
	{
		return this.SendResponse(msgId, default, responseType);
	}

	private bool SendResponse(uint msgId, ReadOnlySpan<byte> payload, PayloadType responseType = PayloadType.NAck)
	{
		if (this.outgoingEndpoint == null)
			return false;

		var header = new MessageHeader(msgId, responseType, (ulong)payload.Length);
		return this.outgoingEndpoint.Write(header, payload, IPC_TIMEOUT_MS);
	}

	private void SendFrameworkSyncRequest()
	{
		if (this.outgoingEndpoint == null)
			return;

		ushort seq = this.GetNextSequence(MessageId.FRAMEWORK_SYNC_COMMAND_ID);
		uint msgId = MessageId.Pack(MessageId.FRAMEWORK_SYNC_COMMAND_ID, seq);

		if (!this.SendResponse(msgId, PayloadType.Request))
		{
			Log.Warning("Failed to send framework sync request");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ushort GetNextSequence(uint hookIndex)
	{
		return (ushort)this.sequenceCounters.AddOrUpdate(hookIndex, 1, (_, v) => (ushort)((v + 1) & MessageId.MAX_SEQ_NUM));
	}

	private void SendShutdownMessage()
	{
		if (this.outgoingEndpoint == null)
			return;

		var header = new MessageHeader(0, type: PayloadType.Bye);
		if (!this.outgoingEndpoint.Write(header, IPC_TIMEOUT_MS))
		{
			Log.Warning("Failed to send shutdown message to remote controller.");
		}
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
/// An action to trigger a synchronization request with the remote
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

	/// <summary>
	/// Cancels any pending work and resets the framework service to an idle state.
	/// </summary>
	internal void CancelPendingWork()
	{
		lock (this.conditionalTasks)
		{
			foreach (var task in this.conditionalTasks)
				task.Tcs?.TrySetCanceled();

			this.conditionalTasks.Clear();
		}

		this.workQueue.Clear();
		Interlocked.Exchange(ref this.header.LoopState, 0);
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
