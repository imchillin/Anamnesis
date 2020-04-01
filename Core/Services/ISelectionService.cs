// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using ConceptMatrix.Offsets;

	public delegate void SelectionEvent(Selection selection);

	public interface ISelectionService : IService
	{
		event SelectionEvent SelectionChanged;

		Selection CurrentSelection
		{
			get;
		}
	}

	public class Selection
	{
		public readonly BaseAddresses BaseAddress;
		public readonly string ActorId;

		public Selection(Types type, BaseAddresses address, string actorId, string name)
		{
			this.Type = type;
			this.BaseAddress = address;
			this.ActorId = actorId;
			this.Name = name;
		}

		public enum Types
		{
			Character,
		}

		public Types Type { get; private set; }

		public string Name { get; private set; }
	}
}
