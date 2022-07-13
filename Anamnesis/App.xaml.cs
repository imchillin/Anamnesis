// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Anamnesis.Navigation;
using Anamnesis.Panels;
using Anamnesis.Services;
using Anamnesis.Windows;
using Serilog;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using Application = System.Windows.Application;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
	public static readonly ServiceManager Services = new ServiceManager();

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
			this.PerformanceWatcher().Run();
			this.MemoryWatcher().Run();

			LogService.CreateLog();

			this.CheckWindowsVersion();
			this.CheckWorkingDirectory();
			this.CheckForProcesses();

			await Services.InitializeCriticalServices();
			SettingsService.ApplyTheme();
			await Dispatch.MainThread();

			Window oldwindow = this.MainWindow;

			if (SettingsService.Current.OverlayWindow)
			{
				IPanelGroupHost wnd = new OverlayWindow();
				NavigationPanel nav = new(wnd);
				wnd.PanelGroupArea.Content = nav;
				wnd.Show();

				this.MainWindow = wnd as Window;
			}
			else
			{
				this.MainWindow = new Anamnesis.Windows.MainWindow();
				this.MainWindow.Show();
			}

			oldwindow.Close();

			await Services.InitializeServices();
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to start application");
			ErrorDialog.ShowError(ExceptionDispatchInfo.Capture(ex), true);
		}

		sw.Stop();
		Log.Information($"Started application in {sw.ElapsedMilliseconds}ms");
	}

	private void CheckWindowsVersion()
	{
		OperatingSystem os = Environment.OSVersion;
		if (os.Platform != PlatformID.Win32NT)
			throw new Exception("Only Windows NT or later is supported");

		if (os.Version.Major < 10)
		{
			throw new Exception("Only Windows 10 or newer is supported");
		}
	}

	private void CheckWorkingDirectory()
	{
		string name = Process.GetCurrentProcess().ProcessName;

		string currentDir = Directory.GetCurrentDirectory();
		Log.Information($"Check Working Directory: \"{currentDir}\" for executable: {name}");

		if (!File.Exists($"{currentDir}/{name}.exe"))
		{
			string? currentPath = AppContext.BaseDirectory;

			if (string.IsNullOrEmpty(currentPath))
				throw new Exception($"Failed to get current path");

			string? newDir = Path.GetDirectoryName(currentPath);

			if (string.IsNullOrEmpty(newDir))
				throw new Exception($"Failed to get current directory");

			currentDir = Path.GetFullPath(newDir);

			if (!File.Exists($"{currentDir}/{name}.exe"))
				throw new Exception($"Incorrect new working directory: {currentDir}");

			Directory.SetCurrentDirectory(currentDir);
			Log.Information($"Set Working Directory: \"{currentDir}\"");
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

	private async Task MemoryWatcher()
	{
		Process proc = Process.GetCurrentProcess();

		while (this._contentLoaded)
		{
			await Task.Delay(60000);
			Log.Information($"{proc.PrivateMemorySize64 / 1024 / 1024}Mb memory In use.");
		}
	}
}
