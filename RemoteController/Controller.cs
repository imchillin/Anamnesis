// © Anamnesis.
// Licensed under the MIT license.

// Notes:
// This will be the controller that communicates with the main application.
// The controller will be injected as a dll into an external process. Furthermore, it will be used to create function hooks, with
// back/forward communication with the main application using a low-latency protocol (perhaps IPC or named pipes?).

// On the main application side:
// - Have a service that performs the DLL injection into the target process. Once injected, do a test ping-pong to ensure communication is established.
// If successful, start listening for commands from the controller.

// TODO:
// - Start by creating a basic controller that sends a periodic ping to the main application to ensure communication is established without
// accessing any of the game's memory or functionality.
// - Define a protocol (message format) for communication between the controller and the main application. this should be as small in size as possible to reduce latency.
//   - Perhaps use protobuf (https://protobuf.dev/reference/csharp/csharp-generated/#invocation)
// - Figure out how to use shared memory files to exchange data between the controller and the main application.
// - https://learn.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files
//   - Recommended use of stream access views for non-persistent files for IPC.
//   - The shared memory map will not persist across process restarts and will basically be a clean slate each time the controller is injected into a new process.

// Message:
// - Header (fixed size, containing metadata such as message length, type, command ID, status flags, message count, etc.)
// - Payload (variable size, containing the actual data to be sent)

// The shared memory file will be a representation of a ring buffer.
// - The different processes have read/write pointers on that file, presented as offsets in the file.

using Serilog;
using System.Runtime.InteropServices;

using static RemoteController.NativeFunctions;

namespace RemoteController;

public class Controller
{
	// TODO: Implement a watchdog-heartbeat mechanism to keep the remote controller alive by sending heartbeat messages from the main
	// application. If the controller does not receive a heartbeat within a certain timeout (e.g., 60 seconds), it should exit out of the
	// loop to unload itself from the target process.

	private static unsafe void* NativePtr() => (delegate* unmanaged<void>)&RemoteControllerEntry;

	[UnmanagedCallersOnly(EntryPoint = "RemoteControllerEntry")]
	public static void RemoteControllerEntry()
	{
		try
		{
			Logger.Initialize();
			Log.Information("Starting remote controller...");

			Thread.Sleep(10000); // Simulate some work being done.
		}
		finally
		{
			Cleanup();

			// Unload
			// IMPORTANT: This MUST be the last action to execute in the entry point.
			// Otherwise, the code after this call will not execute as we self-unload and terminate the thread.
			unsafe
			{
				if (GetModuleHandleEx(
						(uint)GetModuleFlag.GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | (uint)GetModuleFlag.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
						(IntPtr)NativePtr(),
						out IntPtr hModule))
				{
					FreeLibraryAndExitThread(hModule, 0);
				}
			}
		}
	}

	private static void Cleanup()
	{
		Log.Information("Running shutdown sequence...");
		Logger.Deinitialize();
	}
}
