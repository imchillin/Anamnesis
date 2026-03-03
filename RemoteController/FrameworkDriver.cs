// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController;

using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using RemoteController.Interop.Delegates;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A wrapper of framework object from the target process
/// that drives synchronization with the host application.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public class FrameworkDriver : IDisposable
{
	private const int FRAMEWORK_DEFAULT_TIMEOUT_MS = 1000;

	private static readonly ConcurrentQueue<WorkItem> s_marshaledWork = new();
	private static readonly ConcurrentBag<WorkItem> s_workItemPool = new();

	private readonly IHook<Framework.Tick> tickHook;
	private bool disposedValue = false;
	private volatile int isSyncEnabled = 0;
	private int requestVersion = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="FrameworkDriver"/> class.
	/// </summary>
	/// <param name="tickFuncAddr">
	/// The target function address of the framework tick method to hook.
	/// </param>
	public FrameworkDriver(IntPtr tickFuncAddr)
	{
		this.tickHook = ReloadedHooks.Instance.CreateHook<Framework.Tick>(this.DetourTick, tickFuncAddr);
		this.tickHook.Activate();
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
	/// </summary>
	/// <param name="func">
	/// The function to execute on the framework thread.
	/// </param>
	/// <param name="timeoutMs">
	/// The timeout in milliseconds to wait for the work item to complete.
	/// </param>
	/// <returns></returns>
	/// <exception cref="TimeoutException">
	/// Thrown if the work item does not complete within the specified timeout.
	/// </exception>
	public static byte[] EnqueueAndWait(Func<byte[]> func, int timeoutMs = FRAMEWORK_DEFAULT_TIMEOUT_MS)
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

			throw new TimeoutException("Framework thread did not process wrapper invoke request within timeout.");
		}
		finally
		{
			item.Reset();
			s_workItemPool.Add(item);
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private byte DetourTick(nint fPtr)
	{
		while (s_marshaledWork.TryDequeue(out var work))
		{
			try
			{
				work.Execute();
			}
			finally
			{
				work.Completion.Set();
			}
		}

		// FAST EXIT: If there's no pending work, run the original function directly
		if (this.isSyncEnabled == 0)
			return this.tickHook!.OriginalFunction(fPtr);

		int versionBefore = Volatile.Read(ref this.requestVersion);
		bool continueProcessing = false;

		try
		{
			continueProcessing = Controller.SendFrameworkRequest();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Encountered error during framework sync.");
			Interlocked.Exchange(ref this.isSyncEnabled, 0);
		}

		if (!continueProcessing)
		{
			int versionAfter = Volatile.Read(ref this.requestVersion);
			if (versionBefore == versionAfter)
			{
				Interlocked.CompareExchange(ref this.isSyncEnabled, 0, 1);
			}
		}

		return this.tickHook!.OriginalFunction(fPtr);
	}

	private void Dispose(bool disposing)
	{
		if (!this.disposedValue)
		{
			if (disposing)
			{
				this.tickHook.Disable();
				if (this.tickHook is IDisposable disposable)
				{
					disposable.Dispose();
				}

				while (s_workItemPool.TryTake(out var item))
					item.Dispose();
			}

			this.disposedValue = true;
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
}
