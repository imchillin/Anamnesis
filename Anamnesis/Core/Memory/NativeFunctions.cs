// © Anamnesis.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Anamnesis.Memory;

/// <summary>
/// A static class for Windows interop native functions.
/// </summary>
internal static partial class NativeFunctions
{
	public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	/// <summary>
	/// The keyboard input message types.
	/// </summary>
	public enum KeyboardInputMsg : int
	{
		WM_KEYDOWN = 0x0100,
		WM_KEYUP = 0x0101,
		WM_SYSKEYDOWN = 0x0104,
		WM_SYSKEYUP = 0x0105,
	}

	public enum HookType : int
	{
		/// <summary>
		/// Installs a hook procedure that records input messages posted to the system message
		/// queue. This hook is useful for recording macros. For more information, see the
		/// JournalRecordProc hook procedure.
		/// </summary>
		/// <remarks>
		/// Windows 11 and newer: Journaling hook APIs are not supported.
		/// We recommend using the SendInput TextInput API instead.
		/// </remarks>
		WH_JOURNALRECORD = 0,

		/// <summary>
		/// Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD
		/// hook procedure. For more information, see the JournalPlaybackProc hook procedure.
		/// </summary>
		/// <remarks>
		/// Windows 11 and newer: Journaling hook APIs are not supported.
		/// We recommend using the SendInput TextInput API instead.
		/// </remarks>
		WH_JOURNALPLAYBACK = 1,

		/// <summary>
		/// Installs a hook procedure that monitors keystroke messages.
		/// For more information, see the KeyboardProc hook procedure.
		/// </summary>
		WH_KEYBOARD = 2,

		/// <summary>
		/// Installs a hook procedure that monitors messages posted to a message queue.
		/// For more information, see the GetMsgProc hook procedure.
		/// </summary>
		WH_GETMESSAGE = 3,

		/// <summary>
		/// Installs a hook procedure that monitors messages before the system sends them to
		/// the destination window procedure. For more information, see the CallWndProc hook
		/// procedure.
		/// </summary>
		WH_CALLWNDPROC = 4,

		/// <summary>
		/// Installs a hook procedure that receives notifications useful to a CBT application.
		/// For more information, see the CBTProc hook procedure.
		/// </summary>
		WH_CBT = 5,

		/// <summary>
		/// Installs a hook procedure that monitors messages generated as a result of an input
		/// event in a dialog box, message box, menu, or scroll bar. The hook procedure monitors
		/// these messages for all applications in the same desktop as the calling thread.
		/// For more information, see the SysMsgProc hook procedure.
		/// </summary>
		WH_SYSMSGFILTER = 6,

		/// <summary>
		/// Installs a hook procedure that monitors mouse messages.
		/// For more information, see the MouseProc hook procedure.
		/// </summary>
		WH_MOUSE = 7,

		/// <summary>
		/// Undocumented.
		/// </summary>
		WH_HARDWARE = 8,

		/// <summary>
		/// Installs a hook procedure useful for debugging other hook procedures.
		/// For more information, see the DebugProc hook procedure.
		/// </summary>
		WH_DEBUG = 9,

		/// <summary>
		/// Installs a hook procedure that receives notifications useful to shell applications.
		/// For more information, see the ShellProc hook procedure.
		/// </summary>
		WH_SHELL = 10,

		/// <summary>
		/// Installs a hook procedure that will be called when the application's foreground thread
		/// is about to become idle. This hook is useful for performing low priority tasks during
		/// idle time. For more information, see the ForegroundIdleProc hook procedure.
		/// </summary>
		WH_FOREGROUNDIDLE = 11,

		/// <summary>
		/// Installs a hook procedure that monitors messages after they have been processed by the
		/// destination window procedure. For more information, see the HOOKPROC callback function
		/// hook procedure.
		/// </summary>
		WH_CALLWNDPROCRET = 12,

		/// <summary>
		/// Installs a hook procedure that monitors low-level keyboard input events.
		/// For more information, see the LowLevelKeyboardProc hook procedure.
		/// </summary>
		WH_KEYBOARD_LL = 13,

		/// <summary>
		/// Installs a hook procedure that monitors low-level mouse input events.
		/// For more information, see the LowLevelMouseProc hook procedure.
		/// </summary>
		WH_MOUSE_LL = 14,
	}

