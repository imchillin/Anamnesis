// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Windows
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.CompilerServices;
	using System.Runtime.ExceptionServices;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Navigation;
	using ConceptMatrix.GUI.Services;

	/// <summary>
	/// Interaction logic for ErrorDialog.xaml.
	/// </summary>
	public partial class ErrorDialog : UserControl
	{
		private Dialog window;

		private ErrorDialog(ExceptionDispatchInfo exDispatch, bool isCritical)
		{
			this.InitializeComponent();

			this.OkButton.Visibility = isCritical ? Visibility.Collapsed : Visibility.Visible;
			this.Message.Text = isCritical ? "Critical Error" : "Error";
			this.Subtitle.Visibility = isCritical ? Visibility.Visible : Visibility.Collapsed;
			this.DetailsExpander.Header = exDispatch.SourceException.Message;

			StringBuilder builder = new StringBuilder();
			Exception ex = exDispatch.SourceException;
			while (ex != null)
			{
				this.StackTraceBlock.Inlines.Add(new Run("[" + ex.GetType().Name + "] ") { FontWeight = FontWeights.Bold });
				this.StackTraceBlock.Inlines.Add(ex.Message);
				this.StackTraceBlock.Inlines.Add("\n");

				this.StackTraceFormatter(ex.StackTrace);

				ex = ex.InnerException;
			}

			if (Debugger.IsAttached)
			{
				this.DetailsExpander.IsExpanded = true;
			}
		}

		public static void ShowError(ExceptionDispatchInfo ex, bool isCriticial)
		{
			if (Application.Current == null)
				return;

			Application.Current.Dispatcher.Invoke(() =>
			{
				SplashWindow.HideWindow();

				Dialog dlg = new Dialog();
				ErrorDialog errorDialog = new ErrorDialog(ex, isCriticial);
				errorDialog.window = dlg;
				dlg.ContentArea.Content = errorDialog;
				dlg.ShowDialog();

				if (Application.Current == null)
					return;

				if (isCriticial)
					Application.Current.Shutdown(2);

				SplashWindow.ShowWindow();
			});
		}

		private void StackTraceFormatter(string stackTrace)
		{
			if (string.IsNullOrEmpty(stackTrace))
				return;

			string[] lines = stackTrace.Split('\n');
			foreach (string line in lines)
			{
				this.StackTraceLineFormatter(line);
			}
		}

		private void StackTraceLineFormatter(string line)
		{
			string[] parts = line.Split(new[] { " in " }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 2)
			{
				this.StackTraceBlock.Inlines.Add(new Run(parts[0]));
				this.StackTraceBlock.Inlines.Add(new Run(" @ ") { Foreground = Brushes.LightGray });

				string path;
				if (this.GetPath(parts[1], out path, out _) && File.Exists(path))
				{
					Hyperlink link = new Hyperlink(new Run(parts[1] + "\n"));
					link.RequestNavigate += this.Link_RequestNavigate;
					link.NavigateUri = new Uri(parts[1]);
					this.StackTraceBlock.Inlines.Add(link);
				}
				else
				{
					this.StackTraceBlock.Inlines.Add(new Run(parts[1] + "\n") { Foreground = Brushes.Gray });
				}
			}
			else
			{
				this.StackTraceBlock.Inlines.Add(parts[0]);
			}
		}

		private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			string path;
			string line;

			if (!this.GetPath(e.Uri.OriginalString, out path, out line))
				return;

			Process[] procs = Process.GetProcessesByName("devenv");
			if (procs.Length == 0)
			{
				Process.Start(path);
			}
			else
			{
				string devEnvPath = procs[0].MainModule.FileName;
				Process.Start(devEnvPath, $"-Edit \"{path}\" -Command \"Edit.Goto {line}\"");
			}
		}

		private bool GetPath(string stackLine, out string path, out string line)
		{
			path = null;
			line = null;

			stackLine = stackLine.Trim();
			string[] parts = stackLine.Split(' ');
			if (parts.Length != 2)
				return false;

			path = parts[0];
			line = parts[1];
			path = path.Replace(":line", string.Empty);
			return true;
		}

		private void OnQuitClick(object sender, RoutedEventArgs e)
		{
			this.window.Close();
		}

		private void OnOkClick(object sender, RoutedEventArgs e)
		{
			this.window.Close();
		}

		private void OnLogClick(object sender, RoutedEventArgs e)
		{
			LogService.ShowLogs();
		}
	}
}
