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
	private const string FASM_RESOURCE = $"Anamnesis.Memory.{FASM_RESOURCE_FILENAME}";
	private const string FASM_RESOURCE_FILENAME = "FASMX64.dll";
	private const string ANAM_CTRL_RESOURCE = "Anamnesis.Memory.RemoteController.dll";
	private const string ANAM_CTRL_RESOURCE_FILENAME = "ANAMCTRL.dll";
	private const string ANAM_CTRL_ENTRY_POINT = "RemoteControllerEntry";

	private readonly Process targetProcess;
	private readonly IntPtr loadLibraryFuncAddr;
	private readonly IntPtr getProcAddressFuncAddr;
	private readonly IntPtr freeLibraryFuncAddr;
	private string? remoteCtrlDllPath;
	private string? fasmDllPath;
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
	/// Injects the remote controller into the target process and invokes its
	/// entry point within the injected module.
	/// </summary>
	/// <remarks>
	/// The method does not block for the completion of the remote entry point execution.
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the DLL fails to load in the target process or if the injection
	/// process encounters an error.
	/// </exception>
	public void Inject()
	{
		ObjectDisposedException.ThrowIf(this.isDisposed, this);

		bool dllInjected = false;
		IntPtr moduleBaseAddress = IntPtr.Zero;

		try
		{
			string tempDir = Path.GetTempPath();
			this.remoteCtrlDllPath = ExtractResourceToDirectory(ANAM_CTRL_RESOURCE, tempDir, ANAM_CTRL_RESOURCE_FILENAME);
			Log.Information($"[Injector] Extracted remote controller DLL to: {this.remoteCtrlDllPath}");

			this.fasmDllPath = ExtractResourceToDirectory(FASM_RESOURCE, tempDir, FASM_RESOURCE_FILENAME);
			Log.Information($"[Injector] Extracted FASM DLL to: {this.fasmDllPath}");

			try
			{
				ApplyCustomAclToProcess(this.targetProcess.Handle);
				Log.Information("[Injector] Successfully applied custom ACL with target process.");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "[Injector] Failed to synchronize ACLs. Hook registrations in remote controller may fail.");
			}

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

			var policy = default(ProcessMitigationDynamicCodePolicy);
			if (GetProcessMitigationPolicy(this.targetProcess.Handle, PROCESS_DYNAMIC_CODE_POLICY, out policy, Marshal.SizeOf(policy)))
			{
				if (policy.ProhibitDynamicCode)
				{
					Log.Warning("Arbitrary Code Guard (ACG) is enabled. This may cause issues with remote controller.");
				}
			}

			// Run the entry point asynchronously
			Task.Run(() => this.CallExport(moduleBaseAddress, ANAM_CTRL_ENTRY_POINT))
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

	private static string ExtractResourceToDirectory(string resourceName, string targetDir, string fileName)
	{
		var assembly = Assembly.GetExecutingAssembly();
		using Stream stream = assembly.GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");

		// Calculate hash of the resource
		string currentHash;
		using (var sha256 = System.Security.Cryptography.SHA256.Create())
		{
			byte[] hashBytes = sha256.ComputeHash(stream);
			currentHash = Convert.ToHexStringLower(hashBytes);
		}

		stream.Position = 0;
		string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
		string ext = Path.GetExtension(fileName);

		// Scan existing Anamnesis resource files for a matching hash
		var existingFiles = Directory.GetFiles(targetDir, $"{nameNoExt}.*{ext}");
		foreach (var filePath in existingFiles)
		{
			if (FileMatchesHash(filePath, currentHash))
			{
				Log.Information($"[Injector] {fileName} hash matches existing file {Path.GetFileName(filePath)}. Reusing.");
				return filePath;
			}
		}

		string targetPath = Path.Combine(targetDir, fileName);
		bool useSuffixedName = false;
		if (File.Exists(targetPath))
		{
			try
			{
				File.Delete(targetPath);
			}
			catch
			{
				/* File is likely in use, continue with hash check */
				useSuffixedName = true;
			}
		}

		if (useSuffixedName)
		{
			string uniqueSuffix = DateTime.UtcNow.Ticks.ToString("x");
			targetPath = Path.Combine(targetDir, $"{nameNoExt}.{uniqueSuffix}{ext}");
		}

		using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
		stream.CopyTo(fileStream);

		Log.Information($"[Injector] Extracted new resource file: {Path.GetFileName(targetPath)}");
		return targetPath;
	}

	private static bool FileMatchesHash(string filePath, string expectedHash)
	{
		try
		{
			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			string actualHash = Convert.ToHexStringLower(sha256.ComputeHash(fs));
			return actualHash == expectedHash;
		}
		catch
		{
			return false; // Assume it's not a match on error
		}
	}

	private static void ScheduleForDeletion(string path)
	{
		if (!File.Exists(path))
			return;

		// Fallback
		MoveFileEx(path, null, (uint)MoveFileFlag.MOVEFILE_DELAY_UNTIL_REBOOT);
		Log.Information($"[Injector] Scheduled file for deletion on OS reboot: {path}");
	}

	private void CallExport(IntPtr hModule, string funcName)
	{
		string exportName = funcName.Length > 0 && funcName[^1] == '\0'
			? funcName
			: funcName + '\0';

		var nameBytes = Encoding.ASCII.GetBytes(exportName);
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
			uint exitCode = this.RunRemoteThread(codeAddr, IntPtr.Zero);
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

	private uint RunRemoteThread(IntPtr startAddress, IntPtr parameter)
	{
		IntPtr hThread = CreateRemoteThread(this.targetProcess.Handle, IntPtr.Zero, 0, startAddress, parameter, 0, out _);
		if (hThread == IntPtr.Zero)
			throw new Win32Exception(Marshal.GetLastWin32Error());

		try
		{
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

		// Clean up temp files
		if (this.remoteCtrlDllPath != null)
		{
			ScheduleForDeletion(this.remoteCtrlDllPath);
		}

		if (this.fasmDllPath != null)
		{
			ScheduleForDeletion(this.fasmDllPath);
		}

		this.isDisposed = true;
	}
#pragma warning restore IDE0060
}
