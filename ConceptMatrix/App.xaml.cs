// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Threading;
	using ConceptMatrix.GUI;
	using ConceptMatrix.Services;
	using Application = System.Windows.Application;

	/// <summary>
	/// Interaction logic for App.xaml.
	/// </summary>
	public partial class App : Application
	{
		public static readonly ServiceManager Services = new ServiceManager();

		public static MainApplicationSettings Settings
		{
			get;
			private set;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
			this.Dispatcher.UnhandledException += this.DispatcherOnUnhandledException;
			Application.Current.DispatcherUnhandledException += this.CurrentOnDispatcherUnhandledException;
			TaskScheduler.UnobservedTaskException += this.TaskSchedulerOnUnobservedTaskException;
			Log.OnException += this.OnException;
			this.Exit += this.OnExit;

			base.OnStartup(e);

			this.MainWindow = new SplashWindow();
			this.MainWindow.Show();

			Task.Run(this.Start);
		}

		private void OnExit(object sender, ExitEventArgs e)
		{
			Task t = Services.ShutdownServices();
			t.Wait();
			Log.Write("Bye!");
		}

		private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			Log.Write(e.Exception, @"Unhandled Task", Log.Severity.Critical);
		}

		private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Write(e.Exception, @"Unhandled Current Dispatcher", Log.Severity.Critical);
		}

		private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Write(e.Exception, @"Unhandled Dispatcher", Log.Severity.Critical);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Write(e.ExceptionObject as Exception, @"Unhandled", Log.Severity.Critical);
		}

		private async Task Start()
		{
			try
			{
				await Services.InitializeServices();

				Settings = await App.Services.Get<ISettingsService>().Load<MainApplicationSettings>();

				while (ErrorDialog.Count > 0)
					await Task.Delay(500);

				Application.Current.Dispatcher.Invoke(() =>
				{
					Window oldwindow = this.MainWindow;
					this.MainWindow = new ConceptMatrix.GUI.MainWindow();
					this.MainWindow.Show();
					oldwindow.Close();
				});
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void OnException(Exception ex, Log.Severity severity, string category)
		{
			if (Application.Current == null)
				return;

			Application.Current.Dispatcher.Invoke(() =>
			{
				ErrorDialog dlg = new ErrorDialog(ex, severity == Log.Severity.Critical);
				dlg.Owner = this.MainWindow;
				dlg.ShowDialog();
			});
		}
	}
}
