// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Panels;

using System.Threading.Tasks;
using System.Windows;

public static class GenericDialog
{
	public static void ShowLocalized(string messageKey, string captionKey)
	{
		GenericDialogPanel.ShowLocalized(messageKey, captionKey);
	}

	public static Task<bool?> ShowLocalizedAsync(string messageKey, string captionKey, MessageBoxButton buttons = MessageBoxButton.OK)
	{
		return GenericDialogPanel.ShowLocalizedAsync(messageKey, captionKey, buttons);
	}

	public static void Show(string message, string caption)
	{
		GenericDialogPanel.Show(message, caption);
	}

	public static Task<bool?> ShowAsync(string message, string title, MessageBoxButton buttons)
	{
		return GenericDialogPanel.ShowAsync(message, title, buttons);
	}
}