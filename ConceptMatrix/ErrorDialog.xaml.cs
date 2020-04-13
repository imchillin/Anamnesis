// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI
{
	using System;
	using System.Text;
	using System.Windows;
	using System.Windows.Documents;
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for ErrorDialog.xaml.
	/// </summary>
	public partial class ErrorDialog : Window
	{
		public ErrorDialog(Exception ex, bool isCritical)
		{
			this.InitializeComponent();

			this.OkButton.Visibility = isCritical ? Visibility.Collapsed : Visibility.Visible;
			this.DetailsExpander.Header = isCritical ? "A critical error has occurred. The application must quit" : ex.Message;

			Count++;

			StringBuilder builder = new StringBuilder();
			while (ex != null)
			{
				this.StackTraceBlock.Inlines.Add(new Run("[" + ex.GetType().Name + "] ") { FontWeight = FontWeights.Bold });
				this.StackTraceBlock.Inlines.Add(ex.Message);
				this.StackTraceBlock.Inlines.Add("\n");

				this.StackTraceFormatter(ex.StackTrace);

				ex = ex.InnerException;
			}

			#if DEBUG
			this.DetailsExpander.IsExpanded = true;
			#endif
		}

		public static int Count
		{
			get;
			private set;
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
				this.StackTraceBlock.Inlines.Add(new Run(parts[0] + "\n") { ToolTip = parts[1].Trim('\r', '\n') });
			}
			else
			{
				this.StackTraceBlock.Inlines.Add(parts[0]);
			}
		}

		private void OnQuitClick(object sender, RoutedEventArgs e)
		{
			this.OnOkClick(sender, e);
			Application.Current.Shutdown(2);
		}

		private void OnOkClick(object sender, RoutedEventArgs e)
		{
			Count--;
			this.Close();
		}

		private void OnExpanded(object sender, RoutedEventArgs e)
		{
			this.Height = 350;
		}

		private void OnCollapsed(object sender, RoutedEventArgs e)
		{
			this.Height = 130;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.DragMove();
			}
		}
	}
}
