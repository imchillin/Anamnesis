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

		private ulong length;
		private byte[] oldData;
		private byte[] newData;

		public MemoryBase(ProcessInjection process, UIntPtr address, ulong length)
			: base(process, address)
		{
			this.length = length;
			this.oldData = new byte[this.length];
			this.newData = new byte[this.length];

			this.value = this.Read(ref this.oldData);

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
				if (this.DoWrite(value))
				{
					this.ValueChanged?.Invoke(this, value);
				}

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
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

		protected abstract T Read(ref byte[] data);
		protected abstract void Write(T value, ref byte[] data);

		protected override void Tick()
		{
			if (this.Freeze)
			{
				// Frozen values get written constantly
				this.DoWrite(this.value);
			}
			else
			{
				// unfrozen values get read constantly
				this.DoRead();

				if (!Equals(this.newData, this.oldData))
				{
					this.value = this.Read(ref this.newData);
					Array.Copy(this.newData, this.oldData, (int)this.length);

					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
					this.ValueChanged?.Invoke(this, this.value);
				}
			}
		}

		private static bool Equals(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
		{
			return a1.SequenceEqual(a2);
		}

		// writes value to oldData and to memory
		private bool DoWrite(T val)
		{
			this.Write(val, ref this.newData);

			if (!Equals(this.newData, this.oldData))
			{
				Array.Copy(this.newData, this.oldData, (int)this.length);
				InjectionService.WriteProcessMemory(this.process.Handle, this.address, this.oldData, (UIntPtr)this.length, out IntPtr bytesRead);
				return true;
			}

			return false;
		}

		// Reads memory into newData array
		private void DoRead()
		{
			if (!InjectionService.ReadProcessMemory(this.process.Handle, this.address, this.newData, (UIntPtr)this.length, IntPtr.Zero))
			{
				throw new Exception("Failed to read process memory");
			}
		}
	}
}
