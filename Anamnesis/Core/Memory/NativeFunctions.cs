// © Anamnesis.
// Licensed under the MIT license.

using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Anamnesis.Memory;

#pragma warning disable SA1121 // Use built-in type alias
/// <summary>
/// A static class for Windows interop native functions.
/// </summary>
internal static partial class NativeFunctions
{
	public const IntPtr INVALID_HANDLE_VALUE = -1;
	public const UInt32 GRANT_ACCESS = 1;

	public const UInt32 STANDARD_RIGHTS_ALL = 0x001F0000;
	public const UInt32 SPECIFIC_RIGHTS_ALL = 0x0000FFFF;
	public const UInt32 PROCESS_VM_WRITE = 0x0020;

	public const int PROCESS_DYNAMIC_CODE_POLICY = 2;

	public const string SE_DEBUG_NAME = "SeDebugPrivilege";
	public const uint SE_PRIVILEGE_ENABLED = 0x2;
	public const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
	public const int TOKEN_QUERY = 0x0008;
	public const int ERROR_NO_TOKEN = 0x3F0;
	public const int ERROR_NOT_ALL_ASSIGNED = 0x514;
	public const string ENV_COMPAT_LAYER_VARNAME = "__COMPAT_LAYER";
	public const string ENV_COMPAT_RUN_AS_ENVOKER = "RunAsInvoker";

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

	public enum WinHookType : int
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
	public enum MemoryProtectionType : uint
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
		/// access alarm.
		///
		/// When an access attempt leads the system to turn off guard page status, the underlying page protection takes
		/// over. If a guard page exception occurs during a system service, the service typically returns a failure status
		/// indicator. This value cannot be used with PAGE_NOACCESS. This flag is not supported by the CreateFileMapping
		/// function.
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
	/// Memory allocation types for use with <see cref="VirtualAllocEx"/>.
	/// </summary>
	/// <remarks>
	/// Ref: https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualallocex.
	/// </remarks>
	[Flags]
	public enum MemoryAllocationType : uint
	{
		/// <summary>
		/// Allocates memory charges (from the overall size of memory and the paging files on disk) for the specified
		/// reserved memory pages. The function also guarantees that when the caller later initially accesses the memory,
		/// the contents will be zero. Actual physical pages are not allocated unless/until the virtual addresses are
		/// actually accessed. To reserve and commit pages in one step, call <see cref="VirtualAllocEx"/> with
		/// MEM_COMMIT | MEM_RESERVE. Attempting to commit a specific address range by specifying MEM_COMMIT without
		/// MEM_RESERVE and a non-NULL lpAddress fails unless the entire range has already been reserved. The resulting
		/// error code is ERROR_INVALID_ADDRESS. An attempt to commit a page that is already committed does not cause the
		/// function to fail. This means that you can commit pages without first determining the current commitment
		/// state of each page. If lpAddress specifies an address within an enclave, flAllocationType must be MEM_COMMIT.
		/// </summary>
		MEM_COMMIT = 0x00001000,

		/// <summary>
		/// Reserves a range of the process's virtual address space without allocating any actual physical storage in
		/// memory or in the paging file on disk. You commit reserved pages by calling <see cref="VirtualAllocEx"/>
		/// again with MEM_COMMIT. To reserve and commit pages in one step, call <see cref="VirtualAllocEx"/> with
		/// MEM_COMMIT | MEM_RESERVE. Other memory allocation functions, such as malloc and LocalAlloc, cannot use
		/// reserved memory until it has been released.
		/// </summary>
		MEM_RESERVE = 0x00002000,

		/// <summary>
		/// Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages should
		/// not be read from or written to the paging file. However, the memory block will be used again later, so it should not
		/// be decommitted. This value cannot be used with any other value. Using this value does not guarantee that the range
		/// operated on with MEM_RESET will contain zeros. If you want the range to contain zeros, decommit the memory and then
		/// recommit it. When you use MEM_RESET, the <see cref="VirtualAllocEx"/> function ignores the value of fProtect.
		/// However, you must still set fProtect to a valid protection value, such as PAGE_NOACCESS. <see cref="VirtualAllocEx"/>
		/// returns an error if you use MEM_RESET and the range of memory is mapped to a file. A shared view is only acceptable
		/// if it is mapped to a paging file.
		/// </summary>
		MEM_RESET = 0x00080000,

		/// <summary>
		/// MEM_RESET_UNDO should only be called on an address range to which MEM_RESET was successfully applied earlier.
		/// It indicates that the data in the specified memory range specified by lpAddress and dwSize is of interest to the
		/// caller and attempts to reverse the effects of MEM_RESET. If the function succeeds, that means all data in the
		/// specified address range is intact. If the function fails, at least some of the data in the address range has
		/// been replaced with zeroes. This value cannot be used with any other value. If MEM_RESET_UNDO is called on an
		/// address range which was not MEM_RESET earlier, the behavior is undefined. When you specify MEM_RESET, the
		/// <see cref="VirtualAllocEx"/> function ignores the value of flProtect. However, you must still set flProtect
		/// to a valid protection value, such as PAGE_NOACCESS.
		/// </summary>
		MEM_RESET_UNDO = 0x1000000,

		/// <summary>
		/// Allocates memory using large page support. The size and alignment must be a multiple of the large-page minimum.
		/// To obtain this value, use the GetLargePageMinimum function. If you specify this value, you must also specify
		/// MEM_RESERVE and MEM_COMMIT.
		/// </summary>
		MEM_LARGE_PAGES = 0x20000000,

		/// <summary>
		/// Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages. This value must
		/// be used with MEM_RESERVE and no other values.
		/// </summary>
		MEM_PHYSICAL = 0x00400000,

		/// <summary>
		/// Allocates memory at the highest possible address. This can be slower than regular allocations, especially when
		/// there are many allocations.
		/// </summary>
		MEM_TOP_DOWN = 0x00100000,
	}

