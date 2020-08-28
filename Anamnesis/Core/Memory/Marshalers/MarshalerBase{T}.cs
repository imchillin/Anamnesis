// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Runtime.InteropServices;
	using System.Threading;
	using Anamnesis.Memory;
	using Anamnesis.Memory.Exceptions;
	using Anamnesis.Memory.Offsets;
	using Anamnesis.Memory.Process;

	internal abstract class MarshalerBase<T> : MarshalerBase, IMarshaler<T>
	{
		private readonly ulong length;
		private byte[] oldData;
		private byte[] newData;

		private T value;
		private bool dirty;

		private Exception? lastException;

		public MarshalerBase(IProcess process, IMemoryOffset[] offsets, ulong length)
			: base(process, offsets)
		{
			this.length = length;
			this.oldData = new byte[this.length];
			this.newData = new byte[this.length];

			this.value = this.Read(ref this.oldData);

			// Tick once to ensure current value is valid
			this.Tick();
		}

		public event ValueChangedEventHandler<T>? ValueChanged;
		public event DisposingEventHandler? Disposing;

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

		public void SetValue(T value, bool immediate = false)
		{
			this.Value = value;
			this.dirty = true;

			if (immediate)
			{
				this.Tick();
			}
		}

		public override void Dispose()
		{
			// Ensure we ahve written any dirty values to memory before disposing
			if (this.dirty)
				this.Tick();

			this.Disposing?.Invoke();
			base.Dispose();
		}

		protected static bool Equals(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
		{
			return a1.SequenceEqual(a2);
		}

		protected abstract T Read(ref byte[] data);
		protected abstract void Write(T value, ref byte[] data);

		protected override void Tick(int attempt = 0)
		{
			if (!this.Active)
				return;

			try
			{
				bool changed = false;
				lock (this)
				{
					this.lastException = null;

					this.DoRead();

					if (this.dirty)
					{
						changed = this.DoWrite(this.value);
						this.dirty = false;
					}

					if (!Equals(this.newData, this.oldData))
					{
						this.value = this.Read(ref this.newData);
						Array.Copy(this.newData, this.oldData, (int)this.length);
						changed = true;
					}
				}

				if (changed)
				{
					// last chance for inactive memory to stop firing events
					if (!this.Active)
						return;

					try
					{
						this.RaiseChanged(nameof(this.Value));
						this.ValueChanged?.Invoke(this, this.value);
					}
					catch (Exception ex)
					{
						MarshalerService.Instance.OnError(ex);
					}
				}
			}
			catch (Exception ex)
			{
				if (attempt <= 10)
				{
					Thread.Sleep(10);
					this.Tick(attempt++);
				}
				else
				{
					this.lastException = ex;
					this.dirty = false;
					this.Dispose();

					MarshalerService.Instance.OnError(new Exception("Disposing of memory: " + this + " due to exception", ex));
				}
			}
		}

		private string LogDat(byte[] data)
		{
			string str = string.Empty;
			foreach (byte b in data)
			{
				str += b.ToString() + " ";
			}

			return str;
		}

		// writes value to oldData and to memory
		private bool DoWrite(T val, bool force = false)
		{
			if (!MarshalerService.Instance.ProcessIsAlive)
				throw new Exception("no FFXIV process");

			this.Write(val, ref this.newData);

			if (!Equals(this.newData, this.oldData) || force)
			{
				Array.Copy(this.newData, this.oldData, (int)this.length);

				MarshalerService.Instance.OnLog("Write memory " + this);

				this.process.Write(this.address, this.oldData, (UIntPtr)this.length, out IntPtr bytesRead);
				return true;
			}

			return false;
		}

		// Reads memory into newData array
		private void DoRead()
		{
			if (!MarshalerService.Instance.ProcessIsAlive)
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
