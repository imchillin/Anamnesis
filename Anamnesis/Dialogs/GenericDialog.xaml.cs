// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Dialogs
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;
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

		public static void ShowLocalized(string messageKey, string captionKey)
		{
			Task.Run(() => ShowLocalizedAsync(messageKey, captionKey, MessageBoxButton.OK));
		}

		public static Task<bool?> ShowLocalizedAsync(string messageKey, string captionKey, MessageBoxButton buttons)
		{
			string message = LocalizationService.GetString(messageKey, true);
			string caption = LocalizationService.GetString(captionKey, true);

			return ShowAsync(message, caption, buttons);
		}

		public static void Show(string message, string caption)
		{
			Task.Run(() => ShowAsync(message, caption, MessageBoxButton.OK));
		}

		public static async Task<bool?> ShowAsync(string message, string caption, MessageBoxButton buttons)
		{
			await Dispatch.MainThread();

			GenericDialog dlg = new GenericDialog();
			dlg.Message = message;

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

			return await ViewService.ShowDialog<GenericDialog, bool?>(caption, dlg);
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
}
