// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Process
{
	using System;
	using System.Diagnostics;
	using Anamnesis.Memory.Offsets;

	public class DummyProcess : IProcess
	{
		public string ExecutablePath
		{
			get
			{
				return @"D:/Games/SteamLibrary/steamapps/common/FINAL FANTASY XIV Online/game/ffxiv_dx11.exe";
			}
		}

		public bool IsAlive
		{
			get
			{
				return true;
			}
		}

		public IntPtr Handle
		{
			get
			{
				return IntPtr.Zero;
			}
		}

		public UIntPtr GetAddress(params IMemoryOffset[] offsets)
		{
			return UIntPtr.Zero;
		}

		public ulong GetBaseAddress()
		{
			return 0;
		}

		public void OpenProcess(Process proc)
		{
		}

		public bool Read(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead)
		{
			return true;
		}

		public bool Write(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten)
		{
			lpNumberOfBytesWritten = IntPtr.Zero;
			return true;
		}
	}
}
