// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;

	/// <summary>
	/// Basic log for when modules dont want to reference SimpleLog.
	/// </summary>
	public static class Log
	{
		public enum Severity
		{
			Log = SimpleLog.Severity.Log,
			Warning = SimpleLog.Severity.Warning,
			Error = SimpleLog.Severity.Error,
			Critical = SimpleLog.Severity.UnhandledException,
		}

		public static void Write(string message, string? category = null, Severity severity = Severity.Log)
		{
			SimpleLog.Log.Write((SimpleLog.Severity)severity, message);
		}

		public static void Write(Exception ex, string? category = null, Severity severity = Severity.Critical)
		{
			SimpleLog.Log.Write((SimpleLog.Severity)severity, ex);
		}
	}
}
