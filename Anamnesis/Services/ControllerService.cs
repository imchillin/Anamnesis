// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using RemoteController;
using RemoteController.Interop;
using RemoteController.IPC;
using SharedMemoryIPC;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public sealed class HookHandle : IDisposable
{
	private readonly ControllerService service;
	private bool disposed;

	internal HookHandle(ControllerService service, uint hookId, string delegateKey)
	{
		this.service = service;
		this.HookId = hookId;
		this.DelegateKey = delegateKey;
	}

	public uint HookId { get; }
	public string DelegateKey { get; }
	public bool IsValid => this.HookId != 0 && !this.disposed;

	public void Dispose()
	{
		if (this.disposed)
			return;

		this.disposed = true;
		this.service.UnregisterHookInternal(this.HookId);
	}
}

/// <summary>
/// A service that communicates with the remote controller that we inject into the game process.
/// The service is responsible for sending and receiving messages to and from the controller, including
/// watchdog heartbeats, and function hook communication.
/// </summary>
public class ControllerService : ServiceBase<ControllerService>
{
	private const string BUF_SHMEM_OUTGOING = "Local\\ANAM_SHMEM_MAIN_TO_CTRL";
	private const string BUF_SHMEM_INCOMING = "Local\\ANAM_SHMEM_CTRL_TO_MAIN";
	private const uint BUF_BLK_COUNT = 128;
	private const ulong BUF_BLK_SIZE = 8192;

	private const int IPC_TIMEOUT_MS = 100;
	private const int IPC_REGISTER_TIMEOUT_MS = 1000;
	private const int HEARTBEAT_INTERVAL_MS = 15_000;

	private static readonly Func<uint, byte, byte> s_incrementFunc = static (_, v) => (byte)((v + 1) & 0xFF);

	private readonly ConcurrentDictionary<uint, PendingRequest<byte[]>> pendingWrapperRequests = new();
	private readonly ConcurrentDictionary<uint, PendingRequest<uint>> pendingRegistrations = new();
	private readonly ConcurrentDictionary<uint, PendingRequest<bool>> pendingUnregistrations = new();
	private readonly ConcurrentDictionary<uint, Func<byte[], byte[]>> handlers = new();
	private readonly ConcurrentDictionary<uint, byte> sequenceCounters = new();
	private readonly ConcurrentDictionary<string, uint> delegateKeyToHookId = new();
	private readonly ObjectPool<PendingRequest<byte[]>> wrapperRequestPool = new(maxSize: 64);
	private readonly PendingRequest<bool> pendingByeMessage = new();

	private uint nextRequestId = 1;

	private Endpoint? outgoingEndpoint = null;
	private Endpoint? incomingEndpoint = null;
	private Timer? heartbeatTimer;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [GameService.Instance];

	/// <inheritdoc/>
	public override async Task Shutdown()
	{
		this.SendShutdownMessage();

		foreach (var kvp in this.pendingWrapperRequests)
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

		this.pendingWrapperRequests.Clear();
		this.pendingRegistrations.Clear();
		this.pendingUnregistrations.Clear();
		this.outgoingEndpoint?.Dispose();
		this.incomingEndpoint?.Dispose();
		await base.Shutdown();
	}

	public TResult? InvokeHook<TResult>(HookHandle handle, int timeoutMs = IPC_TIMEOUT_MS, params object[] args)
		where TResult : unmanaged
	{
		if (handle == null || !handle.IsValid)
			throw new ArgumentException("Invalid hook handle.", nameof(handle));

		byte[] argsPayload = args.Length switch
		{
			0 => [],
			1 => MarshalUtils.SerializeBoxed(args[0]),
			_ => MarshalUtils.SerializeBoxed(args),
		};

		return this.InvokeHook<Delegate, TResult>(handle.HookId, argsPayload, timeoutMs);
	}

