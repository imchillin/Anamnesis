// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController;

using Serilog;

/// <summary>
/// A thin wrapper around Serilog for logging within the remote controller.
/// </summary>
public static class Logger
{
	private static readonly string s_logDirectory = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Anamnesis",
		"Logs");

	private static readonly string s_logFilePath = Path.Combine(
		s_logDirectory,
		$"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_rem_ctrl.txt");

	private static bool s_initialized = false;

	/// <summary>
	/// Initializes the logger to output logs to a text file.
	/// </summary>
	/// <remarks>
	/// Call this method before using the logger to ensure that
	/// log messages are properly recorded.
	/// </remarks>
	public static void Initialize()
	{
		if (s_initialized)
			return;

		if (!Directory.Exists(s_logDirectory))
			Directory.CreateDirectory(s_logDirectory);

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.WriteTo.File(s_logFilePath, rollingInterval: RollingInterval.Infinite)
			.CreateLogger();

		s_initialized = true;
	}

	/// <summary>
	/// Releases resources and performs cleanup for the logging system.
	/// </summary>
	/// <remarks>
	/// Call this method when logging is no longer needed to ensure that log data is flushed and
	/// resources are properly released. After calling this method, the logging system must be
	/// reinitialized before use.
	/// </remarks>
	public static void Deinitialize()
	{
		if (!s_initialized)
			return;

		Log.CloseAndFlush();
		s_initialized = false;
	}
}
