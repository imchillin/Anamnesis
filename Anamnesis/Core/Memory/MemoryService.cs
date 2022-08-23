// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Anamnesis.Core.Memory;
using Anamnesis.GUI.Dialogs;
using Anamnesis.GUI.Windows;
using Anamnesis.Keyboard;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public class MemoryService : ServiceBase<MemoryService>
{
	private const uint VirtualProtectReadWriteExecute = 0x40;

	private readonly Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

	public static IntPtr Handle { get; private set; }
	public static SignatureScanner? Scanner { get; private set; }
	public static Process? Process { get; private set; }
	public static bool IsProcessAlive { get; private set; }
	public static bool DoesProcessHaveFocus { get; private set; }

	public static string GamePath
	{
		get
		{
			if (SettingsService.Current.GamePath != null)
				return SettingsService.Current.GamePath;

			if (Process == null)
				throw new Exception("No game process");

			if (Process.MainModule == null)
				throw new Exception("Process has no main module");

			return Path.GetDirectoryName(Process.MainModule.FileName) + "\\..\\";
		}
	}

	public int LastTickCount { get; set; }

	public static bool GetDoesProcessHaveFocus()
	{
		if (Process == null)
			return false;

		IntPtr wnd = GetForegroundWindow();
		return wnd == Process.MainWindowHandle;
	}

	public static bool GetIsProcessAlive()
	{
		if (!Instance.IsAlive)
			return false;

		if (Process == null || Process.HasExited)
			return false;

		if (!Process.Responding)
			return false;

		return true;
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

	public static T Read<T>(IntPtr address)
		where T : struct
	{
		if (address == IntPtr.Zero)
			throw new Exception("Invalid address");

		int attempt = 0;
		while (attempt < 10)
		{
			int size = Marshal.SizeOf(typeof(T));
			IntPtr mem = Marshal.AllocHGlobal(size);
			ReadProcessMemory(Handle, address, mem, size, out _);
			T? val = Marshal.PtrToStructure<T>(mem);
			Marshal.FreeHGlobal(mem);
			attempt++;

			if (val != null)
				return (T)val;

			Thread.Sleep(100);
		}

		throw new Exception($"Failed to read memory {typeof(T)} from address {address}");
	}

	public static object Read(IntPtr address, Type type)
	{
		if (address == IntPtr.Zero)
			throw new Exception("Invalid address");

		Type readType = type;

		if (type.IsEnum)
			readType = type.GetEnumUnderlyingType();

		if (type == typeof(bool))
			readType = typeof(OneByteBool);

		for (int attempt = 0; attempt < 10; attempt++)
		{
			int size = Marshal.SizeOf(readType);
			IntPtr mem = Marshal.AllocHGlobal(size);

			if (ReadProcessMemory(Handle, address, mem, size, out _))
			{
				object? val = Marshal.PtrToStructure(mem, readType);
				Marshal.FreeHGlobal(mem);

				if (val == null)
					continue;

				if (type.IsEnum)
					val = Enum.ToObject(type, val);

				if (val is OneByteBool obb)
					return obb.Value;

				return val;
			}

			Thread.Sleep(16);
		}

		throw new Exception($"Failed to read memory {type} from address {address}");
	}

	public static void Write<T>(IntPtr address, T value, string purpose)
		where T : struct
	{
		Write(address, value, typeof(T), purpose);
	}

	public static void Write(IntPtr address, object value, string purpose)
	{
		Write(address, value, value.GetType(), purpose);
	}

	public static void Write(IntPtr address, object value, Type type, string purpose)
	{
		if (address == IntPtr.Zero)
			return;

		if (type.IsEnum)
			type = type.GetEnumUnderlyingType();

		byte[] buffer;

		if (type == typeof(bool))
		{
			buffer = new[] { (byte)((bool)value == true ? 1 : 0) };
		}
		else if (type == typeof(byte))
		{
			buffer = new[] { (byte)value };
		}
		else if (type == typeof(int))
		{
			buffer = BitConverter.GetBytes((int)value);
		}
		else if (type == typeof(uint))
		{
			buffer = BitConverter.GetBytes((uint)value);
		}
		else if (type == typeof(short))
		{
			buffer = BitConverter.GetBytes((short)value);
		}
		else if (type == typeof(ushort))
		{
			buffer = BitConverter.GetBytes((ushort)value);
		}
		else
		{
			try
			{
				int size = Marshal.SizeOf(type);
				buffer = new byte[size];
				IntPtr mem = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(value, mem, false);
				Marshal.Copy(mem, buffer, 0, size);
				Marshal.FreeHGlobal(mem);
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to marshal type: {type} to memory", ex);
			}
		}

		Log.Verbose($"Writing: {buffer.Length} bytes to {address} for type {type.Name} for reason: {purpose}");
		Write(address, buffer, false);
	}

	public static bool Read(UIntPtr address, byte[] buffer, UIntPtr size)
	{
		return ReadProcessMemory(Handle, address, buffer, size, IntPtr.Zero);
	}

	public static bool Read(IntPtr address, byte[] buffer, int size = -1)
	{
		if (size <= 0)
			size = buffer.Length;

		return ReadProcessMemory(Handle, address, buffer, size, out _);
	}

	public static bool Write(IntPtr address, byte[] buffer, bool writingCode)
	{
		if(writingCode)
			VirtualProtectEx(Handle, address, buffer.Length, VirtualProtectReadWriteExecute, out _);

		return WriteProcessMemory(Handle, address, buffer, buffer.Length, out _);
	}

	public static void SendKey(Key key, KeyboardKeyStates state)
	{
		if (Process == null)
			return;

		int vkey = KeyInterop.VirtualKeyFromKey(key);

		if (key == Key.LeftShift || key == Key.RightShift)
			vkey = 0x10;

		if (key == Key.LeftCtrl || key == Key.RightCtrl)
			vkey = 0x11;

		if (key == Key.LeftAlt || key == Key.RightAlt)
			vkey = 0x12;

		if (state == KeyboardKeyStates.Pressed)
		{
			PostMessage(Process.MainWindowHandle, 0x100, (IntPtr)vkey, IntPtr.Zero);
		}
		else if (state == KeyboardKeyStates.Released)
		{
			PostMessage(Process.MainWindowHandle, 0x0101, (IntPtr)vkey, IntPtr.Zero);
		}
	}

	public override async Task Initialize()
	{
		await base.Initialize();
		await this.GetProcess();

		_ = Task.Run(this.ProcessWatcherTask);
	}

	public override async Task Start()
	{
		await base.Start();
	}

	/// <summary>
	/// Open the PC game process with all security and access rights.
	/// </summary>
	public async Task OpenProcess(Process process)
	{
		Process = process;

		Log.Information($"Opening game process: {process.MainModule?.FileName}");

		if (!Process.Responding)
			throw new Exception("Target process id not responding");

		if (process.MainModule == null)
			throw new Exception("Process has no main module");

		// checke the game version as soon as we can
		string file = MemoryService.GamePath + "game/ffxivgame.ver";
		string gameVer = File.ReadAllText(file);

		Log.Information($"Found game version: {gameVer}");

		if (gameVer != VersionInfo.ValidatedGameVersion)
		{
			Log.Warning($"Unrecognized game version: {gameVer}. Current validated version is: {VersionInfo.ValidatedGameVersion}");
			await GenericDialog.ShowLocalizedAsync("Error_WrongVersion", "Error_WrongVersionTitle");
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

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

	[DllImport("kernel32.dll")]
	private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

	[DllImport("kernel32.dll")]
	private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

	[DllImport("kernel32.dll")]
	private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

	[DllImport("kernel32.dll")]
	private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

	[DllImport("kernel32.dll")]
	private static extern int CloseHandle(IntPtr hObject);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	private async Task GetProcess()
	{
		Process? proc = null;

		await Dispatch.MainThread();

		if (App.Current == null)
			return;

		App.Current.MainWindow.Topmost = false;

		proc = ProcessSelector.FindProcess();

		if (SettingsService.Exists)
			App.Current.MainWindow.Topmost = SettingsService.Current.AlwaysOnTop;

		await Dispatch.NonUiThread();

		// if still no process, shutdown.
		if (proc == null)
		{
			await Dispatch.MainThread();
			App.Current.MainWindow.Close();
			App.Current.Shutdown();
			return;
		}

		if (proc != null)
			await this.OpenProcess(proc);

		IsProcessAlive = true;
	}

	private async Task ProcessWatcherTask()
	{
		while (this.IsAlive && Process != null)
		{
			await Task.Delay(100);

			DoesProcessHaveFocus = GetDoesProcessHaveFocus();
			IsProcessAlive = GetIsProcessAlive();

			if (!IsProcessAlive)
			{
				try
				{
					Log.Information("FFXIV Process has terminated");
					await Task.Delay(10000);
					TargetService.Instance.ClearSelection();
					await this.GetProcess();
				}
				catch (Win32Exception)
				{
					// Ignore "Only part of a readmemory operation completed errors, caused by reading memory while the game is shutting down.
				}
				catch (AggregateException ex)
				{
					// Ignore "Only part of a readmemory operation completed errors, caused by reading memory while the game is shutting down.
					if (ex.InnerException is Win32Exception)
						continue;

					Log.Error(ex, "Unable to get ffxiv process");
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Unable to get ffxiv process");
				}
			}
		}
	}

	// Special struct for handling 1 byte bool marshaling
	private struct OneByteBool
	{
#pragma warning disable CS0649
		[MarshalAs(UnmanagedType.I1)]
		public bool Value;
	}
}
