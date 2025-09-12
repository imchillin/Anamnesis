// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A services that manages history traversal for the currently selected actor.
/// </summary>
public class HistoryService : ServiceBase<HistoryService>
{
	/// <summary>Lock object for thread synchronization.</summary>
	/// <remarks>
	/// Use this object to ensure that any code dependent on the <see cref="IsRestoring"/>
	/// property does not run while history is being restored.
	/// </remarks>
	public readonly Lock LockObject = new();

	private int isRestoring = 0;

	/// <summary>
	/// The delegate object for the <see cref="OnHistoryApplied"/> event.
	/// </summary>
	public delegate void HistoryAppliedEvent();

	/// <summary>
	/// Event that is raised when a history action has finished
	/// applying on the currently selected actor.
	/// </summary>
	public static event HistoryAppliedEvent? OnHistoryApplied;

	/// <summary>
	/// Gets a value indicating whether the history is currently being restored.
	/// </summary>
	/// <remarks>
	/// This is an atomic property so it can be used in multi-threaded contexts.
	/// </remarks>
	public bool IsRestoring
	{
		get => Interlocked.CompareExchange(ref this.isRestoring, 0, 0) == 1;
		private set => Interlocked.Exchange(ref this.isRestoring, value ? 1 : 0);
	}

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [TargetService.Instance];

	/// <summary>
	/// Sets the current history context for the selected actor.
	/// </summary>
	/// <remarks>
	/// The intent of this method is to differentiate between history entries.
	/// This allows us to warn the user if they are trying to undo/redo an action that
	/// doesn't belong to the current context.
	/// </remarks>
	/// <param name="context">The new history context.</param>
	public static void SetContext(HistoryContext context)
	{
		ActorMemory? actor = TargetService.Instance.SelectedActor;
		if (actor is null)
			return;

		actor.History.CurrentContext = context;
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		HotkeyService.RegisterHotkeyHandler("System.Undo", this.StepBack);
		HotkeyService.RegisterHotkeyHandler("System.Redo", this.StepForward);

		await base.Initialize();
	}

	/// <summary>
	/// Performs a step back in the history to the previous history entry (if any).
	/// </summary>
	public void StepBack() => this.Step(false);

	/// <summary>
	/// Performs a step forward in the history to the next history entry (if any).
	/// </summary>
	public void StepForward() => this.Step(true);

	/// <summary>
	/// Clears the selector actor's history.
	/// </summary>
	public void Clear()
	{
		ActorMemory? actor = TargetService.Instance.SelectedActor;

		if (actor is null)
			return;

		lock (this.LockObject)
		{
			actor.History.Clear();
		}
	}

	private void Step(bool forward)
	{
		ActorMemory? actor = TargetService.Instance.SelectedActor;

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
