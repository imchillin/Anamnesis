// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Dialogs;

using Anamnesis.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf;

/// <summary>
/// Interaction logic for GenericDialog.xaml.
/// </summary>
public partial class GenericDialog : UserControl, IDialog<bool?>
{
	public GenericDialog()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event DialogEvent? Close;

	public bool? Result { get; set; }
	public string Message { get; set; } = string.Empty;

	public string? Left { get; set; }
	public string? Right { get; set; }
	public string? CheckboxLabel { get; set; }
	public bool IsCheckboxChecked { get; set; }

	public static void ShowLocalized(string messageKey, string captionKey)
	{
		Task.Run(() => ShowLocalizedAsync(messageKey, captionKey, MessageBoxButton.OK));
	}

	public static Task<bool?> ShowLocalizedAsync(string messageKey, string captionKey, MessageBoxButton buttons = MessageBoxButton.OK, string? checkboxLabelKey = null, Action? onCheckboxConfirmed = null)
	{
		string message = LocalizationService.GetString(messageKey, true);
		string caption = LocalizationService.GetString(captionKey, true);
		string? checkboxLabel = checkboxLabelKey != null ? LocalizationService.GetString(checkboxLabelKey, true) : null;

		return ShowAsync(message, caption, buttons, checkboxLabel, onCheckboxConfirmed);
	}

	public static void Show(string message, string caption)
	{
		Task.Run(() => ShowAsync(message, caption, MessageBoxButton.OK));
	}

	public static async Task<bool?> ShowAsync(string message, string caption, MessageBoxButton buttons, string? checkboxLabel = null, Action? onCheckboxConfirmed = null)
	{
		await Dispatch.MainThread();

		var dlg = new GenericDialog
		{
			Message = message,
			CheckboxLabel = checkboxLabel,
		};

		switch (buttons)
		{
			case MessageBoxButton.OK:
				{
					dlg.Left = null;
					dlg.Right = "OK";
					break;
				}

			case MessageBoxButton.OKCancel:
				{
					dlg.Left = "Cancel";
					dlg.Right = "OK";
					break;
				}

			case MessageBoxButton.YesNoCancel:
				throw new NotImplementedException();

			case MessageBoxButton.YesNo:
				{
					dlg.Left = "No";
					dlg.Right = "Yes";
					break;
				}
		}

		bool? result = await ViewService.ShowDialog<GenericDialog, bool?>(caption, dlg);
		if (result == true && dlg.IsCheckboxChecked)
			onCheckboxConfirmed?.Invoke();

		return result;
	}

	public void Cancel()
	{
		this.Result = null;
		this.Close?.Invoke();
	}

	private void OnLeftClick(object sender, RoutedEventArgs e)
	{
		this.Result = false;
		this.Close?.Invoke();
	}

	private void OnRightClick(object sender, RoutedEventArgs e)
	{
		this.Result = true;
		this.Close?.Invoke();
	}
}