	/// <summary>
	/// Flags for use with the <see cref="VirtualFreeEx"/> Windows API function.
	/// </summary>
	[Flags]
	public enum MemoryFreeType : uint
	{
		/// <summary>
		/// To coalesce two adjacent placeholders, specify MEM_RELEASE | MEM_COALESCE_PLACEHOLDERS. When you coalesce
		/// placeholders, lpAddress and dwSize must exactly match the overall range of the placeholders to be merged.
		/// </summary>
		MEM_COALESCE_PLACEHOLDERS = 0x00000001,

		/// <summary>
		/// Frees an allocation back to a placeholder (after you've replaced a placeholder with a private allocation using
		/// VirtualAlloc2 or Virtual2AllocFromApp).
		///
		/// To split a placeholder into two placeholders, specify MEM_RELEASE | MEM_PRESERVE_PLACEHOLDER.
		/// </summary>
		MEM_PRESERVE_PLACEHOLDER = 0x00000002,

		/// <summary>
		/// Decommits the specified region of committed pages. After the operation, the pages are in the reserved state.
		/// The function does not fail if you attempt to decommit an uncommitted page. This means that you can decommit
		/// a range of pages without first determining the current commitment state.
		///
		/// The MEM_DECOMMIT value is not supported when the lpAddress parameter provides the base address for an enclave.
		/// This is true for enclaves that do not support dynamic memory management (i.e.SGX1). SGX2 enclaves permit
		/// MEM_DECOMMIT anywhere in the enclave.
		/// </summary>
		MEM_DECOMMIT = 0x00004000,

		/// <summary>
		/// Releases the specified region of pages, or placeholder (for a placeholder, the address space is released and
		/// available for other allocations). After this operation, the pages are in the free state. If you specify this
		/// value, dwSize must be 0 (zero), and lpAddress must point to the base address returned by the VirtualAlloc
		/// function when the region is reserved.The function fails if either of these conditions is not met.
		///
		/// If any pages in the region are committed currently, the function first decommits, and then releases them.
		///
		/// The function does not fail if you attempt to release pages that are in different states, some reserved and
		/// some committed.This means that you can release a range of pages without first determining the current
		/// commitment state.
		/// </summary>
		MEM_RELEASE = 0x00008000,
	}

	/// <summary>
	/// Flags for use with the <see cref="MoveFileEx"/> Windows API function.
	/// </summary>
	[Flags]
	public enum MoveFileFlag : uint
	{
		/// <summary>
		/// If a file named lpNewFileName exists, the function replaces its contents with the contents of the
		/// lpExistingFileName file, provided that security requirements regarding access control lists (ACLs) are
		/// met.
		///
		/// If lpNewFileName names an existing directory, an error is reported.
		/// </summary>
		MOVEFILE_REPLACE_EXISTING = 0x00000001,

		/// <summary>
		/// If the file is to be moved to a different volume, the function simulates the move by using the CopyFile
		/// and DeleteFile functions. If the file is successfully copied to a different volume and the original file
		/// is unable to be deleted, the function succeeds leaving the source file intact.
		///
		/// This value cannot be used with MOVEFILE_DELAY_UNTIL_REBOOT.
		/// </summary>
		MOVEFILE_COPY_ALLOWED = 0x00000002,

		/// <summary>
		/// The system does not move the file until the operating system is restarted. The system moves the file
		/// immediately after AUTOCHK is executed, but before creating any paging files. Consequently, this parameter
		/// enables the function to delete paging files from previous startups. This value can be used only if the
		/// process is in the context of a user who belongs to the administrators group or the LocalSystem account.
		///
		/// This value cannot be used with MOVEFILE_COPY_ALLOWED.
		/// </summary>
		MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,

		/// <summary>
		/// The function does not return until the file is actually moved on the disk. Setting this value guarantees
		/// that a move performed as a copy and delete operation is flushed to disk before the function returns.
		/// The flush occurs at the end of the copy operation.
		///
		/// This value has no effect if MOVEFILE_DELAY_UNTIL_REBOOT is set.
		/// </summary>
		MOVEFILE_WRITE_THROUGH = 0x00000008,

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		MOVEFILE_CREATE_HARDLINK = 0x00000010,

		/// <summary>
		/// The function fails if the source file is a link source, but the file cannot be tracked after the move.
		/// This situation can occur if the destination is a volume formatted with the FAT file system.
		/// </summary>
		MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020,
	}

	/// <summary>
	/// Contains values that specify security impersonation levels. Security impersonation levels govern the
	/// degree to which a server process can act on behalf of a client process.
	/// </summary>
	public enum SecurityImpersonationLevel : int
	{
		/// <summary>
		/// The server process cannot obtain identification information about the client, and it cannot
		/// impersonate the client. It is defined with no value given, and thus, by ANSI C rules, defaults
		/// to a value of zero.
		/// </summary>
		SecurityAnonymous,

		/// <summary>
		/// The server process can obtain information about the client, such as security identifiers and privileges,
		/// but it cannot impersonate the client. This is useful for servers that export their own objects, for
		/// example, database products that export tables and views. Using the retrieved client-security information,
		/// the server can make access-validation decisions without being able to use other services that are using
		/// the client's security context.
		/// </summary>
		SecurityIdentification,

		/// <summary>
		/// The server process can impersonate the client's security context on its local system. The server cannot
		/// impersonate the client on remote systems.
		/// </summary>
		SecurityImpersonation,

		/// <summary>
		/// The server process can impersonate the client's security context on remote systems.
		/// </summary>
		SecurityDelegation,
	}

	/// <summary>
	/// An enumeration that contains the types of Windows objects that support security.
	/// </summary>
	public enum SeObjectType : int
	{
		/// <summary>
		/// Unknown object type.
		/// </summary>
		SE_UNKNOWN_OBJECT_TYPE,

		/// <summary>
		/// Indicates a file or directory.
		/// </summary>
		SE_FILE_OBJECT,

		/// <summary>
		/// Indicates a Windows service. A service object can be a local service,
		/// such as ServiceName, or a remote service, such as \\ComputerName\ServiceName.
		/// </summary>
		SE_SERVICE,

