// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System.Threading.Tasks;

	public delegate void SelectionModeEvent(Modes mode);
	public delegate void SelectionEvent(Actor actor);

	public interface ISelectionService : IService
	{
		event SelectionModeEvent ModeChanged;
		event SelectionEvent ActorSelected;

		Actor? SelectedActor { get; }

		Modes GetMode();
	}

	#pragma warning disable SA1201
	public enum Modes
	{
		Overworld,
		GPose,
	}
}
