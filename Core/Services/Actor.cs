// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Exceptions;
	using Anamnesis.Offsets;

	public class Actor : IDisposable
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

		public delegate void ActorEvent(Actor actor);

		public event ActorEvent ActorRetargeted;

		public ActorTypes Type { get; set; }
		public string Name { get; private set; }
		public string Description { get; set; }

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
			return mem.Value;
		}

		public void SetValue<T>(IMemoryOffset<T> offset, T value)
		{
			using IMemory<T> mem = this.GetMemory(offset);
			mem.Value = value;
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

		public async Task ActorRefreshAsync()
		{
			IActorRefreshService refreshService = Services.Get<IActorRefreshService>();
			await refreshService.RefreshAsync(this);
		}

		public void Retarget(Actor actor)
		{
			if (this.baseOffset == actor.baseOffset)
				return;

			Log.Write("Retargeting actor from " + this.Description + "(" + this.baseOffset + " to " + actor.Description + "(" + actor.baseOffset + ")");

			this.baseOffset = actor.baseOffset;
			this.Name = actor.Name;
			this.Description = actor.Description;
			this.Type = actor.Type;

			IMemory mem;
			foreach (WeakReference<IMemory> weakRef in this.memories)
			{
				if (weakRef.TryGetTarget(out mem))
				{
					if (!mem.Active)
						continue;

					try
					{
						mem.UpdateBaseOffset(this.baseOffset);
					}
					catch (MemoryException)
					{
						mem.Dispose();
					}
				}
			}

			this.ActorRetargeted?.Invoke(this);

			Log.Write("Retargeting actor done");
		}

		public void Dispose()
		{
			IMemory mem;
			foreach (WeakReference<IMemory> weakRef in this.memories)
			{
				if (weakRef.TryGetTarget(out mem))
				{
					mem.Dispose();
				}
			}
		}
	}
}
