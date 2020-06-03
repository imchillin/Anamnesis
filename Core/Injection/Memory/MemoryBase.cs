// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Collections.Generic;
	using ConceptMatrix.Exceptions;

	public abstract class MemoryBase
	{
		protected IProcess process;
		protected UIntPtr address;
		protected IMemoryOffset[] offsets;

		private static readonly List<MemoryBase> ActiveMemory = new List<MemoryBase>();

		public MemoryBase(IProcess process, IMemoryOffset[] offsets)
		{
			this.process = process;
			this.offsets = offsets;

			this.address = process.GetAddress(this.offsets);

			if (this.address == UIntPtr.Zero)
				throw new InvalidAddressException();

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

		public override string ToString()
		{
			string offsetString = string.Empty;
			foreach (IMemoryOffset offset in this.offsets)
			{
				offsetString += " " + offset + ",";
			}

			offsetString = offsetString.Trim(' ', ',');
			return offsetString + " (" + this.address + ")";
		}

		protected abstract void Tick();
	}
}
