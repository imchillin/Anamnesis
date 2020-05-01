// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Runtime.InteropServices;

	public class WinProcess : IProcess
	{
		private ProcessModule mainModule;
		private Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
		private bool is64Bit;

		public IntPtr Handle
		{
			get;
			private set;
		}

		public Process Process
		{
			get;
			private set;
		}

		public bool IsAlive
		{
			get
			{
				if (this.Process.HasExited)
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
				return this.Process.MainModule.FileName;
			}
		}

		public void OpenProcess(string contains)
		{
			Process[] processlist = System.Diagnostics.Process.GetProcesses();
			foreach (Process process in processlist)
			{
				if (process.ProcessName.ToLower().Contains(contains))
				{
					this.OpenProcess(process.Id);
					return;
				}
			}

			throw new Exception($"Failed to find process containing: \"{contains}\"");
		}

		/// <summary>
		/// Open the PC game process with all security and access rights.
		/// </summary>
		public void OpenProcess(int pid)
		{
			if (pid <= 0)
				throw new Exception($"Invalid process id: {pid}");

			this.Process = Process.GetProcessById(pid);

			if (this.Process == null)
				throw new Exception($"Failed to get process: {pid}");

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

			this.Handle = OpenProcess(0x001F0FFF, true, pid);
			if (this.Handle == IntPtr.Zero)
			{
				int eCode = Marshal.GetLastWin32Error();
			}

			// Set main module
			this.mainModule = this.Process.MainModule;

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

			this.is64Bit = Environment.Is64BitOperatingSystem && (IsWow64Process(this.Handle, out bool retVal) && !retVal);

			Debug.WriteLine($"Attached to process: {pid}");
		}

		public ulong GetBaseAddress()
		{
			////(ulong)process.MainModule.BaseAddress.ToInt64())
			return (ulong)this.Process.MainModule.BaseAddress.ToInt64();
		}

		public UIntPtr GetAddress(params IMemoryOffset[] offsets)
		{
			int size = 16;

			List<ulong> offsetsList = new List<ulong>();
			foreach (IMemoryOffset offset in offsets)
			{
				offsetsList.AddRange(offset.Offsets);
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

			LUID luidDebugPrivilege = default(LUID);
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
		private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

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
