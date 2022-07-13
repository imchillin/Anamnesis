// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using System;
using System.Runtime.ExceptionServices;
using System.Windows;
using Anamnesis.Panels;
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

		SplashWindow.HideWindow();

		try
		{
			await ExceptionPanel.Show(ex, isCriticial);
		}
		catch(Exception ex2)
		{
			Log.Error(new ErrorException(ex2), "Failed to display exception panel");

			MessageBox.Show("An error was encountered when attempting to display the previous error", "Anamnesis Internal Error");
			Application.Current.Shutdown(2);
		}

		if (Application.Current == null)
			return;

		if (isCriticial)
			Application.Current.Shutdown(2);

		SplashWindow.ShowWindow();
	}
}
