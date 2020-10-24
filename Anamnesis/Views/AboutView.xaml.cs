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
	using SimpleLog;

	public partial class AboutView : UserControl
	{
		public AboutView()
		{
			this.InitializeComponent();
		}

		private void OnNavigate(object sender, RequestNavigateEventArgs e)
		{
			UrlUtility.Open(e.Uri.AbsoluteUri);
		}

		private void OnLogsClicked(object sender, RoutedEventArgs e)
		{
			LogService.ShowLogs();
		}

		private void OnSetingsClicked(object sender, RoutedEventArgs e)
		{
			SettingsService.ShowDirectory();
		}

		private void OnErrorClicked(object sender, RoutedEventArgs e)
		{
			throw new Exception("You clicked the forbidden button!");
		}
	}
}
