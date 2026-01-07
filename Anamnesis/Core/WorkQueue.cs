// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// A multi-producer, single-consumer work queue.
/// </summary>
public class WorkQueue
{
	private readonly Channel<WorkItem> queue;
	private readonly ChannelReader<WorkItem> reader;
	private readonly ChannelWriter<WorkItem> writer;

	private int isEnabled;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkQueue"/> class.
	/// </summary>
	public WorkQueue()
	{
		var options = new UnboundedChannelOptions
		{
			SingleReader = true,
			SingleWriter = false,
			AllowSynchronousContinuations = false,
		};

		this.queue = Channel.CreateUnbounded<WorkItem>(options);
		this.reader = this.queue.Reader;
		this.writer = this.queue.Writer;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the reader is enabled.
	/// </summary>
	public bool Enabled
	{
		get => Volatile.Read(ref this.isEnabled) != 0;
		set => Interlocked.Exchange(ref this.isEnabled, value ? 1 : 0);
	}

	/// <summary>
	/// Enqueue a work item asynchronously.
	/// </summary>
	/// <param name="action">
	/// The action to enqueue.
	/// </param>
	/// <returns>
	/// True if the item was enqueued successfully; otherwise, false.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Enqueue(Action action) => this.Enabled && this.writer.TryWrite(new WorkItem(action));

	/// <summary>
	/// Enqueue a work item and block until it is completed.
	/// </summary>
	/// <param name="action">
	/// The action to enqueue.
	/// </param>
	/// <returns>
	/// True if the item was enqueued successfully and processed; otherwise, false.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool EnqueueAndWait(Action action)
	{
		if (!this.Enabled)
			return false;

		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		if (this.writer.TryWrite(new WorkItem(action, tcs)))
		{
			tcs.Task.GetAwaiter().GetResult();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Process all pending work items in the queue.
	/// </summary>
	public void ProcessPending()
	{
		if (!this.Enabled)
			return;

		var reader = this.reader;

		// Drain the queue
		while (reader.TryRead(out var item))
		{
			try
			{
				item.Action();
				item.Tcs?.TrySetResult(); // Notify the waiting thread (if any)
			}
			catch (Exception ex)
			{
				// Capture the exception and rethrow it to the waiting thread (if any).
				if (item.Tcs != null)
					item.Tcs.TrySetException(ex);
				else
					throw;
			}
		}
	}

	private readonly struct WorkItem(Action action, TaskCompletionSource? tcs = null)
	{
		public readonly Action Action = action;
		public readonly TaskCompletionSource? Tcs = tcs;
	}
}
