// © Anamnesis.
// Licensed under the MIT license.

namespace UpdateExtractor;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

public class Program
{
	private const int MSI_ERROR_SUCCESS = 0;
	private const int MSI_ERROR_SUCCESS_REBOOT_INITIATED = 1641;
	private const int MSI_ERROR_SUCCESS_REBOOT_REQUIRED = 3010;

	public static void Main(string[] args)
	{
		string processName = "Unknown";

		try
		{
			if (args.Length != 2)
				throw new Exception("Invalid arguments. Update Extractor must be run with the following arguments: 1) destination directory, 2) name of orignal process.");

			string? destDir = args[0];
			processName = args[1];
			Console.Write($"Waiting for {processName} to terminate");
			while (true)
			{
				Process[] procs = Process.GetProcessesByName(processName);
				if (procs.Length <= 0)
				{
					break;
				}
				else
				{
					Thread.Sleep(100);
					Console.Write(".");
				}
			}

			Console.WriteLine(" done.");

			string sourceDir = GetCurrentDirectory();
			if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
				throw new Exception("Unable to determine source directory");

			if (string.IsNullOrEmpty(destDir) || (!Directory.Exists(destDir) && !File.Exists(destDir)))
				throw new Exception("Unable to determine cleanup directory");

			RunVelopackMigration(sourceDir, destDir);
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Failed to update {processName}");
			Console.WriteLine(ex.Message);
			Console.WriteLine();
			Console.WriteLine("Please download the update manually.");
			Console.WriteLine();
			Console.WriteLine("Press any key to close this window.");
			Console.ReadKey();
		}
	}

	public static string GetCurrentDirectory()
	{
		string basePath = AppContext.BaseDirectory;
		if (File.Exists(basePath) || basePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || basePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			basePath = Path.GetDirectoryName(basePath) ?? basePath;

		return basePath;
	}

	private static void DeleteFileIfExists(string path)
	{
		if (!File.Exists(path))
			return;

		var fileInfo = new FileInfo(path)
		{
			Attributes = FileAttributes.Normal,
		};

		File.Delete(path);
	}

	private static void DeleteDirectoryIfExists(string path)
	{
		if (!Directory.Exists(path))
			return;

		SetAttributesNormal(new DirectoryInfo(path));
		Directory.Delete(path, true);
	}

	private static void SetAttributesNormal(DirectoryInfo directory)
	{
		directory.Attributes = FileAttributes.Normal;

		foreach (var subDirectory in directory.GetDirectories())
		{
			SetAttributesNormal(subDirectory);
		}

		foreach (var file in directory.GetFiles())
		{
			file.Attributes = FileAttributes.Normal;
		}
	}

	private static void LogFatalError(params string[] lines)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		foreach (string line in lines)
		{
			Console.WriteLine(line);
		}

		Console.WriteLine();
		Console.WriteLine("Press any key to close this window.");
		Console.ReadKey();
	}

	private static void DeleteLegacyFiles(string? path)
	{
		Console.WriteLine("Cleaning up legacy files...");
		Console.WriteLine($"> Path: {path}");

		if (string.IsNullOrEmpty(path))
			throw new Exception("Unable to determine cleanup directory");

		if (File.Exists(path))
		{
			path = Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(path))
				throw new Exception("Unable to determine cleanup directory");
		}

		path = Path.TrimEndingDirectorySeparator(path);

		string oldExe = Path.Combine(path, "Anamnesis.exe");
		if (!File.Exists(oldExe))
			throw new FileNotFoundException($"No Anamnesis executable found at: {oldExe}");

		DeleteFileIfExists(Path.Combine(path, "Anamnesis.exe"));
		DeleteFileIfExists(Path.Combine(path, "AnamnesisLauncher.exe"));
		DeleteFileIfExists(Path.Combine(path, "Anamnesis.pdb"));
		DeleteFileIfExists(Path.Combine(path, "Anamnesis.xml"));
		DeleteFileIfExists(Path.Combine(path, "Version.txt"));
		DeleteFileIfExists(Path.Combine(path, "AetherTools.Anamnesis.Reloaded.Assembler.targets"));
		DeleteFileIfExists(Path.Combine(path, "Download dotNet desktop runtime"));
		DeleteFileIfExists(Path.Combine(path, "Install .NET Desktop Runtime.bat"));
		DeleteDirectoryIfExists(Path.Combine(path, "Data"));
		DeleteDirectoryIfExists(Path.Combine(path, "Languages"));
		DeleteDirectoryIfExists(Path.Combine(path, "Updater"));
		DeleteDirectoryIfExists(Path.Combine(path, "bin"));
	}

	private static void RunVelopackMigration(string sourceDir, string cleanupPath)
	{
		Console.WriteLine("Starting application installer migration process...");
		string[] msiFiles = Directory.GetFiles(sourceDir, "*.msi");

		// In theory, there should be exactly one .msi installer file in the source directory
		// This is done as a precaution if that assumption is ever violated
		if (msiFiles.Length != 1)
		{
			LogFatalError(
				$"Expected exactly one .msi file in the source directory, but found {msiFiles.Length}.",
				"Please ensure that the update package is correct and try again.");
			return;
		}

		string? msiPath = msiFiles.FirstOrDefault();
		if (msiPath == null || !File.Exists(msiPath))
		{
			LogFatalError("Failed to locate the required .msi installer file.");
			return;
		}

		var msiStartInfo = new ProcessStartInfo
		{
			FileName = "msiexec.exe",
			Arguments = $"/i \"{msiPath}\"",
			UseShellExecute = true,
		};

		Console.WriteLine($"Executing system installer package: {msiStartInfo.Arguments}");
		Console.WriteLine("Follow the setup installer wizard instructions to complete your update migration process.");

		Process msiProcess = Process.Start(msiStartInfo) ?? throw new Exception("Failed to start MSI installer process.");
		msiProcess.WaitForExit();

		// Ref: https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
		if (msiProcess.ExitCode == MSI_ERROR_SUCCESS
			|| msiProcess.ExitCode == MSI_ERROR_SUCCESS_REBOOT_INITIATED
			|| msiProcess.ExitCode == MSI_ERROR_SUCCESS_REBOOT_REQUIRED)
		{
			try
			{
				DeleteLegacyFiles(cleanupPath);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Migration completed successfully");
				Console.WriteLine("Closing updater utility...");
				Console.ResetColor();

				Thread.Sleep(1500);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"Warning: Installer succeeded, but old file cleanup failed: {ex.Message}");
				Console.ResetColor();

				Console.WriteLine();
				Console.WriteLine("Press any key to close this window.");
				Console.ReadKey();
			}
		}
		else
		{
			// User likely cancelled the install wizard or encountered an unhandled system environment error
			LogFatalError(
				$"Installation was not completed successfully (Exit Code: {msiProcess.ExitCode}).",
				"Legacy folder cleanup was skipped to avoid data corruption.",
				"Please run the update again, or install the app manually from the official releases page.");
		}

		return;
	}
}
