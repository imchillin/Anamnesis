// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public delegate void SelectionEvent(Selection selection);

	public interface ISelectionService : IService
	{
		event SelectionEvent SelectionChanged;

		Selection CurrentSelection
		{
			get;
		}

		void ResetSelection();
	}

	public class Selection
	{
		public readonly IBaseMemoryOffset BaseAddress;
		public readonly string ActorId;

		public Selection(ActorTypes type, IBaseMemoryOffset address, string actorId, string name, Modes mode)
		{
			this.Type = type;
			this.BaseAddress = address;
			this.ActorId = actorId;
			this.Name = name;
			this.Mode = mode;
		}

		public enum Modes
		{
			Overworld,
			GPose,
		}

		public ActorTypes Type { get; private set; }

		public string Name { get; private set; }

		public Modes Mode { get; private set; }

		/// <summary>
		/// Marks the selected actor to be refreshed after a short delay.
		/// it is safe to call this repeatedly.
		/// </summary>
		public void ActorRefresh()
		{
			IActorRefreshService refreshService = Services.Get<IActorRefreshService>();
			refreshService.Refresh(this.BaseAddress);
		}
	}
}
