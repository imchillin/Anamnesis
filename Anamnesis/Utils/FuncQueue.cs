// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using System;
using System.Threading.Tasks;

public class FuncQueue(Func<Task> func, int delay)
{
	private readonly int maxDelay = delay;
	private readonly Func<Task> func = func;

	private int delay;
	private Task? task;

	public bool Pending { get; private set; }

	public void Invoke()
	{
		this.delay = this.maxDelay;

		if (this.task == null || this.task.IsCompleted)
		{
			this.task = Task.Run(this.RunTask);
		}
	}

	private async Task RunTask()
	{
		// Double loops to handle case where a refresh delay was added
		// while the refresh was running
		while (this.delay > 0)
		{
			lock (this)
				this.Pending = true;

			while (this.delay > 0)
			{
				await Task.Delay(10);
				this.delay -= 10;
			}

			lock (this)
				this.Pending = false;

			await this.func.Invoke();
		}
	}
}
