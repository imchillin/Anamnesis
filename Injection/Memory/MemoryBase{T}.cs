// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public abstract class MemoryBase<T> : MemoryBase, IMemory<T>
	{
		public MemoryBase(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		public T Value { get; private set; }

		/// <summary>
		/// Gets the current value from the process.
		/// </summary>
		public T Get()
		{
			T newValue = this.Read();
			this.Value = newValue;
			return newValue;
		}

		/// <summary>
		/// Writes a new value to the process, and returns the old value.
		/// </summary>
		public void Set(T value)
		{
			T oldValue = this.Value;
			this.Write(value);
			this.Value = value;
		}

		protected abstract T Read();
		protected abstract void Write(T value);
	}
}
