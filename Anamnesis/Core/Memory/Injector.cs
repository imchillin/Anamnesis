// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Iced.Intel;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Anamnesis.Memory.NativeFunctions;
using static Iced.Intel.AssemblerRegisters;

// TODO: Use GameService's IsSignedIn as a trigger to load in the remote controller
// TODO: Check if the remote controller is already active before attempting to inject again

/// <summary>
/// A class to inject and run embedded resource DLLs into a target process.
/// The injected DLLs are expected to manage their own lifecycle and cleanup, including
/// memory deallocation upon unloading.
/// </summary>
internal sealed class Injector : IDisposable
{
	private const int DLL_DELETE_RETRY_COUNT = 5;
	private const int DLL_DELETE_RETRY_DELAY_MS = 500;

	private readonly Process targetProcess;
	private readonly IntPtr loadLibraryFuncAddr;
	private readonly IntPtr getProcAddressFuncAddr;
	private readonly IntPtr freeLibraryFuncAddr;
	private string? remoteCtrlDllPath;
	private bool isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="Injector"/> class.
	/// </summary>
	/// <remarks>
	/// The provided process must already be opened with sufficient access rights to perform the
	/// injection and execution operations defined in this class.
	/// </remarks>
	/// <param name="process">
	/// The process to be targeted for injection. Must be opened with the required access rights.
	/// </param>
	/// <exception cref="Exception">
	/// Thrown if the internally used kernel32 dll modules could not be located.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the provided process is null.
	/// </exception>
	public Injector(Process process)
	{
		ArgumentNullException.ThrowIfNull(process);
		this.targetProcess = process;

		IntPtr kernel32 = GetModuleHandle("kernel32.dll");
		if (kernel32 == IntPtr.Zero)
			throw new InvalidOperationException("Could not locate kernel32 DLL module");

		this.loadLibraryFuncAddr = GetProcAddress(kernel32, "LoadLibraryW");
		this.getProcAddressFuncAddr = GetProcAddress(kernel32, "GetProcAddress");
		this.freeLibraryFuncAddr = GetProcAddress(kernel32, "FreeLibrary");

		if (this.loadLibraryFuncAddr == IntPtr.Zero)
			throw new InvalidOperationException("Could not locate LoadLibraryW export in kernel32 DLL");

		if (this.getProcAddressFuncAddr == IntPtr.Zero)
			throw new InvalidOperationException("Could not locate GetProcAddress export in kernel32 DLL");

		if (this.freeLibraryFuncAddr == IntPtr.Zero)
			throw new InvalidOperationException("Could not locate FreeLibrary export in kernel32 DLL");
	}

	/// <summary>
	/// Finalizes an instance of the <see cref="Injector"/> class.
	/// </summary>
	~Injector() => this.Dispose(false);

