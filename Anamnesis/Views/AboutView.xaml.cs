// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Navigation;
	using Anamnesis.Services;

	public partial class AboutView : UserControl
	{
		public AboutView()
		{
			this.InitializeComponent();
		}

		private void OnNavigate(object sender, RequestNavigateEventArgs e)
		{
			try
			{
				string url = e.Uri.AbsoluteUri;
				Process.Start(new ProcessStartInfo(@"cmd", $"/c start {url}") { CreateNoWindow = true });
			}
			catch (Exception ex)
			{
				Log.Write(ex, "About", Log.Severity.Error);
			}
		}

		private void OnLogsClicked(object sender, RoutedEventArgs e)
		{
			LogService.ShowLogs();
		}

		private void OnSetingsClicked(object sender, RoutedEventArgs e)
		{
			SettingsService.ShowDirectory();
		}
	}
}
