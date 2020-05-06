// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;

	public class LogService : IService
	{
		private const string LogfilePath = "/Logs/";

		private TextWriter logWriter;

		public static void ShowLogs()
		{
			string dir = Path.GetDirectoryName(FileService.StoreDirectory + LogfilePath);
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", dir);
		}

		public Task Initialize()
		{
			Log.OnLog += this.OnLog;
			Log.OnException += this.OnException;

			string dir = Path.GetDirectoryName(FileService.StoreDirectory + LogfilePath) + "\\";

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

			string newLogPath = dir + DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm-ss") + ".txt";
			this.logWriter = new StreamWriter(newLogPath);

			Log.Write("OS: " + RuntimeInformation.OSDescription, "Info");
			Log.Write("Framework: " + RuntimeInformation.FrameworkDescription, "Info");
			Log.Write("OS Architecture: " + RuntimeInformation.OSArchitecture.ToString(), "Info");
			Log.Write("Process Architecture: " + RuntimeInformation.ProcessArchitecture.ToString(), "Info");

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			lock (this)
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
			lock (this)
			{
				if (this.logWriter == null)
					return;

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
			lock (this)
			{
				if (this.logWriter == null)
					return;

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