		/// <summary>
		/// Indicates a printer. A printer object can be a local printer, such as
		/// PrinterName, or a remote printer, such as \\ComputerName\PrinterName.
		/// </summary>
		SE_PRINTER,

		/// <summary>
		/// ndicates a registry key. A registry key object can be in the local
		/// registry, such as CLASSES_ROOT\SomePath or in a remote registry,
		/// such as \\ComputerName\CLASSES_ROOT\SomePath.
		/// </summary>
		SE_REGISTRY_KEY,

		/// <summary>
		/// Indicates a network share. A share object can be local, such as
		/// ShareName, or remote, such as \\ComputerName\ShareName.
		/// </summary>
		SE_LMSHARE,

		/// <summary>
		/// Indicates a local kernel object.
		/// </summary>
		SE_KERNEL_OBJECT,

		/// <summary>
		/// Indicates a window station or desktop object on the local computer.
		/// </summary>
		SE_WINDOW_OBJECT,

		/// <summary>
		/// Indicates a directory service object or a property set or property
		/// of a directory service object.
		/// </summary>
		SE_DS_OBJECT,

		/// <summary>
		/// Indicates a directory service object and all of its property sets
		/// and properties.
		/// </summary>
		SE_DS_OBJECT_ALL,

		/// <summary>
		/// Indicates a provider-defined object.
		/// </summary>
		SE_PROVIDER_DEFINED_OBJECT,

		/// <summary>
		/// Indicates a WMI object.
		/// </summary>
		SE_WMIGUID_OBJECT,

		/// <summary>
		/// Indicates an object for a registry entry under WOW64.
		/// </summary>
		SE_REGISTRY_WOW64_32KEY,
	}

	/// <summary>
	/// An enumeration that defines object-related security information
	/// being set or queried.
	/// </summary>
	[Flags]
	public enum SecurityInformation : uint
	{
		/// <summary>
		/// The owner identifier of the object is being referenced.
		/// </summary>
		OWNER_SECURITY_INFORMATION = 0x00000001,

		/// <summary>
		/// The primary group identifier of the object is being referenced.
		/// </summary>
		GROUP_SECURITY_INFORMATION = 0x00000002,

		/// <summary>
		/// The DACL of the object is being referenced.
		/// </summary>
		DACL_SECURITY_INFORMATION = 0x00000004,

		/// <summary>
		/// The SACL of the object is being referenced.
		/// </summary>
		SACL_SECURITY_INFORMATION = 0x00000008,

		/// <summary>
		/// The mandatory integrity label is being referenced.
		/// </summary>
		LABEL_SECURITY_INFORMATION = 0x00000010,

		/// <summary>
		/// The SACL inherits access control entries (ACEs) from the parent
		/// object.
		/// </summary>
		UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,

		/// <summary>
		/// The DACL inherits ACEs from the parent object.
		/// </summary>
		UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,

		/// <summary>
		/// The SACL cannot inherit ACEs.
		/// </summary>
		PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,

		/// <summary>
		/// The DACL cannot inherit ACEs.
		/// </summary>
		PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,

		/// <summary>
		/// A SYSTEM_RESOURCE_ATTRIBUTE_ACE is being referenced.
		/// </summary>
		ATTRIBUTE_SECURITY_INFORMATION = 0x00000020,

		/// <summary>
		/// A SYSTEM_SCOPED_POLICY_ID_ACE is being referenced.
		/// </summary>
		SCOPE_SECURITY_INFORMATION = 0x00000040,

		/// <summary>
		/// Reserved.
		/// </summary>
		PROCESS_TRUST_LABEL_SECURITY_INFORMATION = 0x00000080,

		/// <summary>
		/// The security descriptor is being accessed for use in a backup
		/// operation.
		/// </summary>
		BACKUP_SECURITY_INFORMATION = 0x00010000,
	}

	/// <summary>
	/// An enum that declares values that indicate whether a TRUSTEE
	/// structure is an impersonation trustee.
	/// </summary>
	public enum MultipleTrusteeOperation : uint
	{
		/// <summary>
		/// The trustee is not an impersonation trustee.
		/// </summary>
		NO_MULTIPLE_TRUSTEE,

		/// <summary>
		/// The trustee is an impersonation trustee. The pMultipleTrustee
		/// member of the TRUSTEE structure points to a trustee for a
		/// server that can impersonate the client trustee.
		/// </summary>
		TRUSTEE_IS_IMPERSONATE,
	}

	/// <summary>
	/// An enum that declares values that indicate the type of data
	/// pointed to by the ptstrName member of the TRUSTEE structure.
	/// </summary>
	public enum TrusteeForm : uint
	{
		/// <summary>
		/// The ptstrName member is a pointer to a security identifier
		/// (SID) that identifies the trustee.
		/// </summary>
		TRUSTEE_IS_SID,

		/// <summary>
		/// The ptstrName member is a pointer to a null-terminated
		/// string that identifies the trustee.
		/// </summary>
		TRUSTEE_IS_NAME,

		/// <summary>
		/// Indicates a trustee form that is not valid.
		/// </summary>
		TRUSTEE_BAD_FORM,

		/// <summary>
		/// The ptstrName member is a pointer to an OBJECTS_AND_SID structure
		/// that contains the SID of the trustee and the GUIDs of the object
		/// types in an object-specific access control entry(ACE).
		/// </summary>
		TRUSTEE_IS_OBJECTS_AND_SID,

		/// <summary>
		/// The ptstrName member is a pointer to an OBJECTS_AND_NAME structure
		/// that contains the name of the trustee and the names of the object
		/// types in an object-specific ACE.
		/// </summary>
		TRUSTEE_IS_OBJECTS_AND_NAME,
	}

	/// <summary>
	/// An enum that declares values that indicate the type of trustee
	/// identified by a TRUSTEE structure.
	/// </summary>
	public enum TrusteeType : uint
	{
		/// <summary>
		/// The trustee type is unknown, but it may be valid.
		/// </summary>
		TRUSTEE_IS_UNKNOWN,

		/// <summary>
		/// Indicates a user.
		/// </summary>
		TRUSTEE_IS_USER,

