// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Process
{
	using System;
	using System.Diagnostics;
	using ConceptMatrix.Memory.Offsets;

	public interface IProcess
	{
		string ExecutablePath { get; }
		bool IsAlive { get; }
		IntPtr Handle { get; }

		void OpenProcess(Process process);
		ulong GetBaseAddress();
		UIntPtr GetAddress(params IMemoryOffset[] offsets);

		bool Read(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);
		bool Write(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);
	}
}
