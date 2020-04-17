// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Runtime.InteropServices;

	public class ProcessInjection
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

			this.Handle = OpenProcess(0x001F0FFF, true, pid);
			System.Diagnostics.Process.EnterDebugMode();

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

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

		[DllImport("kernel32.dll")]
		private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);
	}
}
