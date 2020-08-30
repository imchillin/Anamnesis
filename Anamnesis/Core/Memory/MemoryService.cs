// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.GUI.Windows;
	using Anamnesis.Memory.Marshalers;
	using Anamnesis.Memory.Offsets;
	using SimpleLog;

	using SysProcess = System.Diagnostics.Process;

	public class MemoryService : IService
	{
		private static readonly Logger Log = SimpleLog.Log.GetLogger<MemoryService>();

		private static Dictionary<Type, Type> marshalerLookup = new Dictionary<Type, Type>();
		private static ulong tickCount = 0;
		private readonly Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
		private bool isActive;

		public static IntPtr Handle { get; private set; }
		public static SignatureScanner? Scanner { get; private set; }
		public static SysProcess? Process { get; private set; }
		public static bool ProcessIsAlive { get; private set; }

		public static bool IsAlive
		{
			get
			{
				if (Process == null || Process.HasExited)
					return false;

				if (!Process.Responding)
					return false;

				return true;
			}
		}

		public static string GamePath
		{
			get
			{
				if (Process == null)
					throw new Exception("No game process");

				return Path.GetDirectoryName(Process.MainModule.FileName) + "\\..\\";
			}
		}

		public static void AddMarshaler<TType, TMarshaler>()
			where TMarshaler : IMarshaler<TType>
		{
			Type memoryType = typeof(TType);

			if (marshalerLookup.ContainsKey(memoryType))
				throw new Exception("Marshaler already registered for type: " + memoryType);

			marshalerLookup.Add(memoryType, typeof(TMarshaler));
		}

		public static IMarshaler<T> GetMarshaler<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets)
		{
			List<IMemoryOffset> newOffsets = new List<IMemoryOffset>();
			newOffsets.Add(baseOffset);
			newOffsets.AddRange(offsets);
			return GetMarshaler<T>(newOffsets.ToArray());
		}

		public static IMarshaler<T> GetMarshaler<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return GetMarshaler<T>(baseOffset, (IMemoryOffset[])offsets);
		}

		public static IMarshaler<T> GetMarshaler<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return GetMarshaler<T>((IBaseMemoryOffset)baseOffset, (IMemoryOffset[])offsets);
		}

		public static IMarshaler<T> GetMarshaler<T>(params IMemoryOffset[] offsets)
		{
			Type marshalerType = GetMarshalerType(typeof(T));
			try
			{
				object? obj = Activator.CreateInstance(marshalerType, offsets);

				if (obj == null)
					throw new Exception("Failed to create marshaler instance");

				return (MarshalerBase<T>)obj;
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException == null)
					throw ex;

				throw ex.InnerException;
			}
		}

		public static async Task WaitForMarshalerTick()
		{
			// we wait for two ticks since we might be towards the end of a tick,
			// meaning the next tick (+1) will become active without ticking _all_ the memory.
			// wait for +2 guarantees that all currently tracked memory will get a chance to tick
			// before we return.
			ulong targetTick = tickCount + 2;

			while (tickCount < targetTick)
			{
				await Task.Delay(100);
			}
		}

		public static IntPtr ReadPtr(IntPtr address)
		{
			byte[] d = new byte[8];
			ReadProcessMemory(Handle, address, d, 8, out _);
			long i = BitConverter.ToInt64(d, 0);
			IntPtr ptr = (IntPtr)i;
			return ptr;
		}

		public static T? Read<T>(UIntPtr address)
			where T : struct
		{
			unsafe
			{
				IntPtr ptr = (IntPtr)address.ToPointer();
				return Read<T>(ptr);
			}
		}

		public static T? Read<T>(IntPtr address)
			where T : struct
		{
			if (address == IntPtr.Zero)
				return null;

			int size = Marshal.SizeOf(typeof(T));
			IntPtr mem = Marshal.AllocHGlobal(size);
			ReadProcessMemory(Handle, address, mem, size, out _);
			T val = Marshal.PtrToStructure<T>(mem);
			Marshal.FreeHGlobal(mem);

			return val;
		}

		public async Task Initialize()
		{
			this.isActive = true;

			AddMarshaler<ActorTypes, ActorTypesMarshaler>();
			AddMarshaler<Appearance, AppearanceMarshaler>();
			AddMarshaler<bool, BoolMarshaler>();
			AddMarshaler<byte, ByteMarshaler>();
			AddMarshaler<Color4, Color4Marshaler>();
			AddMarshaler<Color, ColorMarshaler>();
			AddMarshaler<Equipment, EquipmentMarshaler>();
			AddMarshaler<Flag, FlagMarshaler>();
			AddMarshaler<float, FloatMarshaler>();
			AddMarshaler<int, IntMarshaler>();
			AddMarshaler<Quaternion, QuaternionMarshaler>();
			AddMarshaler<short, ShortMarshaler>();
			AddMarshaler<string, StringMarshaler>();
			AddMarshaler<Transform, TransformMarshaler>();
			AddMarshaler<ushort, UShortMarshaler>();
			AddMarshaler<Vector2D, Vector2DMarshaler>();
			AddMarshaler<Vector, VectorMarshaler>();
			AddMarshaler<Weapon, WeaponMarshaler>();

			while (!ProcessIsAlive)
			{
				try
				{
					SysProcess[] processes = System.Diagnostics.Process.GetProcesses();
					SysProcess? proc = null;
					foreach (SysProcess process in processes)
					{
						if (process.ProcessName.ToLower().Contains("ffxiv_dx11"))
						{
							if (proc != null)
								throw new Exception("Multiple processes found");

							proc = process;
						}
					}

					if (proc == null)
						throw new Exception("No process found");

					this.OpenProcess(proc);
					ProcessIsAlive = true;
				}
				catch (Exception ex)
				{
					SysProcess? proc = null;

					proc = await App.Current.Dispatcher.InvokeAsync<Process?>(() =>
					{
						Process? proc = ProcessSelector.FindProcess();

						if (proc == null)
							App.Current.Shutdown();

						return proc;
					});

					if (proc == null)
						throw new Exception("Unable to locate FFXIV process", ex);

					this.OpenProcess(proc);
					ProcessIsAlive = true;
				}
			}

			new Thread(new ThreadStart(this.TickMarshalersThread)).Start();
			new Thread(new ThreadStart(this.ProcessWatcherThread)).Start();
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.isActive = false;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Open the PC game process with all security and access rights.
		/// </summary>
		public void OpenProcess(SysProcess process)
		{
			Process = process;

			if (!Process.Responding)
				throw new Exception("Target process id not responding");

			SysProcess.EnterDebugMode();
			int debugPrivilegeCheck = CheckSeDebugPrivilege(out bool isDebugEnabled);
			if (debugPrivilegeCheck != 0)
			{
				throw new Exception($"ERROR: CheckSeDebugPrivilege failed with error: {debugPrivilegeCheck}");
			}
			else if (!isDebugEnabled)
			{
				throw new Exception("ERROR: SeDebugPrivilege not enabled. Please report this!");
			}

			Handle = OpenProcess(0x001F0FFF, true, process.Id);
			if (Handle == IntPtr.Zero)
			{
				int eCode = Marshal.GetLastWin32Error();
			}

			// Set all modules
			this.modules.Clear();
			foreach (ProcessModule? module in Process.Modules)
			{
				if (module == null)
					continue;

				if (string.IsNullOrEmpty(module.ModuleName))
					continue;

				if (this.modules.ContainsKey(module.ModuleName))
					continue;

				this.modules.Add(module.ModuleName, module.BaseAddress);
			}

			Scanner = new SignatureScanner(process.MainModule);
		}

		internal static ulong GetBaseAddress()
		{
			if (Process == null)
				throw new Exception("No game process");

			////(ulong)process.MainModule.BaseAddress.ToInt64())
			return (ulong)Process.MainModule.BaseAddress.ToInt64();
		}

		internal static UIntPtr GetAddress(params IMemoryOffset[] offsets)
		{
			int size = 16;

			List<ulong> offsetsList = new List<ulong>();
			for (int i = 0; i < offsets.Length; i++)
			{
				IMemoryOffset offset = offsets[i];
				ulong[] offsetValues = offset.Offsets;

				if (i == 0)
					offsetValues = new[] { GetBaseAddress() + offsetValues[0] };

				offsetsList.AddRange(offsetValues);
			}

			ulong[] longOffsets = offsetsList.ToArray();

			if (longOffsets.Length > 1)
			{
				byte[] memoryAddress = new byte[size];
				ReadProcessMemory(Handle, (UIntPtr)longOffsets[0], memoryAddress, (UIntPtr)size, IntPtr.Zero);

				long num1 = BitConverter.ToInt64(memoryAddress, 0);

				UIntPtr base1 = (UIntPtr)0;

				for (int i = 1; i < longOffsets.Length; i++)
				{
					base1 = new UIntPtr(Convert.ToUInt64(num1 + (long)longOffsets[i]));
					ReadProcessMemory(Handle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
					num1 = BitConverter.ToInt64(memoryAddress, 0);
				}

				return base1;
			}
			else
			{
				return (UIntPtr)longOffsets[0];
			}
		}

		internal static bool Read(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead)
		{
			return ReadProcessMemory(Handle, lpBaseAddress, lpBuffer, nSize, lpNumberOfBytesRead);
		}

		internal static bool Write(UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten)
		{
			return WriteProcessMemory(Handle, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesWritten);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

		[DllImport("kernel32.dll")]
		private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetCurrentProcess();

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool LookupPrivilegeValue(string? lpSystemName, string lpName, ref LUID lpLuid);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool PrivilegeCheck(IntPtr clientToken, ref PRIVILEGE_SET requiredPrivileges, out bool pfResult);

		[DllImport("kernel32.dll")]
		private static extern int CloseHandle(IntPtr hObject);

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

		private static Type GetMarshalerType(Type type)
		{
			if (!marshalerLookup.ContainsKey(type))
				throw new Exception($"No memory wrapper for type: {type}");

			return marshalerLookup[type];
		}

		private void TickMarshalersThread()
		{
			try
			{
				while (this.isActive)
				{
					Thread.Sleep(16);

					if (!ProcessIsAlive)
						return;

					tickCount++;
					MarshalerBase.TickAllActive();
				}

				MarshalerBase.DisposeAll();
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Marshaler thread exception", ex));
			}
		}

		private void ProcessWatcherThread()
		{
			while (this.isActive && Process != null)
			{
				ProcessIsAlive = IsAlive;

				if (!ProcessIsAlive)
				{
					Log.Write(new Exception("FFXIV Process has terminated"));
				}

				Thread.Sleep(100);
			}
		}

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
