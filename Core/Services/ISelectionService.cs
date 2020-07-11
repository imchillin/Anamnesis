// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public delegate void SelectionModeEvent(Modes mode);
	public delegate void SelectionEvent(Actor actor, bool focus);

	public interface ISelectionService : IService
	{
		event SelectionModeEvent ModeChanged;
		event SelectionEvent ActorSelected;
		Modes GetMode();
	}

	#pragma warning disable SA1201
	public enum Modes
	{
		Overworld,
		GPose,
	}
}
