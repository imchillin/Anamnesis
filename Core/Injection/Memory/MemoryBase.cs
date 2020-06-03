// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using ConceptMatrix.Exceptions;

	public abstract class MemoryBase : IMemory
	{
		protected IProcess process;
		protected UIntPtr address;
		protected IMemoryOffset[] offsets;

		private static readonly List<MemoryBase> ActiveMemory = new List<MemoryBase>();

		public MemoryBase(IProcess process, IMemoryOffset[] offsets)
		{
			foreach (IMemoryOffset offset in offsets)
			{
				this.Name += offset.Name + ", ";
			}

			this.process = process;
			this.offsets = offsets;

			this.UpdateAddress();

			lock (ActiveMemory)
			{
				ActiveMemory.Add(this);
			}

			this.Active = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string Name
		{
			get;
			private set;
		}

		public bool Active
		{
			get;
			private set;
		}

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

		public void UpdateBaseOffset(IBaseMemoryOffset newBaseOffset)
		{
			if (this.offsets[0] is IBaseMemoryOffset)
			{
				this.offsets[0] = newBaseOffset;
				this.UpdateAddress();
			}
			else
			{
				throw new Exception("First offset in memory was not a base offset");
			}
		}

		public void UpdateAddress()
		{
			this.address = this.process.GetAddress(this.offsets);

			if (this.address == UIntPtr.Zero)
				throw new InvalidAddressException();
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
			return this.Name + ": " + offsetString + " (" + this.address + ")";
		}

		protected abstract void Tick();

		protected void RaiseChanged(string name)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
