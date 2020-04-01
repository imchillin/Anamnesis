// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using ConceptMatrix.Injection.Memory;

	public class ProcessInjection
	{
		private Process process;

		private ProcessModule mainModule;
		private Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
		private bool is64Bit;

		public IntPtr Handle
		{
			get;
			private set;
		}

		public void OpenProcess(string contains)
		{
			Process[] processlist = Process.GetProcesses();
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

			this.process = Process.GetProcessById(pid);

			if (this.process == null)
				throw new Exception($"Failed to get process: {pid}");

			if (!this.process.Responding)
				throw new Exception("Target process id not responding");

			this.Handle = OpenProcess(0x001F0FFF, true, pid);
			Process.EnterDebugMode();

			if (this.Handle == IntPtr.Zero)
			{
				int eCode = Marshal.GetLastWin32Error();
			}

			// Set main module
			this.mainModule = this.process.MainModule;

			// Set all modules
			this.modules.Clear();
			foreach (ProcessModule module in this.process.Modules)
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

		public UIntPtr GetAddress(params string[] offsets)
		{
			string offset = GetOffset(offsets);
			return this.GetAddress(offset);
		}

		public string GetBaseAddress(string offset)
		{
			// this is a little weird, but its how it worked in CM2, so lets not mess with it.
			long offsetL = int.Parse(offset, NumberStyles.HexNumber);
			long value = this.process.MainModule.BaseAddress.ToInt64() + offsetL;
			return value.ToString("X");
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

		[DllImport("kernel32.dll")]
		private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		private static string GetOffset(params string[] offsets)
		{
			string ret = string.Empty;

			foreach (string a in offsets)
				ret += a + ",";

			return ret.TrimEnd(',');
		}

		/// <summary>
		/// Convert code from string to real address.
		/// </summary>
		private UIntPtr GetAddress(string name, int size = 16)
		{
			string theCode = name;

			if (string.IsNullOrEmpty(theCode))
				return UIntPtr.Zero;

			string newOffsets = theCode;
			if (theCode.Contains("+"))
				newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);

			byte[] memoryAddress = new byte[size];

			if (!theCode.Contains("+") && !theCode.Contains(","))
				return new UIntPtr(Convert.ToUInt64(theCode, 16));

			if (newOffsets.Contains(','))
			{
				List<long> offsetsList = new List<long>();

				string[] newerOffsets = newOffsets.Split(',');
				foreach (string oldOffsets in newerOffsets)
				{
					string test = oldOffsets;
					if (oldOffsets.Contains("0x"))
						test = oldOffsets.Replace("0x", string.Empty);

					long preParse = 0;
					if (!oldOffsets.Contains("-"))
					{
						preParse = long.Parse(test, NumberStyles.AllowHexSpecifier);
					}
					else
					{
						test = test.Replace("-", string.Empty);
						preParse = long.Parse(test, NumberStyles.AllowHexSpecifier);
						preParse = preParse * -1;
					}

					offsetsList.Add(preParse);
				}

				long[] offsets = offsetsList.ToArray();

				if (theCode.Contains("base") || theCode.Contains("main"))
				{
					ReadProcessMemory(this.Handle, (UIntPtr)((long)this.mainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
				}
				else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
				{
					string[] moduleName = theCode.Split('+');
					IntPtr altModule = IntPtr.Zero;
					if (!moduleName[0].Contains(".dll") && !moduleName[0].Contains(".exe"))
					{
						altModule = (IntPtr)long.Parse(moduleName[0], System.Globalization.NumberStyles.HexNumber);
					}
					else
					{
						try
						{
							altModule = this.modules[moduleName[0]];
						}
						catch
						{
							Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
							Debug.WriteLine("Modules: " + string.Join(",", this.modules));
						}
					}

					ReadProcessMemory(this.Handle, (UIntPtr)((long)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
				}
				else
				{
					// no offsets
					ReadProcessMemory(this.Handle, (UIntPtr)offsets[0], memoryAddress, (UIntPtr)size, IntPtr.Zero);
				}

				long num1 = BitConverter.ToInt64(memoryAddress, 0);

				UIntPtr base1 = (UIntPtr)0;

				for (int i = 1; i < offsets.Length; i++)
				{
					base1 = new UIntPtr(Convert.ToUInt64(num1 + offsets[i]));
					ReadProcessMemory(this.Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
					num1 = BitConverter.ToInt64(memoryAddress, 0);
				}

				return base1;
			}
			else
			{
				long trueCode = Convert.ToInt64(newOffsets, 16);
				IntPtr altModule = IntPtr.Zero;
				if (theCode.Contains("base") || theCode.Contains("main"))
				{
					altModule = this.mainModule.BaseAddress;
				}
				else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
				{
					string[] moduleName = theCode.Split('+');
					if (!moduleName[0].Contains(".dll") && !moduleName[0].Contains(".exe"))
					{
						string theAddr = moduleName[0];
						if (theAddr.Contains("0x"))
							theAddr = theAddr.Replace("0x", string.Empty);
						altModule = (IntPtr)long.Parse(theAddr, NumberStyles.HexNumber);
					}
					else
					{
						try
						{
							altModule = this.modules[moduleName[0]];
						}
						catch
						{
							Debug.WriteLine("Module " + moduleName[0] + " was not found in module list!");
							Debug.WriteLine("Modules: " + string.Join(",", this.modules));
						}
					}
				}
				else
				{
					altModule = this.modules[theCode.Split('+')[0]];
				}

				return (UIntPtr)((long)altModule + trueCode);
			}
		}
	}
}