	/// <inheritdoc/>
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Injects a managed DLL resource into the target process and invokes a specified
	/// entry point within the injected module.
	/// </summary>
	/// <remarks>
	/// The method does not block for the completion of the remote entry point execution.
	/// </remarks>
	/// <param name="resourceName">
	/// The name of the embedded DLL resource to extract and inject.
	/// Cannot be null, empty, or consist only of white-space characters.
	/// </param>
	/// <param name="entryPoint">
	/// The name of the exported entry point method to invoke within the injected DLL.
	/// Cannot be null, empty, or consist only of white-space characters.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the DLL fails to load in the target process or if the injection
	/// process encounters an error.
	/// </exception>
	public void Inject(string resourceName, string entryPoint)
	{
		ObjectDisposedException.ThrowIf(this.isDisposed, this);
		ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
		ArgumentException.ThrowIfNullOrWhiteSpace(entryPoint);

		bool dllInjected = false;
		IntPtr moduleBaseAddress = IntPtr.Zero;

		try
		{
			this.remoteCtrlDllPath = ExtractResourceToTemp(resourceName);
			Log.Information($"[Injector]  Extracted resource to temp: {this.remoteCtrlDllPath}");
			this.InjectDll(this.remoteCtrlDllPath);
			Log.Information($"[Injector] Injected DLL into process {this.targetProcess.Id}");

			// Try to locate the injected DLL in the target process to validate that it loaded successfully
			this.targetProcess.Refresh();
			var module = this.targetProcess.Modules.Cast<ProcessModule>()
				.FirstOrDefault(m => m.FileName.Equals(this.remoteCtrlDllPath, StringComparison.OrdinalIgnoreCase));

			if (module == null)
				throw new InvalidOperationException("Module load failed silently in target process");

			moduleBaseAddress = module.BaseAddress;
			dllInjected = true;
			Log.Information($"[Injector] Module loaded at base address 0x{moduleBaseAddress.ToInt64():X}");

			// Run the entry point asynchronously
			Task.Run(() => this.CallExport(moduleBaseAddress, entryPoint))
				.ContinueWith(t => Log.Error(t.Exception, "Failed to start up entry point"), TaskContinuationOptions.OnlyOnFaulted);
		}
		catch (Exception ex)
		{
			// If the DLL was injected but we encountered an error afterwards, attempt to unload it
			if (dllInjected && moduleBaseAddress != IntPtr.Zero)
			{
				try
				{
					this.RunRemoteThread(this.freeLibraryFuncAddr, moduleBaseAddress);
					Log.Information("[Injector] Successfully unloaded DLL after injection failure");
				}
				catch (Exception unloadEx)
				{
					Log.Error(unloadEx, "Failed to unload injected DLL after injection failure");
				}
			}

			throw new InvalidOperationException($"Injection failed: {ex.Message}", ex);
		}
	}

	private static string ExtractResourceToTemp(string resourceName)
	{
		var assembly = Assembly.GetExecutingAssembly();
		using Stream? stream = assembly.GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException($"Resource '{resourceName}' not found in assembly.");

		// Generate a unique temporary file path
		string tempPath = Path.Combine(Path.GetTempPath(), $"anam_ctrl_{Guid.NewGuid():N}.dll");
		using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
		stream.CopyTo(fileStream);

		return tempPath;
	}

	private static void TryDeleteFile(string path)
	{
		if (!File.Exists(path))
			return;

		for (int i = 0; i < DLL_DELETE_RETRY_COUNT; i++)
		{
			try
			{
				File.Delete(path);
				return;
			}
			catch
			{
				Thread.Sleep(DLL_DELETE_RETRY_DELAY_MS);
			}
		}

		// Fallback
		MoveFileEx(path, null, (uint)MoveFileFlag.MOVEFILE_DELAY_UNTIL_REBOOT);
		Log.Information($"[Injector] Could not delete temp DLL; Scheduled for deletion on OS reboot: {path}");
	}

	private void CallExport(IntPtr hModule, string funcName)
	{
		var nameBytes = Encoding.ASCII.GetBytes(funcName + '\0');
		int codeOffset = (nameBytes.Length + 15) & ~15; // Align to 16 bytes
		IntPtr remoteMem = this.Alloc((uint)codeOffset + 512); // [String] + [Code]
		if (remoteMem == IntPtr.Zero)
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to allocate remote memory for injection");

		try
		{
			if (!this.Write(remoteMem, nameBytes))
				throw new InvalidOperationException("Failed to write function name to remote memory");

			// Assemble shellcode
			var asm = new Assembler(64);
			var labelExit = asm.CreateLabel();

			// --- Shellcode Start ---
			// Align stack to 16-byte boundary + 32-byte shadow space
			asm.push(rbx);           // Save non-volatile register
			asm.sub(rsp, 32);        // Shadow space

			// Call GetProcAddress(hModule, funcName)
			asm.mov(rcx, (long)hModule);
			asm.mov(rdx, (long)remoteMem); // Pointer to function name string
			asm.mov(rax, (long)this.getProcAddressFuncAddr);
			asm.call(rax);

			// Check if GetProcAddress returned NULL
			// If NULL, exit with error code 0
			asm.test(rax, rax);
			asm.jz(labelExit);       // Jump to exit (rax is 0, which we will use as error code)

			// Function found
			asm.mov(rbx, rax);       // Save function pointer
			asm.call(rbx);           // Call the entry point

			// Set success code (1)
			asm.mov(rax, 1);
			asm.jmp(labelExit);

			// Restore stack and return
			asm.Label(ref labelExit);
			asm.add(rsp, 32);
			asm.pop(rbx);
			asm.ret();
			// --- Shellcode End ---

			// Encode shellcode
			using var ms = new MemoryStream();
			IntPtr codeAddr = remoteMem + codeOffset;
			asm.Assemble(new StreamCodeWriter(ms), (ulong)codeAddr);
			var code = ms.ToArray();

			if (!this.Write(codeAddr, code))
				throw new InvalidOperationException("Failed to write shellcode to remote memory");

			// Set shellcode access rights (W^X policy)
			if (!VirtualProtectEx(this.targetProcess.Handle, codeAddr, code.Length, (uint)MemoryProtectionType.PAGE_EXECUTE_READ, out _))
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to set memory protection");

			// Execute and check result
			uint exitCode = this.RunRemoteThread(codeAddr, IntPtr.Zero, false);
			if (exitCode == 0)
				throw new InvalidOperationException($"GetProcAddress could not find export '{funcName}'. Ensure the DLL is compiled with native AOT.");

			Log.Information($"[Injector] Remote entry point '{funcName}' invoked successfully");
		}
		finally
		{
			this.Free(remoteMem);
		}
	}