	/// <summary>
	/// Memory protection constants for use with <see cref="VirtualProtectEx"/>.
	/// </summary>
	/// <remarks>
	/// Ref: https://learn.microsoft.com/en-us/windows/win32/Memory/memory-protection-constants.
	/// </remarks>
	[Flags]
	public enum MemoryProtection : uint
	{
		/// <summary>
		/// Disables all access to the committed region of pages.
		/// An attempt to read from, write to, or execute the committed region results in an access violation.
		/// </summary>
		PAGE_NOACCESS = 0x01,

		/// <summary>
		/// Enables read-only access to the committed region of pages. An attempt to write to the committed region
		/// results in an access violation. If Data Execution Prevention is enabled, an attempt to execute code in
		/// the committed region results in an access violation.
		/// </summary>
		PAGE_READONLY = 0x02,

		/// <summary>
		/// Enables read-only or read/write access to the committed region of pages. If Data Execution Prevention
		/// is enabled, attempting to execute code in the committed region results in an access violation.
		/// </summary>
		PAGE_READWRITE = 0x04,

		/// <summary>
		/// Enables read-only or copy-on-write access to a mapped view of a file mapping object. An attempt to write
		/// to a committed copy-on-write page results in a private copy of the page being made for the process. The
		/// private page is marked as PAGE_READWRITE, and the change is written to the new page. If Data Execution
		/// Prevention is enabled, attempting to execute code in the committed region results in an access violation.
		/// </summary>
		PAGE_WRITECOPY = 0x08,

		/// <summary>
		/// Enables execute access to the committed region of pages. An attempt to write to the committed region
		/// results in an access violation.
		/// </summary>
		PAGE_EXECUTE = 0x10,

		/// <summary>
		/// Enables execute or read-only access to the committed region of pages. An attempt to write to the
		/// committed region results in an access violation.
		/// </summary>
		PAGE_EXECUTE_READ = 0x20,

		/// <summary>
		/// Enables execute, read-only, or read/write access to the committed region of pages.
		/// </summary>
		PAGE_EXECUTE_READWRITE = 0x40,

		/// <summary>
		/// Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. An attempt
		/// to write to a committed copy-on-write page results in a private copy of the page being made for the process.
		/// The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
		/// </summary>
		PAGE_EXECUTE_WRITECOPY = 0x80,

		/// <summary>
		/// Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a
		/// STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. Guard pages thus act as a one-time
		/// access alarm. For more information, see Creating Guard Pages. When an access attempt leads the system to turn
		/// off guard page status, the underlying page protection takes over. If a guard page exception occurs during a
		/// system service, the service typically returns a failure status indicator. This value cannot be used with
		/// PAGE_NOACCESS. This flag is not supported by the CreateFileMapping function.
		/// </summary>
		PAGE_GUARD = 0x100,

		/// <summary>
		/// Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required
		/// for a device. Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an
		/// EXCEPTION_ILLEGAL_INSTRUCTION exception. The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD,
		/// PAGE_NOACCESS, or PAGE_WRITECOMBINE flags. The PAGE_NOCACHE flag can be used only when allocating private
		/// memory with the VirtualAlloc, VirtualAllocEx, or VirtualAllocExNuma functions.To enable non-cached memory
		/// access for shared memory, specify the SEC_NOCACHE flag when calling the CreateFileMapping function.
		/// </summary>
		PAGE_NOCACHE = 0x200,

		/// <summary>
		/// Sets all pages to be write-combined. Applications should not use this attribute except when explicitly
		/// required for a device. Using the interlocked functions with memory that is mapped as write-combined can
		/// result in an EXCEPTION_ILLEGAL_INSTRUCTION exception. The PAGE_WRITECOMBINE flag cannot be specified with
		/// the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags. The PAGE_WRITECOMBINE flag can be used only when
		/// allocating private memory with the VirtualAlloc, VirtualAllocEx, or VirtualAllocExNuma functions. To enable
		/// write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when calling the
		/// CreateFileMapping function.
		/// </summary>
		PAGE_WRITECOMBINE = 0x400,

		/// <summary>
		/// Pages in the region will not have their CFG information updated while the protection changes for VirtualProtect.
		/// For example, if the pages in the region was allocated using PAGE_TARGETS_INVALID, then the invalid information
		/// will be maintained while the page protection changes. This flag is only valid when the protection changes to an
		/// executable type like PAGE_EXECUTE, PAGE_EXECUTE_READ, PAGE_EXECUTE_READWRITE and PAGE_EXECUTE_WRITECOPY.
		/// The default behavior for VirtualProtect protection change to executable is to mark all locations as valid call
		/// targets for CFG.
		/// </summary>
		PAGE_TARGETS_NO_UPDATE = 0x40000000,
	}

