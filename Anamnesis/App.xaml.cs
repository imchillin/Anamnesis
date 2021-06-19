// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.ExceptionServices;
	using System.Runtime.Loader;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Threading;
	using Anamnesis.GUI;
	using Anamnesis.GUI.Windows;
	using Anamnesis.Services;
	using MaterialDesignThemes.Wpf;
	using Serilog;
	using Application = System.Windows.Application;

	/// <summary>
	/// Interaction logic for App.xaml.
	/// </summary>
	public partial class App : Application
	{
		private static readonly ServiceManager Services = new ServiceManager();

		protected override void OnStartup(StartupEventArgs e)
		{
			AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
			this.Dispatcher.UnhandledException += this.DispatcherOnUnhandledException;
			Application.Current.DispatcherUnhandledException += this.CurrentOnDispatcherUnhandledException;
			TaskScheduler.UnobservedTaskException += this.TaskSchedulerOnUnobservedTaskException;
			this.Exit += this.OnExit;

			base.OnStartup(e);

			this.MainWindow = new SplashWindow();
			this.MainWindow.Show();

			Task.Run(this.Start);
		}

		private void OnExit(object sender, ExitEventArgs e)
		{
			Task.Run(async () => { await Services.ShutdownServices(); });
		}

		private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Exception == null)
				return;

			Log.Fatal(e.Exception, e.Exception.Message);
		}

		private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.Exception, e.Exception.Message);
		}

		private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.Exception, e.Exception.Message);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception? ex = e.ExceptionObject as Exception;

			if (ex == null)
				return;

			Log.Fatal(ex, ex.Message);
		}

		private async Task Start()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			try
			{
				_ = Task.Run(this.PerformanceWatcher);

				LogService.CreateLog();

				this.CheckWorkingDirectory();
				this.CheckForProcesses();

				await Services.InitializeServices();

				await Dispatch.MainThread();

				Window oldwindow = this.MainWindow;
				this.MainWindow = new Anamnesis.GUI.MainWindow();
				this.MainWindow.Show();
				oldwindow.Close();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to start application");
				ErrorDialog.ShowError(ExceptionDispatchInfo.Capture(ex), true);
			}

			sw.Stop();
			Log.Information($"Started application in {sw.ElapsedMilliseconds}ms");
		}

		private void CheckWorkingDirectory()
		{
			string currentDir = Directory.GetCurrentDirectory();
			Log.Information($"Check Working Directory: \"{currentDir}\"");

			if (!File.Exists(currentDir + "/Anamnesis.exe"))
			{
				string? currentPath = AppContext.BaseDirectory;

				if (string.IsNullOrEmpty(currentPath))
					throw new Exception($"Failed to get current path");

				string? newDir = Path.GetDirectoryName(currentPath);

				if (string.IsNullOrEmpty(newDir))
					throw new Exception($"Failed to get current directory");

				currentDir = Path.GetFullPath(newDir);

				if (!File.Exists(currentDir + "/Anamnesis.exe"))
					throw new Exception($"Incorrect new working directory: {currentDir}");

				Directory.SetCurrentDirectory(currentDir);
				Log.Information($"Set Working Directory: \"{currentDir}\"");
			}

			if (currentDir.Contains("\\AppData\\Local\\Temp\\"))
			{
				throw new Exception("Attempt to run from temporary directory. (Are you running Anamneis from a zip file?)");
			}
		}

		private void CheckForProcesses()
		{
			string name = Process.GetCurrentProcess().ProcessName;
			Process[] processes = Process.GetProcessesByName(name);

			if (processes.Length < 1)
				throw new Exception($"Unable to locate {name} process.");

			if (processes.Length > 1)
			{
				throw new Exception($"Multiple {name} processes found. Please close all other instances.");
			}
		}

		private async Task PerformanceWatcher()
		{
			Stopwatch sw = new Stopwatch();
			while (this._contentLoaded)
			{
				await Task.Delay(500);

				sw.Restart();
				await Dispatch.MainThread();
				await Task.Delay(16);
				await Dispatch.MainThread();
				long ms = sw.ElapsedMilliseconds;

				if (ms > 50)
				{
					Log.Warning($"UI thread took {ms}ms to tick");
				}
			}
		}
	}
}
