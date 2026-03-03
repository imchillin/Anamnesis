// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.IPC;

using Serilog;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// An interface for a queueable work item.
/// Designed for use with <see cref="WorkPipeline"/>.
/// </summary>
public interface IWorkItem
{
	void Execute();
	void Cleanup();
}

/// <summary>
/// A base implementation of a work item with state.
/// </summary>
/// <typeparam name="TState">
/// The state object that is passed to the work item's function.
/// </typeparam>
public unsafe sealed class WorkItem<TState> : IWorkItem
{
	private ObjectPool<WorkItem<TState>>? pool;
	private delegate* managed<TState, void> logicPtr;
	private TState state = default!;

	/// <summary>
	/// Configures the work item prior to enqueueing into the work pipeline.
	/// </summary>
	/// <param name="pool">
	/// A reference to the object pool that this work item was retrieved from.
	/// </param>
	/// <param name="logicPtr">
	/// A pointer to the logic function for this work item.
	/// </param>
	/// <param name="state">
	/// A state object that will be passed to the work item's function.
	/// </param>
	public void Initialize(
		ObjectPool<WorkItem<TState>> pool,
		delegate* managed<TState, void> logicPtr,
		TState state)
	{
		this.pool = pool;
		this.logicPtr = logicPtr;
		this.state = state;
	}

	/// <summary>
	/// Executes the work item's logic function.
	/// </summary>
	/// <remarks>
	/// Manual invocation of this method is not desired.
	/// Use <see cref="WorkPipeline"/> to manage work item execution.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Execute()
	{
#if DEBUG
    if (this.logicPtr == null)
        throw new InvalidOperationException("Work item logic pointer is null. Was Cleanup() called?");
#endif

		this.logicPtr(this.state);
	}

	/// <summary>
	/// Cleans up the work item after execution and returns it to the pool.
	/// </summary>
	/// <remarks>
	/// Manual invocation of this method is not desired.
	/// Use <see cref="WorkPipeline"/> to manage work item lifetimes.
	/// </remarks>
	public void Cleanup()
	{
		this.logicPtr = null;
		this.state = default!;
		this.pool?.Return(this);
	}
}

/// <summary>
/// A pipeline for processing work items asynchronously with a pool of worker tasks.
/// </summary>
public sealed class WorkPipeline : IDisposable
{
	private readonly Channel<IWorkItem> channel;
	private readonly ChannelWriter<IWorkItem> writer;
	private readonly Task[] workers;
	private readonly CancellationTokenSource cts = new();
	private bool isDisposed = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkPipeline"/> class.
	/// </summary>
	/// <param name="workers">
	/// The number of worker tasks to spawn for processing work items.
	/// </param>
	public WorkPipeline(int workers = 2)
	{
		this.channel = Channel.CreateUnbounded<IWorkItem>(new UnboundedChannelOptions
		{
			SingleReader = workers == 1,
			SingleWriter = false,
			AllowSynchronousContinuations = false,
		});
		this.writer = this.channel.Writer;

		this.workers = new Task[workers];
		for (int i = 0; i < workers; i++)
		{
			this.workers[i] = Task.Factory.StartNew(this.ProcessLoop, TaskCreationOptions.LongRunning);
		}
	}

	/// <summary>
	/// Enqueues a work item for processing by the pipeline.
	/// </summary>
	/// <param name="work">
	/// The work item to enqueue.
	/// </param>
	public void Enqueue(IWorkItem work) => this.writer.TryWrite(work);

	/// <inheritdoc/>
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private async Task ProcessLoop()
	{
		var reader = this.channel.Reader;
		while (await reader.WaitToReadAsync(this.cts.Token))
		{
			while (reader.TryRead(out var work))
			{
				try
				{
					work.Execute();
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Work execution failed");
				}
				finally
				{
					work.Cleanup();
				}
			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (this.isDisposed)
			return;

		if (disposing)
		{
			this.cts.Cancel();
			this.writer.TryComplete();
			Task.WaitAll(this.workers);
			this.cts.Dispose();
		}

		this.isDisposed = true;
	}
}
