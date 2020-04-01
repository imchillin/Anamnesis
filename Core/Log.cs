// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;

	public static class Log
	{
		public delegate void LogEvent(string message, string category);
		public delegate void ErrorEvent(Exception ex, string category);

		public static event LogEvent OnLog;
		public static event ErrorEvent OnError;

		public static void Write(string message, string category = null)
		{
			Console.WriteLine($"[{category}] {message}");
			OnLog?.Invoke(message, category);
		}

		public static void Write(Exception ex, string category = null)
		{
			Console.WriteLine($"[{category}][{ex.GetType().Name}] {ex.Message}");
			Console.WriteLine(ex.StackTrace);

			Exception exception = ex.InnerException;
			while (exception != null)
			{
				Console.WriteLine($"Inner: [{exception.GetType().Name}] {exception.Message}");
				Console.WriteLine(exception.StackTrace);
				exception = exception.InnerException;
			}

			OnError?.Invoke(ex, category);
		}
	}
}
