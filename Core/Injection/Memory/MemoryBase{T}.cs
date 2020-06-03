// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Runtime.InteropServices;
	using ConceptMatrix;
	using ConceptMatrix.Exceptions;

	public abstract class MemoryBase<T> : MemoryBase, IMemory<T>
	{
		private static readonly IInjectionService Injection = Services.Get<IInjectionService>();

		private readonly ulong length;
		private byte[] oldData;
		private byte[] newData;

		private T value;
		private bool freeze;
		private bool dirty;

		private Exception lastException;

		public MemoryBase(IProcess process, IMemoryOffset[] offsets, ulong length)
			: base(process, offsets)
		{
			this.length = length;
			this.oldData = new byte[this.length];
			this.newData = new byte[this.length];

			this.value = this.Read(ref this.oldData);

			// Tick once to ensure current value is valid
			this.Tick();
		}

		public event ValueChangedEventHandler ValueChanged;
		public event DisposingEventHandler Disposing;

		public T Value
		{
			get
			{
				if (!this.Active)
					throw new MemoryException("Cannot access disposed memory");

				if (this.lastException != null)
					throw this.lastException;

				return this.value;
			}

			set
			{
				if (!this.Active)
					throw new MemoryException("Cannot access disposed memory");

				this.value = value;
				this.dirty = true;
			}
		}

		public bool Freeze
		{
			get
			{
				if (!this.Active)
					throw new MemoryException("Cannot access disposed memory");

				return this.freeze;
			}

			set
			{
				if (!this.Active)
					throw new MemoryException("Cannot access disposed memory");

				this.freeze = value;
				this.RaiseChanged(nameof(this.Value));
			}
		}

		public void SetValue(T value, bool immediate = false)
		{
			this.Value = value;

			if (immediate)
			{
				this.Tick();
			}
		}

		public override void Dispose()
		{
			this.Disposing?.Invoke();
			base.Dispose();
		}

		protected static bool Equals(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
		{
			return a1.SequenceEqual(a2);
		}

		protected abstract T Read(ref byte[] data);
		protected abstract void Write(T value, ref byte[] data);

		protected override void Tick()
		{
			try
			{
				if (!this.Active)
					throw new MemoryException("Cannot access disposed memory");

				bool changed = false;
				lock (this)
				{
					this.lastException = null;

					if (this.dirty)
					{
						changed = this.DoWrite(this.value);
						this.dirty = false;
					}

					if (this.Freeze)
					{
						// Frozen values get written constantly
						this.DoWrite(this.value, true);
					}
					else
					{
						// unfrozen values get read constantly
						this.DoRead();

						if (!Equals(this.newData, this.oldData))
						{
							this.value = this.Read(ref this.newData);
							Array.Copy(this.newData, this.oldData, (int)this.length);
							changed = true;
						}
					}
				}

				if (changed)
				{
					this.RaiseChanged(nameof(this.Value));
					this.ValueChanged?.Invoke(this, this.value);
				}
			}
			catch (Exception ex)
			{
				this.lastException = ex;
				this.Dispose();
				Log.Write("Disposing of memory due to exception: " + ex.Message);
			}
		}

		// writes value to oldData and to memory
		private bool DoWrite(T val, bool force = false)
		{
			if (!Injection.ProcessIsAlive)
				throw new Exception("no FFXIV process");

			this.Write(val, ref this.newData);

			if (!Equals(this.newData, this.oldData) || force)
			{
				Array.Copy(this.newData, this.oldData, (int)this.length);

				Log.Write("Write memory " + this, "Injection");

				this.process.Write(this.address, this.oldData, (UIntPtr)this.length, out IntPtr bytesRead);
				return true;
			}

			return false;
		}

		// Reads memory into newData array
		private void DoRead()
		{
			if (!Injection.ProcessIsAlive)
				throw new Exception("no FFXIV process");

			if (!this.process.Read(this.address, this.newData, (UIntPtr)this.length, IntPtr.Zero))
			{
				int code = Marshal.GetLastWin32Error();

				// code 0 means success. ooh boy Win32 legacy API's.
				// code 299 means part of a ReadProcessMemory or WriteProcessMemory request was completed.
				// code 998 is an Invalid access to memory location.
				if (code == 0 || code == 299)
					return;

				throw new MemoryException($"Failed to read process memory {code}", new Win32Exception(code));
			}
		}
	}
}
