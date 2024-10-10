// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Keyboard;
using Anamnesis.Memory;
using System.Threading;
using System.Threading.Tasks;

public class HistoryService : ServiceBase<HistoryService>
{
	/// <summary>Lock object for thread synchronization.</summary>
	public readonly object LockObject = new();
	private int isRestoring = 0;
	public delegate void HistoryAppliedEvent();

	public static event HistoryAppliedEvent? OnHistoryApplied;

	public bool IsRestoring
	{
		get => Interlocked.CompareExchange(ref this.isRestoring, 0, 0) == 1;
		private set => Interlocked.Exchange(ref this.isRestoring, value ? 1 : 0);
	}

	public override Task Initialize()
	{
		HotkeyService.RegisterHotkeyHandler("System.Undo", this.StepBack);
		////HotkeyService.RegisterHotkeyHandler("System.Redo", this.StepForward);

		return base.Initialize();
	}

	private void StepBack()
	{
		this.Step(false);
	}

	private void StepForward()
	{
		this.Step(true);
	}

	private void Step(bool forward)
	{
		ActorMemory? actor = TargetService.Instance.SelectedActor;

		if (actor == null)
			return;

		// It's only necessary to gate the IsRestoring property when it's being set to true
		// to ensure we're not changing state while a dependent method is mid-execution.
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
