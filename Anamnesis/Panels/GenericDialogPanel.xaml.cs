// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Navigation;
using Anamnesis.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public partial class GenericDialogPanel : PanelBase
{
	public GenericDialogPanel(IPanelGroupHost host, DialogInfo info)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.Title = info.Title;
		this.Message = info.Message;

		switch (info.Buttons)
		{
			case MessageBoxButton.OK:
			{
				this.Left = null;
				this.Right = "OK";
				break;
			}

			case MessageBoxButton.OKCancel:
			{
				this.Left = "Cancel";
				this.Right = "OK";
				break;
			}

			case MessageBoxButton.YesNoCancel: throw new NotImplementedException();

			case MessageBoxButton.YesNo:
			{
				this.Left = "No";
				this.Right = "Yes";
				break;
			}
		}
	}

	public bool? Result { get; set; }
	public string Message { get; set; } = string.Empty;

	public string? Left { get; set; }
	public string? Right { get; set; }

	public static void ShowLocalized(string messageKey, string captionKey)
	{
		Task.Run(() => ShowLocalizedAsync(messageKey, captionKey, MessageBoxButton.OK));
	}

	public static Task<bool?> ShowLocalizedAsync(string messageKey, string captionKey, MessageBoxButton buttons = MessageBoxButton.OK)
	{
		string message = LocalizationService.GetString(messageKey, true);
		string caption = LocalizationService.GetString(captionKey, true);

		return ShowAsync(message, caption, buttons);
	}

	public static void Show(string message, string caption)
	{
		Task.Run(() => ShowAsync(message, caption, MessageBoxButton.OK));
	}

	public static async Task<bool?> ShowAsync(string message, string title, MessageBoxButton buttons)
	{
		await Dispatch.MainThread();

		DialogInfo info = new(title, message, buttons);
		GenericDialogPanel? panel = NavigationService.Navigate(new("GenericDialog", info)) as GenericDialogPanel;

		if (panel == null)
			throw new Exception("Failed to open generic dialog");

		await panel.WhileOpen();

		return panel.Result;
	}

	public void Cancel()
	{
		this.Result = null;
		this.Close();
	}

	private void OnLeftClick(object sender, RoutedEventArgs e)
	{
		this.Result = false;
		this.Close();
	}

	private void OnRightClick(object sender, RoutedEventArgs e)
	{
		this.Result = true;
		this.Close();
	}

	public class DialogInfo
	{
		public DialogInfo(string title, string message, MessageBoxButton buttons)
		{
			this.Title = title;
			this.Message = message;
			this.Buttons = buttons;
		}

		public string Title { get; init; }
		public string Message { get; init; }
		public MessageBoxButton Buttons { get; init; }
	}
}
