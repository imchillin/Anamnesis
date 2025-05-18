// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.GUI.Windows;
using Anamnesis.Services;
using Serilog;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
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
			errorDialog.OnQuitRequested = HandleFatalErrorShutdown;

			if (SettingsService.Instance.IsInitialized && SettingsService.Instance.Settings != null)
				dlg.Topmost = SettingsService.Current.AlwaysOnTop;

			dlg.ContentArea.Content = errorDialog;
			dlg.ShowDialog();

			// If the user closed the error dialog without clicking the quit button, call the handler explicitly
			if (isCriticial)
				await HandleFatalErrorShutdown();

			SplashWindow.ShowWindow();
		}
		catch (Exception newEx)
		{
			Log.Error(new ErrorException(newEx), "Failed to display error dialog");
		}
	}

	private static async Task<bool> HandleFatalErrorShutdown()
	{
		if (Application.Current == null)
			return false;

		Log.Verbose($"Shutting down app on critical error");
		TargetService.Instance.ClearSelection();
		await App.Services.ShutdownServices();
		Application.Current?.Shutdown(2);
		return true; // Indicate that quit was handled to prevent forced shutdown
	}
}
