// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core;
#if !DEBUG
using Anamnesis.GUI.Dialogs;
#endif
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
using static Anamnesis.Memory.NativeFunctions;

/// <summary>
/// A service that handles memory operations on the game process.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class MemoryService : ServiceBase<MemoryService>
{
	/// <summary>
	/// The maximum number of attempts to read from memory before failing.
	/// </summary>
	private const int MAX_READ_ATTEMPTS = 10;

	/// <summary>
	/// The number of milliseconds to wait between read attempts.
	/// </summary>
	private const int TIME_BETWEEN_READ_ATTEMPTS = 10;

	/// <summary>
	/// The interval in milliseconds to wait for process refresh checks.
	/// </summary>
	private const int PROCESS_REFRESH_TIMER_INTERVAL = 100;

	/// <summary>
	/// The interval in milliseconds to wait for before doing a process recovery attempt.
	/// </summary>
	private const int PROCESS_WATCHDOG_INTERVAL = 10000;

	/// <summary>
	/// A dictionary to store the base addresses of loaded modules by their names.
	/// </summary>
	private readonly Dictionary<string, IntPtr> modules = [];

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
			if (!Instance.IsInitialized)
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
		unsafe
		{
			fixed (byte* ptr = d)
			{
				NtReadVirtualMemory(Handle, address, (nint)ptr, d.Length, out _);
			}
		}

		long i = BitConverter.ToInt64(d, 0);
		IntPtr ptrResult = checked((IntPtr)i);
		return ptrResult;
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
			IntPtr ptr = checked((IntPtr)address.ToPointer());
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
			throw new ArgumentException("Invalid address", nameof(address));

		int attempt = 0;
		int size = Marshal.SizeOf<T>();
		Span<byte> buffer = stackalloc byte[size];

		while (attempt < MAX_READ_ATTEMPTS)
		{
			unsafe
			{
				fixed (byte* ptr = buffer)
				{
					if (NtReadVirtualMemory(Handle, address, (nint)ptr, size, out _) == (uint)NtStatus.STATUS_SUCCESS)
					{
						return MemoryMarshal.Read<T>(buffer);
					}
				}
			}

			attempt++;
			Thread.Sleep(TIME_BETWEEN_READ_ATTEMPTS);
		}

		throw new InvalidOperationException($"Failed to read memory {typeof(T)} from address {address}");
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
			throw new ArgumentException("Invalid address", nameof(address));

		Type readType = type;

		if (type.IsEnum)
			readType = type.GetEnumUnderlyingType();
		else if (type == typeof(bool))
			readType = typeof(OneByteBool);

		int attempt = 0;
		int size = Marshal.SizeOf(readType);
		Span<byte> buffer = stackalloc byte[size];

		while (attempt < MAX_READ_ATTEMPTS)
		{
			unsafe
			{
				fixed (byte* ptr = buffer)
				{
					if (NtReadVirtualMemory(Handle, address, (nint)ptr, size, out _) == (uint)NtStatus.STATUS_SUCCESS)
					{
						object? val = Marshal.PtrToStructure((IntPtr)ptr, readType);

						if (val == null)
							continue;

						if (type.IsEnum)
							return Enum.ToObject(type, val);

						if (val is OneByteBool obb)
							return obb.Value;

						return val;
					}
				}
			}

			attempt++;
			Thread.Sleep(TIME_BETWEEN_READ_ATTEMPTS);
		}

		throw new InvalidOperationException($"Failed to read memory {type} from address {address}");
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
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtReadVirtualMemory(Handle, (nint)address, (nint)ptr, (int)size, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
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

		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtReadVirtualMemory(Handle, (nint)address, (nint)ptr, size, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
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
			return NtReadVirtualMemory(Handle, address, (nint)ptr, buffer.Length, out _) == (uint)NtStatus.STATUS_SUCCESS;
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
		Span<byte> buffer = stackalloc byte[1];
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				NtReadVirtualMemory(Handle, baseAddress + offset, (nint)ptr, buffer.Length, out _);
			}
		}

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
		Span<byte> buffer = stackalloc byte[2];
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				NtReadVirtualMemory(Handle, baseAddress + offset, (nint)ptr, buffer.Length, out _);
			}
		}

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
		Span<byte> buffer = stackalloc byte[4];
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				NtReadVirtualMemory(Handle, baseAddress + offset, (nint)ptr, buffer.Length, out _);
			}
		}

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
		Span<byte> buffer = stackalloc byte[8];
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				NtReadVirtualMemory(Handle, baseAddress + offset, (nint)ptr, buffer.Length, out _);
			}
		}

		return BitConverter.ToInt64(buffer);
	}

	/// <summary>
	/// Writes a byte array to a specified memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The byte array to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, byte[] buffer)
	{
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtWriteVirtualMemory(Handle, address, (nint)ptr, buffer.Length, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
	}

	/// <summary>
	/// Writes a span buffer to a specified memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The span buffer to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, Span<byte> buffer)
	{
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtWriteVirtualMemory(Handle, address, (nint)ptr, buffer.Length, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
	}

	/// <summary>
	/// Writes a byte array to a specified memory address with executable permissions.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The byte array to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	/// <remarks>
	/// Used for writing executable code to memory. Changes memory protection to allow writing.
	/// </remarks>
	public static bool WriteExecutable(IntPtr address, byte[] buffer)
	{
		VirtualProtectEx(Handle, address, buffer.Length, (uint)MemoryProtectionType.PAGE_EXECUTE_READWRITE, out _);

		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtWriteVirtualMemory(Handle, address, (nint)ptr, buffer.Length, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
	}

	/// <summary>
	/// Writes a span buffer to a specified memory address with executable permissions.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The span buffer to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	/// <remarks>
	/// Used for writing executable code to memory. Changes memory protection to allow writing.
	/// </remarks>
	public static bool WriteExecutable(IntPtr address, Span<byte> buffer)
	{
		VirtualProtectEx(Handle, address, buffer.Length, (uint)MemoryProtectionType.PAGE_EXECUTE_READWRITE, out _);

		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtWriteVirtualMemory(Handle, address, (nint)ptr, buffer.Length, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
	}

	/// <summary>
	/// Writes a value of a specified type to a given memory address.
	/// </summary>
	/// <typeparam name="T">The type of the value to write. Must be a struct.</typeparam>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The value to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write<T>(IntPtr address, T value)
		where T : struct
	{
		return Write(address, value, typeof(T));
	}

	/// <summary>
	/// Writes an object value to a given memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The object value to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, object value)
	{
		return Write(address, value, value.GetType());
	}

	/// <summary>
	/// Writes an object value of a specified type to a given memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="type">The type of the value to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	public static bool Write(IntPtr address, object value, Type type)
	{
		if (address == IntPtr.Zero)
			return false;

		if (type.IsEnum)
			type = Enum.GetUnderlyingType(type);

		switch (Type.GetTypeCode(type))
		{
			case TypeCode.Boolean:
				return WriteWithStackAlloc(address, (byte)((bool)value ? 1 : 0), 1);
			case TypeCode.Byte:
				return WriteWithStackAlloc(address, (byte)value, 1);
			case TypeCode.Int16:
				return WriteWithStackAlloc(address, (short)value, 2);
			case TypeCode.UInt16:
				return WriteWithStackAlloc(address, (ushort)value, 2);
			case TypeCode.Int32:
				return WriteWithStackAlloc(address, (int)value, 4);
			case TypeCode.UInt32:
				return WriteWithStackAlloc(address, (uint)value, 4);
			case TypeCode.Int64:
				return WriteWithStackAlloc(address, (long)value, 8);
			case TypeCode.UInt64:
				return WriteWithStackAlloc(address, (ulong)value, 8);
			case TypeCode.Single:
				return WriteWithStackAlloc(address, (float)value, 4);
			case TypeCode.Double:
				return WriteWithStackAlloc(address, (double)value, 8);
			default:
				byte[] buffer = MarshalToByteArray(value, type);
				return Write(address, buffer);
		}
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
		string gameVer = await Task.Run(() => File.ReadAllText(file));

		Log.Information($"Found game version: {gameVer}");

		if (gameVer != VersionInfo.ValidatedGameVersion)
		{
			Log.Warning($"Unrecognized game version: {gameVer}. Current validated version is: {VersionInfo.ValidatedGameVersion}");
#if !DEBUG
			await GenericDialog.ShowLocalizedAsync("Error_WrongVersion", "Error_WrongVersionTitle");
#endif
		}

		SetCompatibilityLayer();
		ClaimSeDebugPrivilege();
		Handle = NativeFunctions.OpenProcess(0x001F0FFF, true, process.Id);
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

		var injector = new Injector(Process);
		injector.Inject();
	}

	/// <summary>
	/// Converts a value of a specified type to a byte array using marshaling.
	/// </summary>
	/// <param name="value">The value to marshal.</param>
	/// <param name="type">The type of the value to marshal.</param>
	/// <returns>A byte array containing the marshaled value.</returns>
	/// <exception cref="Exception"> Thrown if the marshaling operation fails.</exception>
	private static byte[] MarshalToByteArray(object value, Type type)
	{
		int size = Marshal.SizeOf(type);
		byte[] buffer = new byte[size];
		IntPtr mem = Marshal.AllocHGlobal(size);
		try
		{
			Marshal.StructureToPtr(value, mem, false);
			Marshal.Copy(mem, buffer, 0, size);
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to marshal type: {type} to memory", ex);
		}
		finally
		{
			Marshal.FreeHGlobal(mem);
		}

		return buffer;
	}

	private static bool WriteWithStackAlloc<T>(IntPtr address, T val, int size)
		where T : unmanaged
	{
		Span<byte> buffer = stackalloc byte[size];
		MemoryMarshal.Write(buffer, in val);
		return Write(address, buffer);
	}

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

		if (SettingsService.Instance.IsInitialized)
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
		bool servicesWereShutdown = false;

		while (this.IsInitialized && Process != null)
		{
			await Task.Delay(PROCESS_REFRESH_TIMER_INTERVAL);

			if (!IsProcessAlive)
			{
				try
				{
					// Shutdown all services only once per process death
					if (!servicesWereShutdown)
					{
						TargetService.Instance.ClearSelection();
						await ServiceManager.ShutdownServices();
						servicesWereShutdown = true;
					}

					Log.Information("FFXIV Process has terminated");
					await Task.Delay(PROCESS_WATCHDOG_INTERVAL);
					await this.GetProcess();

					// If process is restored, start all services again
					if (IsProcessAlive && servicesWereShutdown)
					{
						await ServiceManager.StartServices();
						servicesWereShutdown = false;
					}
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
