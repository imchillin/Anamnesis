// © Anamnesis.
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
	using Anamnesis.Windows;
	using Serilog;
	using Serilog.Core;
	using Serilog.Events;

	public class LogService : IService
	{
		private const string LogfilePath = "/Logs/";

		private static readonly LoggingLevelSwitch LogLevel = new LoggingLevelSwitch()
		{
#if DEBUG
			MinimumLevel = LogEventLevel.Verbose,
#else
			MinimumLevel = LogEventLevel.Debug,
#endif
		};

		private static LogService? instance;
		private static string? currentLogPath;

		public static LogService Instance
		{
			get
			{
				if (instance == null)
					throw new Exception("No logging service found");

				return instance;
			}
		}

		public bool VerboseLogging
		{
			get => LogLevel.MinimumLevel == LogEventLevel.Verbose;
			set => LogLevel.MinimumLevel = value ? LogEventLevel.Verbose : LogEventLevel.Debug;
		}

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

		public static void CreateLog()
		{
			if (!string.IsNullOrEmpty(currentLogPath))
				return;

			string dir = Path.GetDirectoryName(FileService.StoreDirectory + LogfilePath) + "\\";
			dir = FileService.ParseToFilePath(dir);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string[] logs = Directory.GetFiles(dir);
			for (int i = logs.Length - 1; i >= 0; i--)
			{
				if (i <= logs.Length - 15)
				{
					File.Delete(logs[i]);
				}
			}

			currentLogPath = dir + DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm-ss") + ".txt";

			LoggerConfiguration config = new LoggerConfiguration();
			config.MinimumLevel.ControlledBy(LogLevel);
			config.WriteTo.File(currentLogPath);
			config.WriteTo.Sink<ErrorDialogLogDestination>();
			config.WriteTo.Debug();

			Serilog.Log.Logger = config.CreateLogger();

			Log.Information("OS: " + RuntimeInformation.OSDescription, "Info");
			Log.Information("Framework: " + RuntimeInformation.FrameworkDescription, "Info");
			Log.Information("OS Architecture: " + RuntimeInformation.OSArchitecture.ToString(), "Info");
			Log.Information("Process Architecture: " + RuntimeInformation.ProcessArchitecture.ToString(), "Info");
			Log.Information("Anamnesis Version: " + VersionInfo.Date.ToString(@"yyyy-MM-dd HH:mm"), "Info");
		}

		public Task Initialize()
		{
			instance = this;
			CreateLog();

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private class ErrorDialogLogDestination : ILogEventSink
		{
			public void Emit(LogEvent logEvent)
			{
				if (logEvent.Level >= LogEventLevel.Error)
				{
					ErrorDialog.ShowError(ExceptionDispatchInfo.Capture(new Exception(logEvent.MessageTemplate.Text, logEvent.Exception)), logEvent.Level == LogEventLevel.Fatal);
				}
			}
		}
	}
}
