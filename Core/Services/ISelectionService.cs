// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Threading.Tasks;

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

		public void ActorRefresh()
		{
			Task.Run(this.ActorRefreshAsync);
		}

		public async Task ActorRefreshAsync()
		{
			using (IMemory<ActorTypes> actorTypeMem = this.BaseAddress.GetMemory(Offsets.ActorType))
			{
				using (IMemory<byte> actorRenderMem = this.BaseAddress.GetMemory(Offsets.ActorRender))
				{
					if (actorTypeMem.Value == ActorTypes.Player)
					{
						actorTypeMem.Value = ActorTypes.BattleNpc;
						actorRenderMem.Value = 2;
						await Task.Delay(50);
						actorRenderMem.Value = 0;
						await Task.Delay(50);
						actorTypeMem.Value = ActorTypes.Player;
					}
					else
					{
						actorRenderMem.Value = 2;
						await Task.Delay(50);
						actorRenderMem.Value = 0;
					}
				}
			}
		}
	}
}
