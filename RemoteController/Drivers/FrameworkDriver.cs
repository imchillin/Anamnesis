// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController;
using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.IPC;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// A wrapper of framework object from the target process
/// that drives synchronization with the host application.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public class FrameworkDriver : DriverBase<FrameworkDriver>
{
	public const string FRAMEWORK_INSTANCE_SIGNATURE = "48 8B 1D ?? ?? ?? ?? 8B 7C 24";

	private const int FRAMEWORK_DEFAULT_TIMEOUT_MS = 1000;
	private const int CONFIG_SYNC_TIMEOUT_MS = 5000;

	private static readonly ConcurrentQueue<WorkItem> s_marshaledWork = new();
	private static readonly ConcurrentBag<WorkItem> s_workItemPool = new();
	private static readonly TimeSpan s_frameBudget = TimeSpan.FromMilliseconds(1);

	private readonly ConcurrentQueue<ConditionalTask> incomingCondTasks = new();
	private readonly List<ConditionalTask> activeCondTasks = new();

	private readonly FunctionHook<Framework.Tick> tickHook;
	private readonly Framework.Tick detourTick;
	private readonly nint frameworkInstancePtr;

	private volatile int isSyncEnabled = 0;
	private int requestVersion = 0;
	private ulong tickCount = 0;

	private GameConfigModule GameConfig { get; }

	/// <summary>
	/// Event triggered on every frame update of the main game loop.
	/// </summary>
	/// <remarks>
	/// <para>
	/// CAUTION: This runs on the game thread. Keep subscribers lightweight.
	/// </para>
	/// In most cases, use <see cref="RunOnTick(Func{byte[]}, int)"/> or its subvariants
	/// to schedule work on the framework thread instead of subscribing to this event directly.
	/// </remarks>
	public event Action? GameTick;

	/// <summary>
	/// Initializes a new instance of the <see cref="FrameworkDriver"/> class.
	/// </summary>
	public FrameworkDriver()
	{
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize posing driver: signature resolver is not available.");

		this.frameworkInstancePtr = Controller.Scanner?.GetStaticAddressFromSig(FRAMEWORK_INSTANCE_SIGNATURE) ?? nint.Zero;
		if (this.frameworkInstancePtr == nint.Zero)
			throw new InvalidOperationException("Failed to read framework pointer from memory.");

		this.frameworkInstancePtr = Controller.MemoryReader.ReadPtr(this.frameworkInstancePtr);
		if (this.frameworkInstancePtr == nint.Zero)
			throw new InvalidOperationException("Failed to resolve framework instance pointer.");

		Log.Debug($"Framework instance found at address: 0x{this.frameworkInstancePtr:X}");

		this.detourTick = this.DetourTick;
		this.tickHook = HookRegistry.CreateAndActivateHook(this.detourTick);
		this.GameConfig = new GameConfigModule(this, this.frameworkInstancePtr);

		this.RegisterInstance();
	}

	/// <summary>
	/// Gets or sets a value indicating whether framework synchronization is enabled.
	/// </summary>
	/// <remarks>
	/// When this property is set to true, the framework driver will attempt to
	/// synchronize the framework state with the host application on each tick.
	/// </remarks>
	public bool IsSyncEnabled
	{
		get => this.isSyncEnabled == 1;
		set
		{
			if (value)
			{
				Interlocked.Increment(ref this.requestVersion);
			}

			Interlocked.Exchange(ref this.isSyncEnabled, value ? 1 : 0);
		}
	}

	/// <summary>
	/// Enqueues a work item to be executed on the framework thread
	/// and waits for its completion.
	/// </summary>
	/// <param name="func">
	/// The function to execute on the framework thread.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds to wait for the work item to complete.
	/// </param>
	/// <returns>
	/// The result of the function as a byte array.
	/// </returns>
	/// <exception cref="TimeoutException">
	/// Thrown if the work item does not complete within the specified timeout.
	/// </exception>
	public static byte[] RunOnTick(Func<byte[]> func, int timeoutMs = FRAMEWORK_DEFAULT_TIMEOUT_MS)
	{
		if (!s_workItemPool.TryTake(out WorkItem? item))
		{
			item = new WorkItem();
		}

		item.Setup(func);

		try
		{
			s_marshaledWork.Enqueue(item);

			if (item.Completion.Wait(timeoutMs))
			{
				if (item.CapturedException != null)
					throw item.CapturedException;

				return item.Result ?? [];
			}

			throw new TimeoutException("Framework thread timed out processing request.");
		}
		finally
		{
			item.Reset();
			s_workItemPool.Add(item);
		}
	}

	/// <summary>
	/// Schedules an action to be executed on the framework thread asynchronously.
	/// </summary>
	/// <param name="action">
	/// The action to execute on the framework thread.
	/// </param>
	/// <returns>
	/// A task that completes when the action has been executed.
	/// </returns>
	public Task RunOnTickAsync(Action action) => this.RunOnTickUntilAsync(null, 0, -1, action);

	/// <summary>
	/// Schedules an action to be executed on the framework thread asynchronously
	/// after a specified delay.
	/// </summary>
	/// <param name="delay">
	/// The delay before executing the action.
	/// </param>
	/// <param name="action">
	/// The action to execute on the framework thread.
	/// </param>
	/// <returns>
	/// A task that completes when the action has been executed.
	/// </returns>
	public async Task RunOnTickAsync(TimeSpan delay, Action action)
	{
		await Task.Delay(delay);
		await this.RunOnTickAsync(action);
	}

	public void RunAfterTicks(uint ticks, Action action) => this.RunOnTickUntil(null, ticks, -1, action);

	public void RunOnTickUntil(Func<bool>? condition, uint deferTicks = 0, int timeoutTicks = -1, Action? action = null)
	{
		ulong currentTick = Interlocked.Read(ref this.tickCount);
		this.incomingCondTasks.Enqueue(new ConditionalTask(
			condition,
			action ?? (() => { }),
			currentTick + deferTicks,
			currentTick,
			timeoutTicks));
	}

	public Task RunOnTickUntilAsync(Func<bool>? condition, uint deferTicks = 0, int timeoutTicks = -1, Action? action = null)
	{
		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		ulong currentTick = Interlocked.Read(ref this.tickCount);

		this.incomingCondTasks.Enqueue(new ConditionalTask(
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
			currentTick + deferTicks,
			currentTick,
			timeoutTicks,
			tcs));

		return tcs.Task;
	}

	public void SynchronizeConfig()
	{
		// NOTE: As the synchronization is done asynchronously, we do not need a tight deadline
		Task.Run(async () =>
		{
			uint? currentFps = await this.GameConfig.GetUIntAsync("FPSInActive"); // 1 when the limiter is on, 0 when its off
			if (currentFps != null)
			{
				Log.Debug($"Current FPS limiter setting is: {(currentFps == 1u ? "ENABLED" : "DISABLED")}");

				bool overrideLimiter = false;
				if (Controller.GetConfig(ConfigIdentifier.FpsLimiter, ref overrideLimiter, CONFIG_SYNC_TIMEOUT_MS))
				{
					// Disable the FPS limiter if the override setting is toggled on. If toggled off, leave it as is.
					if (overrideLimiter)
					{
						this.SetFpsLimiterEnabled(!overrideLimiter, "SYNC");
					}
				}
				else
				{
					Log.Warning($"Failed to synchronize inactive FPS limiter setting with main application.");
				}
			}
		});

	}

	public async void SetFpsLimiterEnabled(bool enabled, string? source = null)
	{
		string prefix = source != null ? $"[{source}] " : string.Empty;

		uint targetValue = enabled ? 1u : 0u;
		uint? currentValue = await this.GameConfig.GetUIntAsync("FPSInActive");
		if (currentValue == targetValue)
		{
			Log.Debug($"{prefix}FPS limiter already {(enabled ? "enabled" : "disabled")}, no change needed.");
			return;
		}

		try
		{
			await this.GameConfig.SetUIntAsync("FPSInActive", targetValue);
			Log.Information($"{prefix}Successfully {(enabled ? "enabled" : "disabled")} FPS limiter.");
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"{prefix}Failed to {(enabled ? "enable" : "disable")} FPS limiter.");
		}
	}

	/// <inheritdoc/>
	protected override void OnDispose()
	{
		this.tickHook.Dispose();
		while (s_workItemPool.TryTake(out var item))
			item.Dispose();

		s_workItemPool.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ProcessMarshaledWork(long startTimestamp)
	{
		while (s_marshaledWork.TryDequeue(out var work))
		{
			try { work.Execute(); }
			finally { work.Completion.Set(); }

			if (Stopwatch.GetElapsedTime(startTimestamp) >= s_frameBudget)
				break;
		}
	}

	private byte DetourTick(nint fPtr)
	{
		long startTimestamp = Stopwatch.GetTimestamp();
		ulong currentTick = Interlocked.Increment(ref this.tickCount);

		this.DrainIncomingTasks();
		ProcessMarshaledWork(startTimestamp);
		this.ProcessConditionalTasks(startTimestamp, currentTick);

		try
		{
			this.GameTick?.Invoke();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Encountered error during framework game tick event invocation.");
		}

		if (this.isSyncEnabled == 1)
		{
			this.ProcessRemoteSync();
		}

		return this.tickHook!.OriginalFunction(fPtr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DrainIncomingTasks()
	{
		while (this.incomingCondTasks.TryDequeue(out var newTask))
		{
			this.activeCondTasks.Add(newTask);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessConditionalTasks(long startTimestamp, ulong currentTick)
	{
		for (int i = 0; i < this.activeCondTasks.Count;)
		{
			if (Stopwatch.GetElapsedTime(startTimestamp) >= s_frameBudget)
				break;

			var task = this.activeCondTasks[i];

			if (currentTick >= task.TargetTick)
			{
				try
				{
					if (task.Condition == null || task.Condition())
					{
						this.activeCondTasks.RemoveAt(i);
						task.Action();
						continue;
					}
					else if (task.TimeoutFrames >= 0 && (currentTick - task.EnqueueTick) >= (ulong)task.TimeoutFrames)
					{
						this.activeCondTasks.RemoveAt(i);
						task.Tcs?.TrySetCanceled();
						continue;
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error in conditional task.");
					task.Tcs?.TrySetException(ex);
					this.activeCondTasks.RemoveAt(i);
					continue;
				}
			}

			i++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessRemoteSync()
	{
		int versionBefore = Volatile.Read(ref this.requestVersion);
		try
		{
			if (!Controller.SendFrameworkRequest())
			{
				if (versionBefore == Volatile.Read(ref this.requestVersion))
					Interlocked.CompareExchange(ref this.isSyncEnabled, 0, 1);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Framework remote sync error.");
			Interlocked.Exchange(ref this.isSyncEnabled, 0);
		}
	}

	/// <summary>
	/// A wrapper for a work item to be processed on the framework thread.
	/// </summary>
	private sealed class WorkItem : IDisposable
	{
		public ManualResetEventSlim Completion { get; } = new(false);
		public Func<byte[]>? Func { get; private set; }
		public byte[]? Result { get; private set; }
		public Exception? CapturedException { get; private set; }

		public void Setup(Func<byte[]> func)
		{
			this.Func = func;
			this.Completion.Reset();
			this.Result = null;
			this.CapturedException = null;
		}

		public void Execute()
		{
			if (this.Func != null)
			{
				try
				{
					this.Result = this.Func();
				}
				catch (Exception ex)
				{
					this.CapturedException = ex;
				}
			}
		}

		public void Reset()
		{
			this.Func = null;
			this.Result = null;
			this.CapturedException = null;
		}

		public void Dispose()
		{
			this.Completion.Dispose();
		}
	}

	private record struct ConditionalTask(Func<bool>? Condition, Action Action, ulong TargetTick, ulong EnqueueTick, int TimeoutFrames, TaskCompletionSource? Tcs = null);
}
