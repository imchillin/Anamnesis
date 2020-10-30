// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.ExceptionServices;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using Anamnesis.Files;
	using Anamnesis.GUI.Windows;
	using SimpleLog;

	public class LogService : IService
	{
		private const string LogfilePath = "/Logs/";

		private static string? currentLogPath;

		private FileLogDestination? file;

		public static void ShowLogs()
		{
			string? dir = Path.GetDirectoryName(FileService.StoreDirectory + LogfilePath);

			if (dir == null)
				throw new Exception("Failed to get directory name for path");

			dir = FileService.ParseToFilePath(dir);
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", dir);
		}

		public static void ShowCurrentLog()
		{
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", $"/select, \"{currentLogPath}\"");
		}

		public Task Initialize()
		{
			string dir = Path.GetDirectoryName(FileService.StoreDirectory + LogfilePath) + "\\";
			dir = FileService.ParseToFilePath(dir);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string[] logs = Directory.GetFiles(dir);
			for (int i = logs.Length - 1; i >= 0; i--)
			{
				if (i <= logs.Length - 5)
				{
					File.Delete(logs[i]);
				}
			}

			currentLogPath = dir + DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm-ss") + ".txt";
			this.file = new FileLogDestination(currentLogPath);
			Log.AddDestination(this.file);
			Log.AddDestination<ErrorDialogLogDestination>();
			Log.AddDestination<TraceLogDestination>();

			Log.Write("OS: " + RuntimeInformation.OSDescription, "Info");
			Log.Write("Framework: " + RuntimeInformation.FrameworkDescription, "Info");
			Log.Write("OS Architecture: " + RuntimeInformation.OSArchitecture.ToString(), "Info");
			Log.Write("Process Architecture: " + RuntimeInformation.ProcessArchitecture.ToString(), "Info");

			string ver = "Unknown";
			if (File.Exists("Version.txt"))
				ver = File.ReadAllText("Version.txt");

			Log.Write("CM Version: " + ver, "Info");

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			lock (this)
			{
				this.file?.Dispose();
				this.file = null;
			}

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private class ErrorDialogLogDestination : ILogDestination
		{
			public void OnException(Severity severity, ExceptionDispatchInfo exception, string? category = null)
			{
				if (severity >= Severity.Error)
				{
					ErrorDialog.ShowError(exception, severity == Severity.UnhandledException);
				}
			}

			public void OnLog(Severity severity, string message, string? category = null, string? stack = null)
			{
				if (severity >= Severity.Error)
				{
					Exception ex = new Exception(message);
					ErrorDialog.ShowError(ExceptionDispatchInfo.Capture(ex), severity == Severity.UnhandledException);
				}
			}
		}
	}
}
