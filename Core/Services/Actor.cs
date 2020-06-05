// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Collections.Generic;

	public class Actor
	{
		protected static IInjectionService injection = Services.Get<IInjectionService>();

		private IBaseMemoryOffset baseOffset;
		private List<WeakReference<IMemory>> memories = new List<WeakReference<IMemory>>();

		public Actor(IBaseMemoryOffset baseOffset)
		{
			this.baseOffset = baseOffset;

			this.Name = this.GetValue(Offsets.Main.Name);
			this.Type = this.GetValue(Offsets.Main.ActorType);
		}

		public ActorTypes Type { get; private set; }
		public string Name { get; private set; }

		public string Id
		{
			get
			{
				// it would be nice if we had more info than this...
				return this.Name;
			}
		}

		public IMemory<T> GetMemory<T>(IMemoryOffset<T> offset)
		{
			IMemory<T> mem = injection.GetMemory<T>(this.baseOffset, offset);
			this.memories.Add(new WeakReference<IMemory>(mem));
			return mem;
		}

		public T GetValue<T>(IMemoryOffset<T> offset)
		{
			using IMemory<T> mem = this.GetMemory(offset);
			this.memories.Add(new WeakReference<IMemory>(mem));
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

		public void Retarget(Actor actor)
		{
			if (this.baseOffset == actor.baseOffset)
				return;

			this.baseOffset = actor.baseOffset;
			this.Name = actor.Name;
			this.Type = actor.Type;

			IMemory mem;
			foreach (WeakReference<IMemory> weakRef in this.memories)
			{
				if (weakRef.TryGetTarget(out mem))
				{
					mem.UpdateBaseOffset(this.baseOffset);
				}
			}
		}
	}
}
