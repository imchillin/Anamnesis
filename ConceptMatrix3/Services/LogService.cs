// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.IO;
	using System.Threading.Tasks;

	public class LogService : IService
	{
		public const string LogfilePath = "Log.txt";

		private TextWriter logWriter;

		public Task Initialize()
		{
			Log.OnLog += this.OnLog;
			Log.OnException += this.OnException;

			if (File.Exists(LogfilePath))
				File.Delete(LogfilePath);

			this.logWriter = new StreamWriter(LogfilePath);

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			lock (this.logWriter)
			{
				this.logWriter.Dispose();
				this.logWriter = null;
			}

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private void OnException(Exception ex, Log.Severity severity, string category)
		{
			if (this.logWriter == null)
				return;

			lock (this.logWriter)
			{
				this.logWriter.Write("[");
				this.logWriter.Write(category);
				this.logWriter.Write("][");
				this.logWriter.Write(severity);
				this.logWriter.Write("] ");

				while (ex != null)
				{
					this.logWriter.WriteLine(ex.GetType().Name);
					this.logWriter.WriteLine(ex.Message);
					this.logWriter.WriteLine(ex.StackTrace);

					ex = ex.InnerException;
				}

				this.logWriter.Flush();
			}
		}

		private void OnLog(string message, Log.Severity severity, string category)
		{
			if (this.logWriter == null)
				return;

			lock (this.logWriter)
			{
				this.logWriter.Write("[");
				this.logWriter.Write(category);
				this.logWriter.Write("][");
				this.logWriter.Write(severity);
				this.logWriter.Write("] ");
				this.logWriter.WriteLine(message);

				this.logWriter.Flush();
			}
		}
	}
}
