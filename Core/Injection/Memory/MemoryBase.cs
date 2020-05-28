// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Collections.Generic;

	public abstract class MemoryBase
	{
		protected IProcess process;
		protected UIntPtr address;

		private static readonly List<MemoryBase> ActiveMemory = new List<MemoryBase>();

		public MemoryBase(IProcess process, UIntPtr address)
		{
			this.process = process;
			this.address = address;

			lock (ActiveMemory)
			{
				ActiveMemory.Add(this);
			}

			this.Active = true;
		}

		public bool Active
		{
			get;
			private set;
		}

		public string Name { get; set; }

		public static void TickAllActiveMemory()
		{
			IInjectionService injection = Services.Get<IInjectionService>();

			List<MemoryBase> memories;
			lock (ActiveMemory)
			{
				memories = new List<MemoryBase>(ActiveMemory);
			}

			foreach (MemoryBase memory in memories)
			{
				if (!injection.ProcessIsAlive)
					return;

				memory.Tick();
			}
		}

		public virtual void Dispose()
		{
			lock (ActiveMemory)
			{
				ActiveMemory.Remove(this);
			}

			this.Active = false;
		}

		protected abstract void Tick();
	}
}
