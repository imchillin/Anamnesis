using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ConceptMatrixUpdater
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		// Constants for the tool to make it easier to update and swap out.
		public static readonly string ToolBin = "ConceptMatrix";
		public static readonly string ToolName = "Concept Matrix";
		public static readonly string UpdaterName = "Concept Matrix Updater";
		public static readonly string UpdaterBin = "ConceptMatrixUpdater";
		public static readonly string GithubRepo = "imchillin/CMTool";
		public static readonly string ZipName = "CMTool.zip";

		/// <summary>
		/// Application startup event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			// Create MainWindow.
			var window = new MainWindow();

			// Loop over the arguments.
			foreach (var arg in e.Args)
			{
				// Used in the tool, avoids the "Up to date" message.
				if (arg.Contains("--checkUpdate"))
					window.AlertUpToDate = false;
				// Force an update even if the tool is up to date.
				if (arg.Contains("--forceUpdate"))
					window.ForceCheckUpdate = true;
			}

/*#if DEBUG
			// Force check update while in debug.
			window.ForceCheckUpdate = true;
#endif*/

			// Initialize the update process.
			if (window.Initialize())
			{
				// Display the MainWindow.
				window.Show();
			}
			else
			{
				// Shut down the updater.
				Current.Shutdown();
			}
		}

		public App()
		{
			// When the application is closing.
			Exit += (_, __) =>
			{
				// Get bat file.
				var batFile = Path.Combine(Path.GetTempPath(), "ConceptMatrix", "UpdateReplacer.bat");
				var oldUpdater = Path.Combine(Environment.CurrentDirectory, $"{UpdaterBin}.exe.old");

				// Remove existing bat file.
				if (File.Exists(batFile))
					File.Delete(batFile);

				// If an old version of the updater exists.
				if (File.Exists(oldUpdater))
				{
					// Use stream writer to write bat file.
					using (var writer = new StreamWriter(batFile))
					{
						// Write the bat file to kill the old updater.
						writer.WriteLine("@echo off");
						writer.WriteLine("@echo Attempting to replace updater, please wait...");
						writer.WriteLine("@ping -n 4 127.0.0.1 > nul");
						writer.WriteLine($"@del \"{oldUpdater}\"");
						writer.WriteLine("@del \"%~f0\"");
						writer.Close();

						// Create bat process and initialize values to hide in background.
						var batProc = new Process();
						batProc.StartInfo.CreateNoWindow = true;
						batProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						batProc.StartInfo.FileName = batFile;

						// Start the process.
						batProc.Start();
					}
				}
			};
		}
	}
}
