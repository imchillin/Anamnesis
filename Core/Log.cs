// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;

	public static class Log
	{
		public delegate void LogEvent(string message, Severity severity, string category);
		public delegate void ExceptionEvent(Exception ex, Severity severity, string category);

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
			Console.WriteLine($"[{category}] ({severity}) {message}");
			OnLog?.Invoke(message, severity, category);
		}

		public static void Write(Exception ex, string category = null, Severity severity = Severity.Critical)
		{
			Console.WriteLine($"[{category}][{ex.GetType().Name}] ({severity}) {ex.Message}");
			Console.WriteLine(ex.StackTrace);

			Exception exception = ex.InnerException;
			while (exception != null)
			{
				Console.WriteLine($"Inner: [{exception.GetType().Name}] {exception.Message}");
				Console.WriteLine(exception.StackTrace);
				exception = exception.InnerException;
			}

			OnException?.Invoke(ex, severity, category);
		}
	}
}