	/// <summary>
	/// Status codes returned by Windows NT API functions.
	/// </summary>
	/// <remarks>
	/// Ref: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55.
	/// <para>
	/// The full list is enormous. Add additional status codes as needed.
	/// </para>
	/// </remarks>
	public enum NtStatus : uint
	{
		STATUS_SUCCESS = 0x00000000,
	}

	/// <summary>
	/// Reads from virtual memory in the specified process.
	/// </summary>
	/// <param name="pHandle">
	/// A handle to the process whose memory is being read.
	/// </param>
	/// <param name="baseAddress">
	/// A pointer to the base address in the specified process from which to read.
	/// </param>
	/// <param name="buffer">
	/// A pointer to a buffer that receives the contents from the address space of the specified process.
	/// </param>
	/// <param name="bufferSize">
	/// The number of bytes to be read from the specified process.
	/// </param>
	/// <param name="numberOfBytesRead">
	/// A pointer to a variable that receives the number of bytes transferred into the buffer.
	/// </param>
	/// <returns>
	/// <see cref="NtStatus.STATUS_SUCCESS"/> indicates success. See <see cref="NtStatus"/> for other status codes.
	/// </returns>
	[LibraryImport("ntdll.dll")]
	public static partial uint NtReadVirtualMemory(nint pHandle, nint baseAddress, nint buffer, int bufferSize, out nint numberOfBytesRead);

	/// <summary>
	/// Writes to virtual memory in the specified process.
	/// </summary>
	/// <param name="pHandle">
	/// A handle to the process whose memory is to be modified.
	/// </param>
	/// <param name="baseAddress">
	/// A pointer to the base address in the specified process to which data is written.
	/// </param>
	/// <param name="buffer">
	/// A pointer to the buffer that contaisn the data to be written in the address space of the specified process.
	/// </param>
	/// <param name="bufferSize">
	/// The number of bytes to be written to the specified process.
	/// </param>
	/// <param name="numberOfBytesWritten">
	/// A pointer to a variable that receives the number of bytes transferred into the specified process.
	/// </param>
	/// <returns>
	/// <see cref="NtStatus.STATUS_SUCCESS"/> indicates success. See <see cref="NtStatus"/> for other status codes.
	/// </returns>
	[LibraryImport("ntdll.dll")]
	public static partial uint NtWriteVirtualMemory(nint pHandle, nint baseAddress, nint buffer, int bufferSize, out nint numberOfBytesWritten);

