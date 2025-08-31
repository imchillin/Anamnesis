// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.GUI.Windows;
using Anamnesis.Memory.Exceptions;
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
			dlg.TitleText.Text = $"Anamnesis v{VersionInfo.ApplicationVersion}";
			XivToolsErrorDialog errorDialog = new XivToolsErrorDialog(dlg, ex, isCriticial);
			errorDialog.OnQuitRequested = HandleFatalErrorShutdown;

			try
			{
				if (SettingsService.Instance.IsInitialized && SettingsService.Instance.Settings != null)
					dlg.Topmost = SettingsService.Current.AlwaysOnTop;
			}
			catch (ServiceNotFoundException)
			{
				dlg.Topmost = true;
			}

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

		try
		{
			TargetService.Instance.ClearSelection();
		}
		catch (ServiceNotFoundException)
		{
			// Ignore if service is not instantiated yet
		}

		await App.Services.ShutdownServices();
		Application.Current?.Shutdown(2);
		return true; // Indicate that quit was handled to prevent forced shutdown
	}
}
