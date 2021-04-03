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

		public App()
		{
			AssemblyLoadContext.Default.Resolving += this.ResolveAssembly;
		}

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

		private Assembly? ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
		{
			if (name.Name == null)
				return null;

			string path = AppContext.BaseDirectory + "/bin/" + name.Name + ".dll";
			if (File.Exists(path))
				return context.LoadFromAssemblyPath(path);

			return null;
		}
	}
}
