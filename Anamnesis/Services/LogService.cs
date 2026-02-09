// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.Windows;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

/// <summary>
/// A service that handles logging and error reporting in the application.
/// </summary>
public class LogService : ServiceBase<LogService>
{
	private const string LOG_FILE_PATH = "/Logs/";
	private const int NUM_LOGS_TO_KEEP = 16;

	private static readonly LoggingLevelSwitch s_logLevel = new()
	{
#if DEBUG
		MinimumLevel = LogEventLevel.Verbose,
#else
			MinimumLevel = LogEventLevel.Debug,
#endif
	};

	private static string? s_currentLogPath;

	/// <summary>
	/// Gets or sets the minimum logging level for the application.
	/// </summary>
	/// <remarks>
	/// Setting this to true will enable verbose logging.
	/// Otherwise, the minimum level will be set to debug.
	/// </remarks>
	public static bool VerboseLogging
	{
		get => s_logLevel.MinimumLevel == LogEventLevel.Verbose;
		set => s_logLevel.MinimumLevel = value ? LogEventLevel.Verbose : LogEventLevel.Debug;
	}

	/// <summary>
	/// Opens the application's logs directory in Windows Explorer.
	/// </summary>
	/// <exception cref="Exception">Thrown if the logs directory could not be retrieved.</exception>
	public static void ShowLogs()
	{
		string? dir = Path.GetDirectoryName(FileService.StoreDirectory + LOG_FILE_PATH)
			?? throw new Exception("Failed to get directory name for path");

		dir = FileService.ParseToFilePath(dir);
		Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", dir);
	}

	/// <summary>
	/// Opens the application's logs directory and selects the current log file.
	/// </summary>
	public static void ShowCurrentLog()
	{
		Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", $"/select, \"{s_currentLogPath}\"");
	}

	/// <summary>
	/// Creates a new log file in the application's logs directory.
	/// </summary>
	public static void CreateLog()
	{
		if (!string.IsNullOrEmpty(s_currentLogPath))
			return;

		string dir = Path.GetDirectoryName(FileService.StoreDirectory + LOG_FILE_PATH) + "\\";
		dir = FileService.ParseToFilePath(dir);

		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		var orderedLogs = Directory.GetFiles(dir)
			.Select(f => new FileInfo(f))
			.OrderBy(f => f.CreationTimeUtc)
			.ToArray();

		for (int i = 0; i < orderedLogs.Length - NUM_LOGS_TO_KEEP; i++)
		{
			orderedLogs[i].Attributes = FileAttributes.Normal;
			File.Delete(orderedLogs[i].FullName);
		}

		s_currentLogPath = dir + DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm-ss") + ".txt";

		var config = new LoggerConfiguration();
		config.MinimumLevel.ControlledBy(s_logLevel);
		config.WriteTo.File(s_currentLogPath);
		config.WriteTo.Sink<ErrorDialogLogDestination>();
		config.WriteTo.Debug();

		Serilog.Log.Logger = config.CreateLogger();

		Log.Information("OS: " + RuntimeInformation.OSDescription, "Info");
		Log.Information("Framework: " + RuntimeInformation.FrameworkDescription, "Info");
		Log.Information("OS Architecture: " + RuntimeInformation.OSArchitecture.ToString(), "Info");
		Log.Information("Process Architecture: " + RuntimeInformation.ProcessArchitecture.ToString(), "Info");
		Log.Information($"Anamnesis Version: v{VersionInfo.ApplicationVersion}", "Info");
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		CreateLog();
		await base.Initialize();
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
