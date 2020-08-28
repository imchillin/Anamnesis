// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Memory
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using ConceptMatrix.Memory.Exceptions;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal abstract class MemoryBase : IMemory
	{
		protected IProcess process;
		protected UIntPtr address;
		protected IMemoryOffset[] offsets;

		private static readonly List<MemoryBase> ActiveMemory = new List<MemoryBase>();

		public MemoryBase(IProcess process, IMemoryOffset[] offsets)
		{
			this.Name = string.Empty;

			foreach (IMemoryOffset offset in offsets)
			{
				if (string.IsNullOrEmpty(offset.Name))
				{
					this.Name += "[Unknown], ";
				}
				else
				{
					this.Name += offset.Name + ", ";
				}
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

		public event PropertyChangedEventHandler? PropertyChanged;

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
			List<MemoryBase> memories;
			lock (ActiveMemory)
			{
				memories = new List<MemoryBase>(ActiveMemory);
			}

			AnamnesisService service = AnamnesisService.Instance;

			foreach (MemoryBase memory in memories)
			{
				if (!service.ProcessIsAlive)
					return;

				// Handle cases where memory was disposed while we were ticking.
				if (!memory.Active)
					continue;

				memory.Tick();
			}
		}

		public static void DisposeAllMemory()
		{
			List<MemoryBase> memories;
			lock (ActiveMemory)
			{
				memories = new List<MemoryBase>(ActiveMemory);
			}

			foreach (MemoryBase memory in memories)
			{
				if (!memory.Active)
					continue;

				memory.Dispose();
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
				throw new InvalidAddressException(this.ToString());
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

		protected abstract void Tick(int attempt = 0);

		protected void RaiseChanged(string name)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
