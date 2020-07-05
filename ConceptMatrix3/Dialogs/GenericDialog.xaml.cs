// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Dialogs
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix;

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

		public event DialogEvent Close;

		public bool? Result { get; private set; }
		public string Message { get; set; }

		public string Left { get; set; }
		public string Right { get; set; }

		public static async Task<bool?> Show(string message, string caption, MessageBoxButton buttons)
		{
			IViewService viewService = Services.Get<IViewService>();
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

			return await viewService.ShowDialog<GenericDialog, bool?>(caption, dlg);
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
