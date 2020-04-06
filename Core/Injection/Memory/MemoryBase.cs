// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Collections.Generic;

	public abstract class MemoryBase
	{
		protected ProcessInjection process;
		protected UIntPtr address;

		private static List<MemoryBase> activeMemory = new List<MemoryBase>();

		public MemoryBase(ProcessInjection process, UIntPtr address)
		{
			this.process = process;
			this.address = address;

			lock (activeMemory)
			{
				activeMemory.Add(this);
			}
		}

		public static void TickAllActiveMemory()
		{
			List<MemoryBase> memories;
			lock (activeMemory)
			{
				memories = new List<MemoryBase>(activeMemory);
			}

			foreach (MemoryBase memory in memories)
			{
				memory.Tick();
			}
		}

		public void Dispose()
		{
			lock (activeMemory)
			{
				activeMemory.Remove(this);
			}
		}

		protected abstract void Tick();
	}
}
