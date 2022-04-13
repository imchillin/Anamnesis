// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows
{
	using System;
	using System.Runtime.ExceptionServices;
	using System.Windows;
	using Anamnesis.GUI.Windows;
	using Anamnesis.Services;
	using Serilog;
	using XivToolsWpf;

	using static XivToolsWpf.Dialogs.ErrorDialog;

	using XivToolsErrorDialog = XivToolsWpf.Dialogs.ErrorDialog;

	public static class ErrorDialog
    {
		public static async void ShowError(ExceptionDispatchInfo ex, bool isCriticial)
		{
			if (Application.Current == null)
				return;

			if (ex.SourceException is ErrorException || ex.SourceException?.InnerException is ErrorException)
				return;

			await Dispatch.MainThread();

			try
			{
				SplashWindow.HideWindow();

				Dialog dlg = new Dialog();
				dlg.TitleText.Text = "Anamnesis v" + VersionInfo.Date.ToString("yyyy-MM-dd HH:mm");
				XivToolsErrorDialog errorDialog = new XivToolsErrorDialog(dlg, ex, isCriticial);

				if (SettingsService.Exists && SettingsService.Instance.Settings != null)
					dlg.Topmost = SettingsService.Current.AlwaysOnTop;

				dlg.ContentArea.Content = errorDialog;
				dlg.ShowDialog();

				if (Application.Current == null)
					return;

				if (isCriticial)
					Application.Current.Shutdown(2);

				SplashWindow.ShowWindow();
			}
			catch (Exception newEx)
			{
				Log.Error(new ErrorException(newEx), "Failed to display error dialog");
			}
		}
	}
}