		/// <summary>
		/// Indicates a group.
		/// </summary>
		TRUSTEE_IS_GROUP,

		/// <summary>
		/// Indicates a domain.
		/// </summary>
		TRUSTEE_IS_DOMAIN,

		/// <summary>
		/// Indicates an alias.
		/// </summary>
		TRUSTEE_IS_ALIAS,

		/// <summary>
		/// Indicates a well-known group.
		/// </summary>
		TRUSTEE_IS_WELL_KNOWN_GROUP,

		/// <summary>
		/// Indicates a deleted account.
		/// </summary>
		TRUSTEE_IS_DELETED,

		/// <summary>
		/// Indicates a trustee type that is not valid.
		/// </summary>
		TRUSTEE_IS_INVALID,

		/// <summary>
		/// Indicates a computer.
		/// </summary>
		TRUSTEE_IS_COMPUTER,
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
	/// Applies a custom access control list (ACL) to the target process.
	/// This is necessary to allow hooks to allocate memory for their trampolines.
	/// </summary>
	/// <param name="hProcess">The target process handle.</param>
	/// <remarks>
	/// Dalamud's implementation was used as a loose reference:
	/// https://github.com/goatcorp/Dalamud/blob/master/Dalamud.Injector/GameStart.cs.
	/// </remarks>
	public static void ApplyCustomAclToProcess(IntPtr hProcess)
	{
		var userName = Environment.UserName;
		IntPtr pNewAcl = IntPtr.Zero;

		try
		{
			var explicitAccess = new ExplicitAccess
			{
				GrfAccessPermissions = STANDARD_RIGHTS_ALL | SPECIFIC_RIGHTS_ALL,
				GrfAccessMode = GRANT_ACCESS,
				GrfInheritance = 0,
				Trustee = new Trustee
				{
					PMultipleTrustee = IntPtr.Zero,
					MultipleTrusteeOperation = 0,
					TrusteeForm = TrusteeForm.TRUSTEE_IS_NAME,
					TrusteeType = TrusteeType.TRUSTEE_IS_USER,
					PtstrName = Marshal.StringToHGlobalAuto(userName),
				},
			};

			uint result = SetEntriesInAcl(1, [explicitAccess], IntPtr.Zero, out pNewAcl);
			if (result != 0)
				throw new Win32Exception((int)result, "Failed to build new ACL.");

			result = SetSecurityInfo(
				hProcess,
				(uint)SeObjectType.SE_KERNEL_OBJECT,
				(uint)(SecurityInformation.DACL_SECURITY_INFORMATION | SecurityInformation.UNPROTECTED_DACL_SECURITY_INFORMATION),
				IntPtr.Zero,
				IntPtr.Zero,
				pNewAcl,
				IntPtr.Zero);

			if (result != 0)
				throw new Win32Exception((int)result, "Failed to apply security info to process.");

			Log.Debug("ACL successfully applied to target process.");
		}
		finally
		{
			// SetEntriesInAcl allocates memory that must be freed with LocalFree
			if (pNewAcl != IntPtr.Zero)
				LocalFree(pNewAcl);
		}
	}