	/// <summary>
	/// Opens an existing local process object.
	/// </summary>
	/// <param name="dwDesiredAccess">
	/// The access to the process object. This access right is checked against the security descriptor for the process.
	/// This parameter can be one or more of the process access rights. If the caller has enabled the SeDebugPrivilege
	/// privilege, the requested access is granted regardless of the contents of the security descriptor.
	/// </param>
	/// <param name="bInheritHandle">
	/// If this value is TRUE, processes created by this process will inherit the handle. Otherwise, the processes do
	/// not inherit this handle.
	/// </param>
	/// <param name="processId">
	/// The identifier of the local process to be opened. If the specified process is the System Idle Process(0x00000000),
	/// the function fails and the last error code is ERROR_INVALID_PARAMETER.If the specified process is the System
	/// process or one of the Client Server Run-Time Subsystem (CSRSS) processes, this function fails and the last error code
	/// is ERROR_ACCESS_DENIED because their access restrictions prevent user-level code from opening them.
	/// If you are using GetCurrentProcessId as an argument to this function, consider using GetCurrentProcess instead of
	/// OpenProcess, for improved performance.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is an open handle to the specified process.
	/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	public static partial nint OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int processId);

	/// <summary>
	/// Changes the protection on a region of committed pages in the virtual address space of a specified process.
	/// </summary>
	/// <param name="hProcess">
	/// A handle to the process whose memory protection is to be changed. The handle must have the PROCESS_VM_OPERATION
	/// access right. For more information, see Process Security and Access Rights.
	/// </param>
	/// <param name="lpAddress">
	/// A pointer to the base address of the region of pages whose access protection attributes are to be changed.
	/// All pages in the specified region must be within the same reserved region allocated when calling the VirtualAlloc
	/// or VirtualAllocEx function using MEM_RESERVE. The pages cannot span adjacent reserved regions that were allocated
	/// by separate calls to VirtualAlloc or VirtualAllocEx using MEM_RESERVE.
	/// </param>
	/// <param name="dwSize">
	/// The size of the region whose access protection attributes are changed, in bytes. The region of affected pages
	/// includes all pages containing one or more bytes in the range from the lpAddress parameter to (lpAddress+dwSize).
	/// This means that a 2-byte range straddling a page boundary causes the protection attributes of both pages to be changed.
	/// </param>
	/// <param name="flNewProtect">
	/// The memory protection option. This parameter can be one of the memory protection constants.
	/// For mapped views, this value must be compatible with the access protection specified when the view was mapped
	/// (see MapViewOfFile, MapViewOfFileEx, and MapViewOfFileExNuma).
	/// </param>
	/// <param name="lpflOldProtect">
	/// A pointer to a variable that receives the previous access protection of the first page in the specified region of pages.
	/// If this parameter is NULL or does not point to a valid variable, the function fails.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool VirtualProtectEx(nint hProcess, nint lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

	/// <summary>
	/// Retrieves a handle to the foreground window (the window with which the user is currently working).
	/// The system assigns a slightly higher priority to the thread that creates the foreground window than
	/// it does to other threads.
	/// </summary>
	/// <returns>
	/// The return value is a handle to the foreground window. The foreground window can be NULL in certain circumstances,
	/// such as when a window is losing activation.
	/// </returns>
	[LibraryImport("user32.dll")]
	public static partial nint GetForegroundWindow();

	/// <summary>
	/// Places (posts) a message in the message queue associated with the thread that created the specified window and
	/// returns without waiting for the thread to process the message.
	/// To post a message in the message queue associated with a thread, use the PostThreadMessage function.
	/// </summary>
	/// <param name="hWnd">
	/// A handle to the window whose window procedure is to receive the message.
	/// </param>
	/// <param name="msg">
	/// The message to be posted. For lists of the system-provided messages, see System-Defined Messages.
	/// </param>
	/// <param name="wParam">
	/// Additional message-specific information.
	/// </param>
	/// <param name="lParam">
	/// Additional message-specific information.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

	/// <summary>
	/// Installs an application-defined hook procedure into a hook chain.
	/// You would install a hook procedure to monitor the system for certain types of events.
	/// These events are associated either with a specific thread or with all threads in the same desktop as
	/// the calling thread.
	/// </summary>
	/// <param name="idHook">
	/// The type of hook procedure to be installed.
	/// </param>
	/// <param name="lpfn">
	/// A pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a
	/// thread created by a different process, the lpfn parameter must point to a hook procedure in a DLL.
	/// Otherwise, lpfn can point to a hook procedure in the code associated with the current process.
	/// </param>
	/// <param name="hMod">
	/// A handle to the DLL containing the hook procedure pointed to by the lpfn parameter. The hMod parameter
	/// must be set to NULL if the dwThreadId parameter specifies a thread created by the current process and
	/// if the hook procedure is within the code associated with the current process.
	/// </param>
	/// <param name="dwThreadId">
	/// The identifier of the thread with which the hook procedure is to be associated. For desktop apps,
	/// if this parameter is zero, the hook procedure is associated with all existing threads running in the
	/// same desktop as the calling thread. For Windows Store apps, see the Remarks section.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is the handle to the hook procedure.
	/// If the function fails, the return value is NULL.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowsHookExW")]
	public static partial IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	/// <summary>
	/// Removes a hook procedure installed in a hook chain by the <see cref="SetWindowsHookEx"/> function.
	/// </summary>
	/// <param name="hhk">
	/// A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
	/// <see cref="SetWindowsHookEx"/>
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool UnhookWindowsHookEx(IntPtr hhk);

	/// <summary>
	/// Passes the hook information to the next hook procedure in the current hook chain.
	/// A hook procedure can call this function either before or after processing the hook information.
	/// </summary>
	/// <param name="hhk">
	/// This parameter is ignored.
	/// </param>
	/// <param name="nCode">
	/// The hook code passed to the current hook procedure.
	/// The next hook procedure uses this code to determine how to process the hook information.
	/// </param>
	/// <param name="wParam">
	/// The wParam value passed to the current hook procedure.
	/// The meaning of this parameter depends on the type of hook associated with the current hook chain.
	/// </param>
	/// <param name="lParam">
	/// The lParam value passed to the current hook procedure.
	/// The meaning of this parameter depends on the type of hook associated with the current hook chain.
	/// </param>
	/// <returns>
	/// This value is returned by the next hook procedure in the chain. The current hook procedure must
	/// also return this value. The meaning of the return value depends on the hook type.
	/// For more information, see the descriptions of the individual hook procedures.
	/// </returns>
	[LibraryImport("user32.dll", SetLastError = true)]
	public static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
}
