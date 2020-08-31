// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Dialogs
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Services;

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

		public bool? Result { get; private set; }
		public string Message { get; set; } = string.Empty;

		public string? Left { get; set; }
		public string? Right { get; set; }

		public static async Task<bool?> Show(string message, string caption, MessageBoxButton buttons)
		{
			bool? result = null;

			await Application.Current.Dispatcher.InvokeAsync(async () =>
			{
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

				result = await ViewService.ShowDialog<GenericDialog, bool?>(caption, dlg);
			});

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
}
