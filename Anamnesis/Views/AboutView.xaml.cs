// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Navigation;
	using Anamnesis.GUI.Services;

	public partial class AboutView : UserControl
	{
		public AboutView()
		{
			this.InitializeComponent();

			try
			{
				string str = File.ReadAllText("Version.txt");
				string[] parts = str.Split(' ');
				this.VersionDisplay.Text = parts[0];
				this.VersionDisplay2.Text = parts[1];
				this.BuildMachineDisplay.Text = parts[2];
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Version", Log.Severity.Warning);
				this.VersionDisplay.Text = "Unknown";
				this.VersionDisplay2.Text = string.Empty;
				this.BuildMachineDisplay.Text = string.Empty;
			}
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
