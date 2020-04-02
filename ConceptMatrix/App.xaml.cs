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
			Log.OnError += this.OnError;
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
			this.OnError(e.Exception as Exception, "Unhandled");
		}

		private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			this.OnError(e.Exception as Exception, "Unhandled");
		}

		private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			this.OnError(e.Exception as Exception, "Unhandled");
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			this.OnError(e.ExceptionObject as Exception, "Unhandled");
		}

		private async Task Start()
		{
			try
			{
				await Services.InitializeServices();

				Settings = await App.Services.Get<ISettingsService>().Load<MainApplicationSettings>();

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

		private void OnError(Exception ex, string category)
		{
			if (Application.Current == null)
				return;

			Application.Current.Dispatcher.Invoke(() =>
			{
				ErrorDialog dlg = new ErrorDialog(ex);
				dlg.Owner = this.MainWindow;
				dlg.ShowDialog();
			});
		}
	}
}
