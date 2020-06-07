// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Diagnostics;
	using System.Runtime.ExceptionServices;

	public static class Log
	{
		public delegate void LogEvent(string message, Severity severity, string category);
		public delegate void ExceptionEvent(ExceptionDispatchInfo ex, Severity severity, string category);

		public static event LogEvent OnLog;
		public static event ExceptionEvent OnException;

		public enum Severity
		{
			Log,
			Warning,
			Error,
			Critical,
		}

		public static void Write(string message, string category = null, Severity severity = Severity.Log)
		{
			Trace.WriteLine($"[{category}] ({severity}) {message}");
			OnLog?.Invoke(message, severity, category);
		}

		public static void Write(Exception ex, string category = null, Severity severity = Severity.Critical)
		{
			ExceptionDispatchInfo exDispatch = ExceptionDispatchInfo.Capture(ex);

			Trace.WriteLine($"[{category}][{ex.GetType().Name}] ({severity}) {ex.Message}");
			Trace.WriteLine(ex.StackTrace);

			Exception exception = ex.InnerException;
			while (exception != null)
			{
				Trace.WriteLine($"Inner: [{exception.GetType().Name}] {exception.Message}");
				Trace.WriteLine(exception.StackTrace);
				exception = exception.InnerException;
			}

			OnException?.Invoke(exDispatch, severity, category);
		}
	}
}
