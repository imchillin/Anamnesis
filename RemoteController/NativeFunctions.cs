// © Anamnesis.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RemoteController;

/// <summary>
/// A static class for Windows interop native functions.
/// </summary>
internal static partial class NativeFunctions
{
	/// <summary>
	/// Specifies options for retrieving a module handle using native methods.
	/// </summary>
	/// <remarks>
	/// This enumeration is used to control the behavior of <see cref="GetModuleHandleEx"/>.
	/// </remarks>
	[Flags]
	public enum GetModuleFlag : uint
	{
		GET_MODULE_HANDLE_EX_FLAG_PIN = 0x00000001,
		GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002,
		GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004,
	}

	/// <summary>
	/// Retrieves a module handle for the specified module and increments the module's reference count unless
	/// GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT is specified. The module must have been loaded by the
	/// calling process.
	/// </summary>
	/// <param name="dwFlags">
	/// This parameter can be zero or one or more of the values from the <see cref="GetModuleFlag"/> enum.
	/// If the module's reference count is incremented, the caller must use the FreeLibrary function to
	/// decrement the reference count when the module handle is no longer needed.
	/// </param>
	/// <param name="lpModuleNameOrAddress">
	/// The name of the loaded module, or an address in the module (if dwFlags is
	/// GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS). For a module name, if the file name extension is omitted,
	/// the default library extension.dll is appended. The file name string can include a trailing point
	/// character (.) to indicate that the module name has no extension. The string does not have to specify
	/// a path. When specifying a path, be sure to use backslashes (\), not forward slashes(/).
	/// The name is compared(case independently) to the names of modules currently mapped into the address
	/// space of the calling process. If this parameter is NULL, the function returns a handle to the file
	/// used to create the calling process (.exe file).
	/// </param>
	/// <param name="phModule">
	/// A handle to the specified module. If the function fails, this parameter is NULL. The GetModuleHandleEx
	/// function does not retrieve handles for modules that were loaded using the LOAD_LIBRARY_AS_DATAFILE flag.
	/// For more information, see LoadLibraryEx.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.
	/// To get extended error information, see GetLastError.
	/// </returns>
	[LibraryImport("kernel32.dll", SetLastError = true, EntryPoint = "GetModuleHandleExW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool GetModuleHandleEx(uint dwFlags, IntPtr lpModuleNameOrAddress, out IntPtr phModule);

	/// <summary>
	/// Decrements the reference count of a loaded dynamic-link library (DLL) by one, then calls ExitThread
	/// to terminate the calling thread. The function does not return.
	/// </summary>
	/// <remarks>
	/// The FreeLibraryAndExitThread function allows threads that are executing within a DLL to safely free the DLL
	/// in which they are executing and terminate themselves. If they were to call FreeLibrary and ExitThread separately,
	/// a race condition would exist. The library could be unloaded before ExitThread is called.
	/// </remarks>
	/// <param name="hModule">
	/// A handle to the DLL module whose reference count the function decrements. The LoadLibrary or GetModuleHandleEx
	/// function returns this handle. Do not call this function with a handle returned by either the GetModuleHandleEx
	/// function (with the GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT flag) or the GetModuleHandle function, as they
	/// do not maintain a reference count for the module.
	/// </param>
	/// <param name="dwExitCode">
	/// The exit code for the calling thread.
	/// </param>
	[LibraryImport("kernel32.dll", SetLastError = true)]
	public static partial void FreeLibraryAndExitThread(IntPtr hModule, uint dwExitCode);
}