	private void InjectDll(string dllPath)
	{
		var pathBytes = Encoding.Unicode.GetBytes(dllPath + '\0');
		IntPtr remoteMem = this.Alloc((uint)pathBytes.Length);
		if (remoteMem == IntPtr.Zero)
			throw new InvalidOperationException("Failed to allocate remote memory for injection");

		try
		{
			if (!this.Write(remoteMem, pathBytes))
				throw new InvalidOperationException("Failed to write DLL path to remote memory");

			VirtualProtectEx(this.targetProcess.Handle, remoteMem, pathBytes.Length, (uint)MemoryProtectionType.PAGE_READONLY, out _);
			this.RunRemoteThread(this.loadLibraryFuncAddr, remoteMem);
		}
		finally
		{
			this.Free(remoteMem);
		}
	}

	private IntPtr Alloc(uint size)
	{
		return VirtualAllocEx(
			this.targetProcess.Handle,
			IntPtr.Zero,
			size,
			(uint)(MemoryAllocationType.MEM_COMMIT | MemoryAllocationType.MEM_RESERVE),
			(uint)MemoryProtectionType.PAGE_READWRITE);
	}

	private bool Write(IntPtr addr, byte[] buffer)
	{
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				return NtWriteVirtualMemory(this.targetProcess.Handle, addr, (nint)ptr, buffer.Length, out _) == (uint)NtStatus.STATUS_SUCCESS;
			}
		}
	}

	private void Free(IntPtr addr)
	{
		VirtualFreeEx(this.targetProcess.Handle, addr, 0, (uint)MemoryFreeType.MEM_RELEASE);
	}

	private uint RunRemoteThread(IntPtr startAddress, IntPtr parameter, bool waitForExit = true)
	{
		IntPtr hThread = CreateRemoteThread(this.targetProcess.Handle, IntPtr.Zero, 0, startAddress, parameter, 0, out _);
		if (hThread == IntPtr.Zero)
			throw new Win32Exception(Marshal.GetLastWin32Error());

		try
		{
			if (!waitForExit)
				return 1; // Non-zero value to indicate success

			uint waitResult = WaitForSingleObject(hThread, uint.MaxValue);
			if (waitResult != 0) // WAIT_OBJECT_0 is 0, all other exit codes are either an error or timeout
				throw new Win32Exception(Marshal.GetLastWin32Error(), $"WaitForSingleObject failed with result: {waitResult}");

			if (GetExitCodeThread(hThread, out uint exitCode))
				return exitCode;

			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to get thread exit code");
		}
		finally
		{
			CloseHandle(hThread);
		}
	}

#pragma warning disable IDE0060
	private void Dispose(bool disposing)
	{
		if (this.isDisposed)
			return;

		// Clean up the temp file if it exists
		if (this.remoteCtrlDllPath != null)
		{
			string path = this.remoteCtrlDllPath;
			Task.Run(() => TryDeleteFile(path));
		}

		this.isDisposed = true;
	}
#pragma warning restore IDE0060
}
