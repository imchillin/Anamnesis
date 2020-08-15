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
			this.Description = string.Empty;
		}

		public delegate void ActorEvent(Actor actor);

		public ActorTypes Type { get; set; }
		public string Name { get; private set; }
		public string Description { get; set; }

		public static bool operator ==(Actor lhs, Actor rhs)
		{
			if (object.ReferenceEquals(lhs, null))
				return object.ReferenceEquals(rhs, null);

			return lhs.Equals(rhs);
		}

		public static bool operator !=(Actor lhs, Actor rhs)
		{
			return !(lhs == rhs);
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

		public override bool Equals(object? obj)
		{
			return obj is Actor actor && this.baseOffset.Equals(actor.baseOffset) && this.Name == actor.Name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(this.baseOffset);
		}
	}
}
