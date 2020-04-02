// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.ComponentModel;
	using ConceptMatrix;

	public abstract class MemoryBase<T> : MemoryBase, IMemory<T>
	{
		private T value;
		private bool freeze;

		public MemoryBase(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
			// Tick once to ensure current value is valid
			this.Tick();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event ValueChangedEventHandler ValueChanged;

		public T Value
		{
			get
			{
				return this.value;
			}

			set
			{
				this.value = value;
				this.Write(value);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
				this.ValueChanged.Invoke(this, value);
			}
		}

		public bool Freeze
		{
			get
			{
				return this.freeze;
			}

			set
			{
				this.freeze = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
			}
		}

		protected abstract T Read();
		protected abstract void Write(T value);

		protected override void Tick()
		{
			if (this.Freeze)
			{
				// Frozen values get written constantly
				this.Write(this.value);
			}
			else
			{
				// unfrozen values get read constantly
				T newValue = this.Read();

				if (!newValue.Equals(this.value))
				{
					this.value = newValue;
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
					this.ValueChanged?.Invoke(this, this.value);
				}
			}
		}
	}
}