	/// <summary>
	/// Invokes a wrapper hook asynchronously. The remote controller will call the original function.
	/// </summary>
	public TResult? InvokeHook<TDelegate, TResult>(uint hookId, byte[] argsPayload, int timeoutMs = IPC_TIMEOUT_MS)
		where TDelegate : Delegate
		where TResult : unmanaged
	{
		if (this.outgoingEndpoint == null)
			throw new InvalidOperationException("Controller service not initialized.");

		byte seq = this.GetNextSequence(hookId);
		uint msgId = HookMessageId.Pack(hookId, seq);

		var pending = this.wrapperRequestPool.Get();
		this.pendingWrapperRequests[msgId] = pending;

		try
		{
			var header = new MessageHeader(msgId, PayloadType.Request, (ulong)argsPayload.Length);
			try
			{
				if (!this.outgoingEndpoint.Write(header, argsPayload, IPC_TIMEOUT_MS))
				{
					throw new InvalidOperationException("Failed to send wrapper request.");
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

			return MarshalUtils.Deserialize<TResult>(result);
		}
		finally
		{
			this.pendingWrapperRequests.TryRemove(msgId, out _);
			pending.Reset();
			this.wrapperRequestPool.Return(pending);
		}
	}

	// TODO: Cleanup & docs
	public HookHandle? RegisterWrapper<TDelegate>(
		Func<byte[],
		byte[]>? handler = null,
		int timeoutMs = IPC_REGISTER_TIMEOUT_MS)
		where TDelegate : Delegate
	{
			return this.RegisterHook<TDelegate>(
			HookType.Wrapper,
			HookBehavior.Before, // Doesn't matter for wrappers
			handler,
			timeoutMs);
	}

	// TODO: Cleanup & docs
	public HookHandle? RegisterInterceptor<TDelegate>(
		HookBehavior behavior,
		Func<byte[], byte[]> handler,
		int timeoutMs = IPC_REGISTER_TIMEOUT_MS)
		where TDelegate : Delegate
	{
		return this.RegisterHook<TDelegate>(
			HookType.Interceptor,
			behavior,
			handler,
			timeoutMs);
	}

	/// <summary>
	/// Registers a function hook/wrapper.
	/// </summary>
	public HookHandle? RegisterHook<TDelegate>(
		HookType hookType,
		HookBehavior behavior,
		Func<byte[], byte[]>? handler = null,
		int timeoutMs = IPC_REGISTER_TIMEOUT_MS)
		where TDelegate : Delegate
	{
		return Task.Run(() =>
		{
			if (this.outgoingEndpoint == null)
				throw new InvalidOperationException("Controller service not initialized.");

			string delegateKey = HookUtils.GetKey(typeof(TDelegate));

			if (this.delegateKeyToHookId.ContainsKey(delegateKey))
			{
				Log.Warning($"Hook already registered for: {delegateKey}");
				return null;
			}

			if (typeof(TDelegate).GetCustomAttributes(typeof(FunctionBindAttribute), false)
				.FirstOrDefault() is not FunctionBindAttribute attr)
			{
				Log.Error($"Delegate {delegateKey} is not decorated with FunctionBindAttribute");
				return null;
			}

			if (MemoryService.Scanner == null)
				throw new Exception("No memory scanner");

			nint targetAddress = 0;
			try
			{
				if (attr.Offset != 0)
				{
					targetAddress = MemoryService.Scanner.GetStaticAddressFromSig(attr.Signature, attr.Offset);
				}
				else
				{
					targetAddress = MemoryService.Scanner.ScanText(attr.Signature);
				}
			}
			catch (KeyNotFoundException ex)
			{
				Log.Error(ex, $"Failed to resolve signature for: {delegateKey}");
				return null;
			}

			if (targetAddress == 0)
			{
				Log.Error($"Failed to resolve signature for: {delegateKey}");
				return null;
			}

			var registerPayload = new HookRegistrationData
			{
				Address = targetAddress,
				HookType = hookType,
				HookBehavior = behavior,
				DelegateKeyLength = delegateKey.Length,
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
					return null;
				}

				if (!pending.Wait(timeoutMs))
				{
					Log.Error($"Hook registration timed out for: {delegateKey}");
					return null;
				}

				if (!pending.TryGetResult(out uint hookId) || hookId == 0)
				{
					Log.Error($"Hook registration failed for: {delegateKey}");
					return null;
				}

				if (handler != null)
				{
					this.handlers[hookId] = handler;
				}

				this.delegateKeyToHookId[delegateKey] = hookId;

				Log.Information($"Registered hook[ID: {hookId}] for: {delegateKey}");
				return new HookHandle(this, hookId, delegateKey);
			}
			finally
			{
				this.pendingRegistrations.TryRemove(requestId, out _);
			}
		}).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Unregisters a hook by its handle.
	/// </summary>
	public bool UnregisterHook(HookHandle handle, int timeoutMs = IPC_TIMEOUT_MS)
	{
		if (!handle.IsValid)
			return false;

		return this.UnregisterHookById(handle.HookId, timeoutMs);
	}

	internal void UnregisterHookInternal(uint hookId)
	{
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

		this.heartbeatTimer = new Timer(this.SendHeartbeat, null, HEARTBEAT_INTERVAL_MS, HEARTBEAT_INTERVAL_MS);

		this.BackgroundTask = Task.Factory.StartNew(
			() => this.ProcessIncomingMessages(this.CancellationToken),
			this.CancellationToken,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

		await base.OnStart();
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

	private async Task ProcessIncomingMessages(CancellationToken cancellationToken)
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
							byte[] payloadCopy = ArrayPool<byte>.Shared.Rent(payload.Length);
							int length = payload.Length;
							payload.CopyTo(payloadCopy);
							ThreadPool.UnsafeQueueUserWorkItem(
								_ =>
								{
									try
									{
										this.HandleInterceptRequest(header.Id, payloadCopy.AsSpan(0, length));
									}
									finally
									{
										ArrayPool<byte>.Shared.Return(payloadCopy);
									}
								},
								true);
							break;

						case PayloadType.Blob:
							this.HandleWrapperReturn(header.Id, payload);
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

		if (this.pendingByeMessage.TryGetResult(out _))
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

		if (this.pendingByeMessage.TryGetResult(out _))
		{
			this.pendingByeMessage.SetResult(false);
			return;
		}
	}

	private void HandleWrapperReturn(uint msgId, ReadOnlySpan<byte> resultData)
	{
		if (this.pendingWrapperRequests.TryGetValue(msgId, out var pending))
		{
			pending.SetResult(resultData.ToArray());
		}
		else
		{
			Log.Warning($"Received wrapper result for unknown request: {msgId}");
		}
	}

	private void HandleInterceptRequest(uint msgId, ReadOnlySpan<byte> payload)
	{
		uint hookId = HookMessageId.GetHookId(msgId);

		byte[] response;
		if (this.handlers.TryGetValue(hookId, out var handler))
		{
			try
			{
				response = handler(payload.ToArray());
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error in before handler for hook {hookId}");
				response = [];
			}
		}
		else
		{
			response = [];
		}

		this.SendInterceptResponse(msgId, response);
	}

	private void SendInterceptResponse(uint msgId, byte[] response)
	{
		if (this.outgoingEndpoint == null)
			return;

		var header = new MessageHeader(msgId, PayloadType.Blob, (ulong)response.Length);
		this.outgoingEndpoint.Write(header, response, IPC_TIMEOUT_MS);
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
		var header = new MessageHeader(type: PayloadType.Bye);
		this.outgoingEndpoint.Write(header, IPC_TIMEOUT_MS);

		if (this.pendingByeMessage.Wait(IPC_TIMEOUT_MS))
			Log.Information("Remote controller acknowledged shutdown message.");
		else
			Log.Warning("No acknowledgment received for shutdown message.");

		if (this.pendingByeMessage.TryGetResult(out bool success) && success)
			Log.Information("Remote controller shutdown completed successfully.");
		else
			Log.Warning("Remote controller shutdown reported failure.");
	}
}