	/// <summary>
	/// Enables the SeDebugPrivilege for the current thread.
	/// processes.
	/// </summary>
	/// <remarks>
	/// Dalamud's implementation was used as a reference:
	/// https://github.com/goatcorp/Dalamud/blob/master/Dalamud.Injector/GameStart.cs.
	/// </remarks>
	public static void ClaimSeDebugPrivilege()
	{
		IntPtr hToken = INVALID_HANDLE_VALUE;
		try
		{
			// Try to open the process thread token. If not present, impersonate and retry
			if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken))
			{
				int error = Marshal.GetLastWin32Error();
				if (error != ERROR_NO_TOKEN)
					throw new Win32Exception(error, "Failed to open thread token.");

				if (!ImpersonateSelf((int)SecurityImpersonationLevel.SecurityImpersonation))
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to impersonate self.");

				if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken))
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open thread token after impersonation.");
			}

			// Lookup the LUID for SeDebugPrivilege
			if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, out Luid luid))
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to lookup SeDebugPrivilege LUID.");

			var tp = new TokenPrivileges
			{
				PrivilegeCount = 1,
				Privileges = new LuidAndAttributes(luid, SE_PRIVILEGE_ENABLED),
			};

			// Adjust the token privileges
			if (!AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to adjust token privileges.");

			if (Marshal.GetLastWin32Error() == ERROR_NOT_ALL_ASSIGNED)
				Log.Debug("SeDebugPrivilege could not be assigned. It may already be enabled or not available.");
			else
				Log.Debug("SeDebugPrivilege claimed successfully.");
		}
		finally
		{
			if (hToken != INVALID_HANDLE_VALUE && hToken != IntPtr.Zero)
			{
				CloseHandle(hToken);
			}
		}
	}

	/// <summary>
	/// Sets the compatibility layer environment variable to "RunAsInvoker".
	/// </summary>
	public static void SetCompatibilityLayer()
	{
		try
		{
			var currentLayer = Environment.GetEnvironmentVariable(ENV_COMPAT_LAYER_VARNAME);

			if (string.IsNullOrEmpty(currentLayer))
			{
				Environment.SetEnvironmentVariable(
					ENV_COMPAT_LAYER_VARNAME,
					ENV_COMPAT_RUN_AS_ENVOKER);
			}
			else if (!currentLayer.Contains(
				ENV_COMPAT_RUN_AS_ENVOKER,
				StringComparison.OrdinalIgnoreCase))
			{
				Environment.SetEnvironmentVariable(
					ENV_COMPAT_LAYER_VARNAME,
					$"{ENV_COMPAT_RUN_AS_ENVOKER} {currentLayer}");
			}
		}
		catch (Exception ex)
		{
			Log.Warning(ex, $"Failed to set {ENV_COMPAT_LAYER_VARNAME} environment variable.");
		}
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
	/// access right.
	/// </param>
	/// <param name="lpAddress">
	/// A pointer to the base address of the region of pages whose access protection attributes are to be changed.
	/// All pages in the specified region must be within the same reserved region allocated when calling the VirtualAlloc
	/// or <see cref="VirtualAllocEx"/> function using MEM_RESERVE. The pages cannot span adjacent reserved regions that
	/// were allocated by separate calls to VirtualAlloc or <see cref="VirtualAllocEx"/> using MEM_RESERVE.
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
	/// Reserves, commits, or changes the state of a region of memory within the virtual address space of a specified
	/// process.
	/// </summary>
	/// <remarks>
	/// This method is a P/Invoke signature for the Windows API function <see cref="VirtualAllocEx"/>. Use
	/// Marshal.GetLastWin32Error to obtain extended error information if the function fails. The allocated memory must be
	/// released with VirtualFreeEx. This method is intended for advanced scenarios such as inter-process memory
	/// management.
	/// </remarks>
	/// <param name="hProcess">
	/// A handle to the process in which to allocate memory. The handle must have the PROCESS_VM_OPERATION access right.
	/// </param>
	/// <param name="lpAddress">
	/// The desired starting address of the region to allocate. If IntPtr.Zero, the system determines the address.</param>
	/// <param name="dwSize">
	/// The size of the region of memory to allocate, in bytes. The value is rounded up to the nearest multiple of the
	/// system's page size.
	/// </param>
	/// <param name="flAllocationType">
	/// The type of memory allocation. This parameter can be a combination of allocation type flags such as MEM_COMMIT or
	/// MEM_RESERVE.
	/// </param>
	/// <param name="flProtect">
	/// The memory protection for the region of pages to be allocated. This parameter specifies the desired memory
	/// protection constants, such as PAGE_READWRITE.
	/// </param>
	/// <returns>If the function succeeds, returns a pointer to the base address of the allocated region of pages. If the function
	/// fails, returns IntPtr.Zero.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	public static partial IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

	/// <summary>
	/// Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified
	/// process.
	/// </summary>
	/// <remarks>If the function fails, call GetLastError to obtain extended error information. This function is
	/// typically used in advanced memory management scenarios and should be used with care, as improper use can lead to
	/// process instability.
	/// </remarks>
	/// <param name="hProcess">
	/// A handle to the process whose memory is to be freed. The handle must have the PROCESS_VM_OPERATION access right.
	/// </param>
	/// <param name="lpAddress">
	/// A pointer to the base address of the region of memory to be freed. This value must be the base address returned by
	/// a previous call to the memory allocation function.
	/// </param>
	/// <param name="dwSize">
	/// The size of the region of memory to free, in bytes. If dwFreeType includes MEM_RELEASE, this parameter must be 0.
	/// Otherwise, it specifies the size of the region to decommit.
	/// </param>
	/// <param name="dwFreeType">
	/// The type of free operation. This parameter must be either MEM_DECOMMIT or MEM_RELEASE, and can optionally include
	/// MEM_COALESCE_PLACEHOLDERS or MEM_PRESERVE_PLACEHOLDER.</param>
	/// <returns>true if the operation succeeds; otherwise, false.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

	/// <summary>
	/// Retrieves a handle to the specified loaded module in the calling process.
	/// </summary>
	/// <remarks>
	/// The returned handle is valid only in the context of the calling process. The handle does not need
	/// to be closed. If the function fails, call <see cref="Marshal.GetLastWin32Error"/> to obtain the error code.
	/// </remarks>
	/// <param name="lpModuleName">
	/// The name of the loaded module (DLL or EXE) to retrieve a handle for. This parameter can be null to obtain a handle
	/// to the file used to create the calling process.
	/// </param>
	/// <returns>
	/// An <see cref="IntPtr"/> value representing a handle to the specified module if the function succeeds; otherwise,
	/// <see cref="IntPtr.Zero"/> if the module is not found.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true, EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
	public static partial IntPtr GetModuleHandle(string lpModuleName);

	/// <summary>
	/// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
	/// </summary>
	/// <remarks>
	/// This method is typically used for advanced scenarios such as invoking unmanaged functions not
	/// directly exposed through platform invoke. If the function or variable is not found, use Marshal.GetLastWin32Error
	/// to obtain the error code. The returned address is valid only as long as the module remains loaded.
	/// </remarks>
	/// <param name="module">
	/// A handle to the DLL module that contains the function or variable. This handle must have been obtained by a
	/// previous call to LoadLibrary or a similar method and must not be IntPtr.Zero.</param>
	/// <param name="procName">
	/// The name of the function or variable to retrieve. This can be either a null-terminated string specifying the name
	/// or a string representing the ordinal value prefixed with a '#' character. Cannot be null.</param>
	/// <returns>
	/// An IntPtr representing the address of the exported function or variable if found; otherwise, IntPtr.Zero.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
	public static partial IntPtr GetProcAddress(IntPtr module, string procName);

	/// <summary>
	/// Waits until the specified object is in the signaled state or the time-out interval elapses.
	///
	/// To enter an alertable wait state, use the WaitForSingleObjectEx function. To wait for multiple objects, use
	/// WaitForMultipleObjects.
	/// </summary>
	/// <param name="hHandle">
	/// A handle to the object.
	///
	/// If this handle is closed while the wait is still pending, the function's behavior is undefined.
	///
	/// The handle must have the SYNCHRONIZE access right.
	/// </param>
	/// <param name="dwMilliseconds">
	/// The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object
	/// is signaled or the interval elapses. If dwMilliseconds is zero, the function does not enter a wait state if
	/// the object is not signaled; it always returns immediately. If dwMilliseconds is INFINITE, the function will
	/// return only when the object is signaled.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value indicates the event that caused the function to return.
	/// It can be one of the following values.
	/// </returns>
	[LibraryImport("kernel32.dll")]
	public static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

	/// <summary>
	/// Creates a thread that runs in the virtual address space of another process.
	///
	/// Use the CreateRemoteThreadEx function to create a thread that runs in the virtual address space of another
	/// process and optionally specify extended attributes.
	/// </summary>
	/// <param name="hProcess">
	/// A handle to the process in which the thread is to be created. The handle must have the
	/// PROCESS_CREATE_THREAD, PROCESS_QUERY_INFORMATION, PROCESS_VM_OPERATION, PROCESS_VM_WRITE, and
	/// PROCESS_VM_READ access rights, and may fail without these rights on certain platforms.
	/// </param>
	/// <param name="lpThreadAttributes">
	/// A pointer to a SECURITY_ATTRIBUTES structure that specifies a security descriptor for the new thread and
	/// determines whether child processes can inherit the returned handle. If lpThreadAttributes is NULL, the
	/// thread gets a default security descriptor and the handle cannot be inherited. The access control lists
	/// (ACL) in the default security descriptor for a thread come from the primary token of the creator.
	/// </param>
	/// <param name="dwStackSize">
	/// The initial size of the stack, in bytes. The system rounds this value to the nearest page.
	/// If this parameter is 0 (zero), the new thread uses the default size for the executable.
	/// </param>
	/// <param name="lpStartAddress">
	/// A pointer to the application-defined function of type LPTHREAD_START_ROUTINE to be executed by the
	/// thread and represents the starting address of the thread in the remote process. The function must exist
	/// in the remote process.
	/// </param>
	/// <param name="lpParameter">
	/// A pointer to a variable to be passed to the thread function.
	/// </param>
	/// <param name="dwCreationFlags">
	/// The flags that control the creation of the thread.
	/// </param>
	/// <param name="lpThreadId">
	/// A pointer to a variable that receives the thread identifier.
	///
	/// If this parameter is NULL, the thread identifier is not returned.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is a handle to the new thread.
	///
	/// If the function fails, the return value is NULL.To get extended error information, call GetLastError.
	///
	/// Note that CreateRemoteThread may succeed even if lpStartAddress points to data, code, or is not
	/// accessible. If the start address is invalid when the thread runs, an exception occurs, and the thread
	/// terminates. Thread termination due to a invalid start address is handled as an error exit for the
	/// thread's process. This behavior is similar to the asynchronous nature of CreateProcess, where the
	/// process is created even if it refers to invalid or missing dynamic-link libraries (DLL).
	/// </returns>
	[LibraryImport("kernel32.dll")]
	public static partial IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

	/// <summary>
	/// Closes an open object handle.
	/// </summary>
	/// <param name="hObject">A valid handle to an open object.</param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	///
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	///
	/// If the application is running under a debugger, the function will throw an exception if it receives
	/// either a handle value that is not valid or a pseudo-handle value. This can happen if you close a
	/// handle twice, or if you call CloseHandle on a handle returned by the FindFirstFile function instead
	/// of calling the FindClose function.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool CloseHandle(IntPtr hObject);

	/// <summary>
	/// Retrieves the termination status of the specified thread.
	/// </summary>
	/// <param name="hThread">
	/// A handle to the thread.
	///
	/// The handle must have the THREAD_QUERY_INFORMATION or THREAD_QUERY_LIMITED_INFORMATION access right.
	/// </param>
	/// <param name="lpExitCode">
	/// A pointer to a variable to receive the thread termination status.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	///
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

	/// <summary>
	/// Moves an existing file or directory, including its children, with various move options.
	///
	/// The MoveFileWithProgress function is equivalent to the MoveFileEx function, except that
	/// MoveFileWithProgress allows you to provide a callback function that receives progress notifications.
	///
	/// To perform this operation as a transacted operation, use the MoveFileTransacted function.
	/// </summary>
	/// <remarks>
	/// If the dwFlags parameter specifies MOVEFILE_DELAY_UNTIL_REBOOT, MoveFileEx fails if it cannot access
	/// the registry. The function stores the locations of the files to be renamed at restart in the following
	/// registry value: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\PendingFileRenameOperations
	/// For more information, see: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefileexw.
	/// </remarks>
	/// <param name="lpExistingFileName">
	/// The current name of the file or directory on the local computer.
	///
	/// If dwFlags specifies MOVEFILE_DELAY_UNTIL_REBOOT, the file cannot exist on a remote share, because delayed
	/// operations are performed before the network is available.
	///
	/// By default, the name is limited to MAX_PATH characters. To extend this limit to 32,767 wide characters,
	/// prepend "\\?\" to the path.
	/// </param>
	/// <param name="lpNewFileName">
	/// The new name of the file or directory on the local computer. When moving a file, the destination can be
	/// on a different file system or volume.If the destination is on another drive, you must set the
	/// MOVEFILE_COPY_ALLOWED flag in dwFlags.
	///
	/// When moving a directory, the destination must be on the same drive.
	///
	/// If dwFlags specifies MOVEFILE_DELAY_UNTIL_REBOOT and lpNewFileName is NULL, MoveFileEx registers the
	/// lpExistingFileName file to be deleted when the system restarts.If lpExistingFileName refers to a directory,
	/// the system removes the directory at restart only if the directory is empty.
	/// </param>
	/// <param name="dwFlags">
	/// This parameter can be one or more of the values defined in <see cref="MoveFileFlag"/>
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	///
	/// If the function fails, the return value is zero(0). To get extended error information,
	/// call GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll", EntryPoint = "MoveFileExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, uint dwFlags);

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
	/// </returns>
	[LibraryImport("user32.dll", SetLastError = true)]
	public static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	/// <summary>
	/// Retrieves a pseudo handle for the current process.
	/// </summary>
	/// <returns>
	/// The return value is a pseudo handle to the current process.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	public static partial IntPtr GetCurrentProcess();

	/// <summary>
	/// The ImpersonateSelf function obtains an access token that impersonates the security context of the
	/// calling process. The token is assigned to the calling thread.
	/// </summary>
	/// <param name="impersonationLevel">
	/// Specifies a SECURITY_IMPERSONATION_LEVEL enumerated type that supplies the impersonation level of
	/// the new token.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	///
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool ImpersonateSelf(uint impersonationLevel);

	/// <summary>
	/// The OpenProcessToken function opens the access token associated with a process.
	/// </summary>
	/// <param name="ProcessHandle">
	/// A handle to the process whose access token is opened. The process must have the
	/// PROCESS_QUERY_LIMITED_INFORMATION access permission. See Process Security and Access Rights for more info.
	/// </param>
	/// <param name="DesiredAccess">
	/// Specifies an access mask that specifies the requested types of access to the access token. These
	/// requested access types are compared with the discretionary access control list (DACL) of the token
	/// to determine which accesses are granted or denied.
	/// </param>
	/// <param name="TokenHandle">
	/// A pointer to a handle that identifies the newly opened access token when the function returns.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	///
	/// If the function fails, the return value is zero.To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

	/// <summary>
	/// Retrieves mitigation policy settings for the calling process.
	/// </summary>
	/// <param name="hProcess">
	/// A handle to the process. This handle must have the PROCESS_QUERY_INFORMATION access right.
	/// </param>
	/// <param name="mitigationPolicy">
	/// The mitigation policy to retrieve.
	/// </param>
	/// <param name="lpBuffer">
	/// Different based on selected mitigation policy.
	/// See https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocessmitigationpolicy
	/// for more details.
	/// </param>
	/// <param name="dwLength">
	/// The size of lpBuffer, in bytes.
	/// </param>
	/// <returns>
	/// If the function succeeds, it returns TRUE. If the function fails, it returns FALSE. To retrieve
	/// error values defined for this function, call GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool GetProcessMitigationPolicy(IntPtr hProcess, int mitigationPolicy, out ProcessMitigationDynamicCodePolicy lpBuffer, int dwLength);

	/// <summary>
	/// The LookupPrivilegeValue function retrieves the locally unique identifier (LUID) used on a
	/// specified system to locally represent the specified privilege name.
	/// </summary>
	/// <param name="lpSystemName">
	/// A pointer to a null-terminated string that specifies the name of the system on which the
	/// privilege name is retrieved. If a null string is specified, the function attempts to find
	/// the privilege name on the local system.
	/// </param>
	/// <param name="lpName">
	/// A pointer to a null-terminated string that specifies the name of the privilege, as defined
	/// in the Winnt.h header file. For example, this parameter could specify the constant,
	/// SE_SECURITY_NAME, or its corresponding string, "SeSecurityPrivilege".
	/// </param>
	/// <param name="lpLuid">
	/// A pointer to a variable that receives the LUID by which the privilege is known on the
	/// system specified by the lpSystemName parameter
	/// </param>
	/// <returns>
	/// If the function succeeds, the function returns nonzero.
	///
	/// If the function fails, it returns zero.To get extended error information, call GetLastErro
	/// </returns>
	[LibraryImport("advapi32.dll", SetLastError = true, EntryPoint = "LookupPrivilegeValueW", StringMarshalling = StringMarshalling.Utf16)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool LookupPrivilegeValue(string? lpSystemName, string lpName, out Luid lpLuid);

	/// <summary>
	/// The AdjustTokenPrivileges function enables or disables privileges in the specified access
	/// token. Enabling or disabling privileges in an access token requires TOKEN_ADJUST_PRIVILEGES
	/// access.
	/// </summary>
	/// <param name="tokenHandle">
	/// A handle to the access token that contains the privileges to be modified. The handle must
	/// have TOKEN_ADJUST_PRIVILEGES access to the token. If the PreviousState parameter is not
	/// NULL, the handle must also have TOKEN_QUERY access.
	/// </param>
	/// <param name="disableAllPrivileges">
	/// Specifies whether the function disables all of the token's privileges. If this value is
	/// TRUE, the function disables all privileges and ignores the NewState parameter. If it is
	/// FALSE, the function modifies privileges based on the information pointed to by the
	/// NewState parameter.
	/// </param>
	/// <param name="newState">
	/// A pointer to a TOKEN_PRIVILEGES structure that specifies an array of privileges and
	/// their attributes. If the DisableAllPrivileges parameter is FALSE, the AdjustTokenPrivileges
	/// function enables, disables, or removes these privileges for the token. The following table
	/// describes the action taken by the AdjustTokenPrivileges function, based on the privilege
	/// attribute.
	/// </param>
	/// <param name="bufferLength">
	/// Specifies the size, in bytes, of the buffer pointed to by the PreviousState parameter.
	/// This parameter can be zero if the PreviousState parameter is NULL.
	/// </param>
	/// <param name="previousState">
	/// A pointer to a buffer that the function fills with a TOKEN_PRIVILEGES structure that
	/// contains the previous state of any privileges that the function modifies. That is, if
	/// a privilege has been modified by this function, the privilege and its previous state
	/// are contained in the TOKEN_PRIVILEGES structure referenced by PreviousState. If the
	/// PrivilegeCount member of TOKEN_PRIVILEGES is zero, then no privileges have been changed
	/// by this function. This parameter can be NULL.
	///
	/// If you specify a buffer that is too small to receive the complete list of modified
	/// privileges, the function fails and does not adjust any privileges.In this case, the
	/// function sets the variable pointed to by the ReturnLength parameter to the number of
	/// bytes required to hold the complete list of modified privileges.
	/// </param>
	/// <param name="returnLength">
	/// A pointer to a variable that receives the required size, in bytes, of the buffer pointed
	/// to by the PreviousState parameter. This parameter can be NULL if PreviousState is NULL.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero. To determine whether the function
	/// adjusted all of the specified privileges, call GetLastError, which returns one of the
	/// following values when the function succeeds: ERROR_SUCCESS or ERROR_NOT_ALL_ASSIGNED.
	/// </returns>
	[LibraryImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool AdjustTokenPrivileges(
		IntPtr tokenHandle,
		[MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
		ref TokenPrivileges newState,
		int bufferLength,
		IntPtr previousState,
		IntPtr returnLength);

	/// <summary>
	/// The SetSecurityInfo function sets specified security information in the security
	/// descriptor of a specified object. The caller identifies the object by a handle.
	///
	/// To set the SACL of an object, the caller must have the SE_SECURITY_NAME privilege
	/// enabled.
	/// </summary>
	/// <param name="handle">
	/// A handle to the object for which to set security information.
	/// </param>
	/// <param name="objectType">
	/// A member of the SE_OBJECT_TYPE enumeration that indicates the type of object
	/// identified by the handle parameter.
	/// </param>
	/// <param name="securityInfo">
	/// A set of bit flags that indicate the type of security information to set.
	/// This parameter can be a combination of the SECURITY_INFORMATION bit flags.
	/// </param>
	/// <param name="ppsidOwner">
	/// A pointer to a SID that identifies the owner of the object. The SID must be one
	/// that can be assigned as the owner SID of a security descriptor. The SecurityInfo
	/// parameter must include the OWNER_SECURITY_INFORMATION flag. This parameter can be
	/// NULL if you are not setting the owner SID.
	/// </param>
	/// <param name="ppsidGroup">
	/// A pointer to a SID that identifies the primary group of the object. The
	/// SecurityInfo parameter must include the GROUP_SECURITY_INFORMATION flag.
	/// This parameter can be NULL if you are not setting the primary group SID.
	/// </param>
	/// <param name="ppDacl">
	/// A pointer to the new DACL for the object. This parameter is ignored unless the
	/// value of the SecurityInfo parameter includes the DACL_SECURITY_INFORMATION flag.
	/// If the value of the SecurityInfo parameter includes the DACL_SECURITY_INFORMATION
	/// flag and the value of this parameter is set to NULL, full access to the object is
	/// granted to everyone. For information about null DACLs, see Creating a DACL.
	/// </param>
	/// <param name="ppSacl">
	/// A pointer to the new SACL for the object. The SecurityInfo parameter must include
	/// any of the following flags: SACL_SECURITY_INFORMATION, LABEL_SECURITY_INFORMATION,
	/// ATTRIBUTE_SECURITY_INFORMATION, SCOPE_SECURITY_INFORMATION, or
	/// BACKUP_SECURITY_INFORMATION. If setting SACL_SECURITY_INFORMATION or
	/// SCOPE_SECURITY_INFORMATION, the caller must have the SE_SECURITY_NAME privilege
	/// enabled. This parameter can be NULL if you are not setting the SACL.
	/// </param>
	/// <returns>
	/// If the function succeeds, the function returns ERROR_SUCCESS.
	///
	/// If the function fails, it returns a nonzero error code defined in WinError.h.
	/// </returns>
	[LibraryImport("advapi32.dll", SetLastError = true)]
	public static partial uint SetSecurityInfo(
		IntPtr handle,
		uint objectType,
		uint securityInfo,
		IntPtr ppsidOwner,
		IntPtr ppsidGroup,
		IntPtr ppDacl,
		IntPtr ppSacl);

	/// <summary>
	/// Frees the specified local memory object and invalidates its handle.
	/// </summary>
	/// <param name="hMem">
	/// A handle to the local memory object. This handle is returned by either the LocalAlloc
	/// or LocalReAlloc function. It is not safe to free memory allocated with GlobalAlloc.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is NULL.
	///
	/// If the function fails, the return value is equal to a handle to the local memory
	/// object. To get extended error information, call GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	public static partial uint LocalFree(IntPtr hMem);

	/// <summary>
	/// The SetEntriesInAcl function creates a new access control list (ACL) by merging new access
	/// control or audit control information into an existing ACL structure.
	/// </summary>
	/// <param name="cCountOfExplicitEntries">
	/// The number of EXPLICIT_ACCESS structures in the pListOfExplicitEntries array.
	/// </param>
	/// <param name="pListOfExplicitEntries">
	/// A pointer to an array of EXPLICIT_ACCESS structures that describe the access
	/// control information to merge into the existing ACL.
	/// </param>
	/// <param name="OldAcl">
	/// A pointer to the existing ACL. This parameter can be NULL, in which case, the function
	/// creates a new ACL based on the EXPLICIT_ACCESS entries.
	/// </param>
	/// <param name="NewAcl">
	/// A pointer to a variable that receives a pointer to the new ACL. If the function succeeds,
	/// you must call the LocalFree function to free the returned buffer.
	/// </param>
	/// <returns>
	/// If the function succeeds, the function returns ERROR_SUCCESS.
	///
	/// If the function fails, it returns a nonzero error code defined in WinError.h.
	/// </returns>
	[LibraryImport("advapi32.dll", SetLastError = true, EntryPoint = "SetEntriesInAclW")]
	public static partial uint SetEntriesInAcl(
		uint cCountOfExplicitEntries,
		[MarshalAs(UnmanagedType.LPArray)] ExplicitAccess[] pListOfExplicitEntries,
		IntPtr OldAcl,
		out IntPtr NewAcl);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 0)]
	public struct ExplicitAccess
	{
		public uint GrfAccessPermissions;
		public uint GrfAccessMode;
		public uint GrfInheritance;
		public Trustee Trustee;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 0)]
	public struct Trustee : IDisposable
	{
		public IntPtr PMultipleTrustee;
		public MultipleTrusteeOperation MultipleTrusteeOperation;
		public TrusteeForm TrusteeForm;
		public TrusteeType TrusteeType;
		public IntPtr PtstrName;

		public readonly string Name => Marshal.PtrToStringAuto(this.PtstrName) ?? string.Empty;

		readonly void IDisposable.Dispose()
		{
			if (this.PtstrName != IntPtr.Zero)
				Marshal.Release(this.PtstrName);
		}
	}

	// Src: https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-process_mitigation_dynamic_code_policy
	public struct ProcessMitigationDynamicCodePolicy
	{
		public uint Flags;
		public readonly bool ProhibitDynamicCode => (this.Flags & 0x1) != 0;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Luid
	{
		public UInt32 LowPart;
		public Int32 HighPart;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LuidAndAttributes(Luid luid, UInt32 attributes)
	{
		public Luid Luid = luid;
		public UInt32 Attributes = attributes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TokenPrivileges
	{
		public UInt32 PrivilegeCount;
		public LuidAndAttributes Privileges; // Size defined as 1 in the public header Winnt.h
	}
}
#pragma warning restore SA1121
