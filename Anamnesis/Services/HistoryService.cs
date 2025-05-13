// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using System.Threading;
using System.Threading.Tasks;

public class HistoryService : ServiceBase<HistoryService>
{
	/// <summary>Lock object for thread synchronization.</summary>
	/// <remarks>
	/// Use this object to ensure that any code dependent on the <see cref="IsRestoring"/>
	/// property does not run while history is being restored.
	/// </remarks>
	public readonly Lock LockObject = new();
	private int isRestoring = 0;
	public delegate void HistoryAppliedEvent();

	public static event HistoryAppliedEvent? OnHistoryApplied;

	public bool IsRestoring
	{
		get => Interlocked.CompareExchange(ref this.isRestoring, 0, 0) == 1;
		private set => Interlocked.Exchange(ref this.isRestoring, value ? 1 : 0);
	}

	public static void SetContext(HistoryContext context)
	{
		ActorMemory? actor = TargetService.Instance?.SelectedActor;
		if (actor is null)
			return;

		actor.History.CurrentContext = context;
	}

	public override async Task Initialize()
	{
		HotkeyService.RegisterHotkeyHandler("System.Undo", this.StepBack);
		HotkeyService.RegisterHotkeyHandler("System.Redo", this.StepForward);

		await base.Initialize();
	}

	public void StepBack()
	{
		this.Step(false);
	}

	public void StepForward()
	{
		this.Step(true);
	}

	public void Clear()
	{
		ActorMemory? actor = TargetService.Instance?.SelectedActor;

		if (actor is null)
			return;

		lock (this.LockObject)
		{
			actor.History.Clear();
		}
	}

	private void Step(bool forward)
	{
		ActorMemory? actor = TargetService.Instance?.SelectedActor;

		if (actor is null)
			return;

		// Lock the object to prevent any dependent operations from
		// running while we are restoring history.
		lock (this.LockObject)
		{
			this.IsRestoring = true;

			actor.PauseSynchronization = true;

			if (forward)
			{
				actor.History.StepForward();
			}
			else
			{
				actor.History.StepBack();
			}

			actor.WriteDelayedBinds();
			OnHistoryApplied?.Invoke();
			actor.PauseSynchronization = false;
			this.IsRestoring = false;
		}
	}
}
