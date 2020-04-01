// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Runtime.InteropServices;

	public abstract class MemoryBase
	{
		protected ProcessInjection process;
		protected UIntPtr address;

		public MemoryBase(ProcessInjection process, UIntPtr address)
		{
			this.process = process;
			this.address = address;
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

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);
	}
}
