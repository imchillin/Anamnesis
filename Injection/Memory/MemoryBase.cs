// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

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

		/// <summary>
		/// Write byte array to address.
		/// </summary>
		public void WriteBytes(byte[] write)
		{
			WriteProcessMemory(this.process.Handle, this.address, write, (UIntPtr)write.Length, out IntPtr bytesRead);
		}

		/// <summary>
		/// Read byte array from the address.
		/// </summary>
		public byte[] ReadBytes(long length)
		{
			byte[] bytes = new byte[length];

			if (!ReadProcessMemory(this.process.Handle, this.address, bytes, (UIntPtr)length, IntPtr.Zero))
			{
				Array.Clear(bytes, 0, bytes.Length);
				return bytes;
			}

			return bytes;
		}

		public void Dispose()
		{
			lock (activeMemory)
			{
				activeMemory.Remove(this);
			}
		}

		protected abstract void Tick();

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);
	}
}
