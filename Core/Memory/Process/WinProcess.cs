// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Process
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using ConceptMatrix.Memory.Offsets;

	public class WinProcess : IProcess
	{
		private readonly Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

		public IntPtr Handle
		{
			get;
			private set;
		}

		public Process? Process
		{
			get;
			private set;
		}

		public bool IsAlive
		{
			get
			{
				if (this.Process == null || this.Process.HasExited)
					return false;

				if (!this.Process.Responding)
					return false;

				return true;
			}
		}

		public string ExecutablePath
		{
			get
			{
				if (this.Process == null)
					throw new Exception("No game process");

				return this.Process.MainModule.FileName;
			}
		}

		/// <summary>
		/// Open the PC game process with all security and access rights.
		/// </summary>
		public void OpenProcess(Process process)
		{
			this.Process = process;

			if (!this.Process.Responding)
				throw new Exception("Target process id not responding");

			Process.EnterDebugMode();
			int debugPrivilegeCheck = CheckSeDebugPrivilege(out bool isDebugEnabled);
			if (debugPrivilegeCheck != 0)
			{
				throw new Exception($"ERROR: CheckSeDebugPrivilege failed with error: {debugPrivilegeCheck}");
			}
			else if (!isDebugEnabled)
			{
				throw new Exception("ERROR: SeDebugPrivilege not enabled. Please report this!");
			}

			this.Handle = OpenProcess(0x001F0FFF, true, process.Id);
			if (this.Handle == IntPtr.Zero)
			{
				int eCode = Marshal.GetLastWin32Error();
			}

			// Set all modules
			this.modules.Clear();
			foreach (ProcessModule module in this.Process.Modules)
			{
				if (string.IsNullOrEmpty(module.ModuleName))
					continue;

				if (this.modules.ContainsKey(module.ModuleName))
					continue;

				this.modules.Add(module.ModuleName, module.BaseAddress);
			}
		}

		public ulong GetBaseAddress()
		{
			if (this.Process == null)
				throw new Exception("No game process");

			////(ulong)process.MainModule.BaseAddress.ToInt64())
			return (ulong)this.Process.MainModule.BaseAddress.ToInt64();
		}

		public UIntPtr GetAddress(params IMemoryOffset[] offsets)
		{
			int size = 16;

			List<ulong> offsetsList = new List<ulong>();
			for (int i = 0; i < offsets.Length; i++)
			{
				IMemoryOffset offset = offsets[i];
				ulong[] offsetValues = offset.Offsets;

				if (i == 0)
					offsetValues = new[] { this.GetBaseAddress() + offsetValues[0] };

				offsetsList.AddRange(offsetValues);
			}

			ulong[] longOffsets = offsetsList.ToArray();

			if (longOffsets.Length > 1)
			{
				byte[] memoryAddress = new byte[size];
				ReadProcessMemory(this.Handle, (UIntPtr)longOffsets[0], memoryAddress, (UIntPtr)size, IntPtr.Zero);

				long num1 = BitConverter.ToInt64(memoryAddress, 0);

				UIntPtr base1 = (UIntPtr)0;

				for (int i = 1; i < longOffsets.Length; i++)
				{
					base1 = new UIntPtr(Convert.ToUInt64(num1 + (long)longOffsets[i]));
					ReadProcessMemory(this.Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
					num1 = BitConverter.ToInt64(memoryAddress, 0);
				}

				return base1;
			}
			else
			{
				return (UIntPtr)longOffsets[0];
			}
		}

		public bool Read(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead)
		{
			return ReadProcessMemory(this.Handle, lpBaseAddress, lpBuffer, nSize, lpNumberOfBytesRead);
		}

		public bool Write(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten)
		{
			return WriteProcessMemory(this.Handle, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesWritten);
		}

		private static int CheckSeDebugPrivilege(out bool isDebugEnabled)
		{
			isDebugEnabled = false;

			if (!OpenProcessToken(GetCurrentProcess(), 0x8 /*TOKEN_QUERY*/, out IntPtr tokenHandle))
				return Marshal.GetLastWin32Error();

			LUID luidDebugPrivilege = default;
			if (!LookupPrivilegeValue(null, "SeDebugPrivilege", ref luidDebugPrivilege))
				return Marshal.GetLastWin32Error();

			PRIVILEGE_SET requiredPrivileges = new PRIVILEGE_SET
			{
				PrivilegeCount = 1,
				Control = 1 /* PRIVILEGE_SET_ALL_NECESSARY */,
				Privilege = new LUID_AND_ATTRIBUTES[1],
			};

			requiredPrivileges.Privilege[0].Luid = luidDebugPrivilege;
			requiredPrivileges.Privilege[0].Attributes = 2 /* SE_PRIVILEGE_ENABLED */;

			if (!PrivilegeCheck(tokenHandle, ref requiredPrivileges, out bool bResult))
				return Marshal.GetLastWin32Error();

			// bResult == true => SeDebugPrivilege is on; otherwise it's off
			isDebugEnabled = bResult;

			CloseHandle(tokenHandle);

			return 0;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

		[DllImport("kernel32.dll")]
		private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetCurrentProcess();

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(
			IntPtr processHandle,
			uint desiredAccess,
			out IntPtr tokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool LookupPrivilegeValue(string? lpSystemName, string lpName, ref LUID lpLuid);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool PrivilegeCheck(
			IntPtr clientToken,
			ref PRIVILEGE_SET requiredPrivileges,
			out bool pfResult);

		[DllImport("kernel32.dll")]
		private static extern int CloseHandle(
		IntPtr hObject);

		[StructLayout(LayoutKind.Sequential)]
		private struct LUID
		{
			public uint LowPart;
			public int HighPart;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct PRIVILEGE_SET
		{
			public uint PrivilegeCount;
			public uint Control;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public LUID_AND_ATTRIBUTES[] Privilege;
		}

		private struct LUID_AND_ATTRIBUTES
		{
			public LUID Luid;
			public uint Attributes;
		}
	}
}
