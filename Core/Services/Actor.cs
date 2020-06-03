// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public class Actor
	{
		protected static IInjectionService injection = Services.Get<IInjectionService>();

		private readonly IBaseMemoryOffset baseOffset;

		public Actor(IBaseMemoryOffset baseOffset)
		{
			this.baseOffset = baseOffset;

			this.Name = this.GetValue(Offsets.Main.Name);
			this.Type = this.GetValue(Offsets.Main.ActorType);
			this.Mode = Modes.Overworld;
		}

		public enum Modes
		{
			Overworld,
			GPose,
		}

		public ActorTypes Type { get; private set; }
		public string Name { get; private set; }
		public Modes Mode { get; private set; }

		public IMemory<T> GetMemory<T>(IMemoryOffset<T> offset)
		{
			return injection.GetMemory<T>(this.baseOffset, offset);
		}

		public T GetValue<T>(IMemoryOffset<T> offset)
		{
			using IMemory<T> mem = this.GetMemory(offset);
			return mem.Value;
		}

		/// <summary>
		/// Marks the selected actor to be refreshed after a short delay.
		/// it is safe to call this repeatedly.
		/// </summary>
		public void ActorRefresh()
		{
			IActorRefreshService refreshService = Services.Get<IActorRefreshService>();
			refreshService.Refresh(this);
		}
	}
}
