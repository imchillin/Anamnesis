// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core.Memory;
using Anamnesis.GUI.Dialogs;
using Anamnesis.GUI.Windows;
using Anamnesis.Keyboard;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public class MemoryService : ServiceBase<MemoryService>
{
	/// <summary>
	/// The memory protection constant for read, write, and execute permissions.
	/// </summary>
	private const uint VirtualProtectReadWriteExecute = 0x40;

	/// <summary>
	/// The maximum number of attempts to read from memory before failing.
	/// </summary>
	private const int MaxReadAttempts = 10;

	/// <summary>
	/// The number of milliseconds to wait between read attempts.
	/// </summary>
	private const int TimeBetweenReadAttempts = 20;

	/// <summary>
	/// The interval in milliseconds to wait for process refresh checks.
	/// </summary>
	private const int ProcessRefreshTimerInterval = 100;

	/// <summary>
	/// The interval in milliseconds to wait for before doing a process recovery attempt.
	/// </summary>
	private const int ProcessWatchdogInterval = 10000;

	/// <summary>
	/// A dictionary to store the base addresses of loaded modules by their names.
	/// </summary>
	private readonly Dictionary<string, IntPtr> modules = new();

	/// <summary>Gets the handle to the opened process.</summary>
	public static IntPtr Handle { get; private set; }

	/// <summary>Gets the signature scanner for the process.</summary>
	public static SignatureScanner? Scanner { get; private set; }

	/// <summary>Gets the process being managed.</summary>
	public static Process? Process { get; private set; }

	/// <summary>Gets a value indicating whether the process is alive.</summary>
	public static bool IsProcessAlive
	{
		get
		{
			if (!Instance.IsAlive)
				return false;

			if (Process == null || Process.HasExited)
				return false;

			if (!Process.Responding)
				return false;

			return true;
		}
	}

	/// <summary>Gets a value indicating whether the process has window focus.</summary>
	public static bool DoesProcessHaveFocus
	{
		get
		{
			if (Process == null)
				return false;

			IntPtr wnd = GetForegroundWindow();
			return wnd == Process.MainWindowHandle;
		}
	}

	/// <summary>
	/// Gets the path to the game directory.
	/// </summary>
	/// <exception cref="Exception">Thrown if the game process or its main module are not available.</exception>
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

	/// <summary>
	/// Reads a pointer from the specified memory address.
	/// </summary>
	/// <param name="address">The memory address to read the pointer from.</param>
	/// <returns>The pointer read from the specified memory address.</returns>
	public static IntPtr ReadPtr(IntPtr address)
	{
		byte[] d = new byte[8];
		ReadProcessMemory(Handle, address, d, 8, out _);
		long i = BitConverter.ToInt64(d, 0);
		IntPtr ptr = (IntPtr)i;
		return ptr;
	}

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> from the specified memory address.
	/// </summary>
	/// <typeparam name="T">The type of value to read. Must be a struct.</typeparam>
	/// <param name="address">The memory address to read the value from.</param>
	/// <returns>The value read from the specified memory address, or null if the read fails.</returns>
	/// <exception cref="Exception">Thrown if the specified memory address is invalid.</exception>
	public static T? Read<T>(UIntPtr address)
		where T : struct
	{
		unsafe
		{
			IntPtr ptr = (IntPtr)address.ToPointer();
			return Read<T>(ptr);
		}
	}

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> from the specified memory address.
	/// </summary>
	/// <typeparam name="T">The type of value to read. The type must be a struct.</typeparam>
	/// <param name="address">The memory address to read the value from.</param>
	/// <returns>The value read from the specified memory address.</returns>
	/// <exception cref="Exception">Thrown if the address is invalid or the read operation fails after multiple attempts.</exception>
	public static T Read<T>(IntPtr address)
		where T : struct
	{
		if (address == IntPtr.Zero)
			throw new Exception("Invalid address");

		int attempt = 0;
		while (attempt < MaxReadAttempts)
		{
			int size = Marshal.SizeOf<T>();
			IntPtr mem = Marshal.AllocHGlobal(size);
			ReadProcessMemory(Handle, address, mem, size, out _);
			T? val = Marshal.PtrToStructure<T>(mem);
			Marshal.FreeHGlobal(mem);
			attempt++;

			if (val != null)
				return (T)val;

			Thread.Sleep(TimeBetweenReadAttempts);
		}

		throw new Exception($"Failed to read memory {typeof(T)} from address {address}");
	}

	/// <summary>
	/// Reads a value of the specified type from the given memory address.
	/// </summary>
	/// <param name="address">The memory address to read the value from.</param>
	/// <param name="type">The type of value to read.</param>
	/// <returns>The value read from the specified memory address.</returns>
	/// <exception cref="Exception">Thrown if the address is invalid or the read operation fails after multiple attempts.</exception>
	public static object Read(IntPtr address, Type type)
	{
		if (address == IntPtr.Zero)
			throw new Exception("Invalid address");

		Type readType = type;

		if (type.IsEnum)
			readType = type.GetEnumUnderlyingType();

		if (type == typeof(bool))
			readType = typeof(OneByteBool);

		for (int attempt = 0; attempt < MaxReadAttempts; attempt++)
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

			Thread.Sleep(TimeBetweenReadAttempts);
		}

		throw new Exception($"Failed to read memory {type} from address {address}");
	}

	/// <summary>
	/// Reads memory from the specified address into the provided buffer.
	/// </summary>
	/// <param name="address">The address to read from.</param>
	/// <param name="buffer">The buffer to store the read data.</param>
	/// <param name="size">The size of the data to read.</param>
	/// <returns>True if the read operation was successful; otherwise, False.</returns>
	public static bool Read(UIntPtr address, byte[] buffer, UIntPtr size)
	{
		return ReadProcessMemory(Handle, address, buffer, size, IntPtr.Zero);
	}

	/// <summary>
	/// Reads memory from the specified address into the provided buffer.
	/// </summary>
	/// <param name="address">The address to read from.</param>
	/// <param name="buffer">The buffer to store the read data.</param>
	/// <param name="size">The size of the data to read. If less than or equal to 0, the buffer length is used.</param>
	/// <returns>True if the read operation was successful; otherwise, False.</returns>
	public static bool Read(IntPtr address, byte[] buffer, int size = -1)
	{
		if (size <= 0)
			size = buffer.Length;

		return ReadProcessMemory(Handle, address, buffer, size, out _);
	}

	/// <summary>
	/// Reads memory from the specified address into the provided span buffer.
	/// </summary>
	/// <param name="address">The address to read from.</param>
	/// <param name="buffer">The span buffer to store the read data.</param>
	/// <returns>True if the read operation was successful; otherwise, False.</returns>
	public static unsafe bool Read(IntPtr address, Span<byte> buffer)
	{
		fixed (byte* ptr = buffer)
		{
			return ReadProcessMemory(Handle, address, (IntPtr)ptr, buffer.Length, out _);
		}
	}

	/// <summary>
	/// Reads a byte from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The byte value read from the specified address.</returns>
	public static byte ReadByte(IntPtr baseAddress, int offset = 0)
	{
		byte[] buffer = new byte[1];
		ReadProcessMemory(Handle, baseAddress + offset, buffer, 1, out _);
		return buffer[0];
	}

	/// <summary>
	/// Reads a 16-bit integer from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The 16-bit integer value read from the specified address.</returns>
	public static short ReadInt16(IntPtr baseAddress, int offset = 0)
	{
		byte[] buffer = new byte[2];
		ReadProcessMemory(Handle, baseAddress + offset, buffer, 2, out _);
		return BitConverter.ToInt16(buffer);
	}

	/// <summary>
	/// Reads a 32-bit integer from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The 32-bit integer value read from the specified address.</returns>
	public static int ReadInt32(IntPtr baseAddress, int offset = 0)
	{
		byte[] buffer = new byte[4];
		ReadProcessMemory(Handle, baseAddress + offset, buffer, 4, out _);
		return BitConverter.ToInt32(buffer);
	}

	/// <summary>
	/// Reads a 64-bit integer from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The 64-bit integer value read from the specified address.</returns>
	public static long ReadInt64(IntPtr baseAddress, int offset = 0)
	{
		byte[] buffer = new byte[8];
		ReadProcessMemory(Handle, baseAddress + offset, buffer, 8, out _);
		return BitConverter.ToInt64(buffer);
	}

	/// <summary>
	/// Writes a byte array to a specified memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The byte array to write.</param>
	/// <param name="writingCode">Indicates whether the write operation involves writing executable code.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, byte[] buffer, bool writingCode)
	{
		if (writingCode)
			VirtualProtectEx(Handle, address, buffer.Length, VirtualProtectReadWriteExecute, out _);

		return WriteProcessMemory(Handle, address, buffer, buffer.Length, out _);
	}

	/// <summary>
	/// Writes a value of a specified type to a given memory address.
	/// </summary>
	/// <typeparam name="T">The type of the value to write. Must be a struct.</typeparam>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The value to write.</param>
	/// <param name="reason">The reason for writing the value.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write<T>(IntPtr address, T value, string reason)
		where T : struct
	{
		return Write(address, value, typeof(T), reason);
	}

	/// <summary>
	/// Writes an object value to a given memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="reason">The reason for writing the value.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, object value, string reason)
	{
		return Write(address, value, value.GetType(), reason);
	}

	/// <summary>
	/// Writes an object value of a specified type to a given memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="type">The type of the value to write.</param>
	/// <param name="reason">The reason for writing the value.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, object value, Type type, string reason)
	{
		if (address == IntPtr.Zero)
			return false;

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

		Log.Verbose($"Writing: {buffer.Length} bytes to {address} for type {type.Name} for reason: {reason}");
		return Write(address, buffer, false);
	}

	/// <summary>
	/// Sends a key press or release event to the main window of the attached process.
	/// </summary>
	/// <param name="key">The key to be sent.</param>
	/// <param name="state">The state of the key (pressed or released).</param>
	/// <remarks>
	/// This method handles special cases for the Shift, Ctrl, and Alt keys to ensure the
	/// correct virtual key code is used.
	/// </remarks>
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

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		await base.Initialize();
		await this.GetProcess();

		_ = Task.Run(this.ProcessWatcherTask);
	}

	/// <inheritdoc/>
	public override async Task Start()
	{
		await base.Start();
	}

	/// <summary>
	/// Opens the specified game process with all necessary security and access rights.
	/// </summary>
	/// <param name="process">The process to be opened.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="Exception">Thrown if the target process is not responding or has no main module.</exception>
	public async Task OpenProcess(Process process)
	{
		Debug.Assert(process != null, "Process is null");

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

	/// <summary>
	/// Attempts to find and open the game process, setting it as the current process.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
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
	}

	/// <summary>
	/// Monitors the game process to ensure it is still running, and attempts to reopen it if it terminates.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task ProcessWatcherTask()
	{
		while (this.IsAlive && Process != null)
		{
			await Task.Delay(ProcessRefreshTimerInterval);

			if (!IsProcessAlive)
			{
				try
				{
					Log.Information("FFXIV Process has terminated");
					await Task.Delay(ProcessWatchdogInterval);
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

					Log.Error(ex, "Unable to get FFXIV process");
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Unable to get FFXIV process");
				}
			}
		}
	}

	/// <summary>
	/// Special struct for handling 1-byte bool marshaling.
	/// </summary>
	private struct OneByteBool
	{
#pragma warning disable CS0649
		[MarshalAs(UnmanagedType.I1)]
		public bool Value;
#pragma warning restore CS0649
	}
}
