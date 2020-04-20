// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Windows
{
	using System;
	using System.Windows;
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for Dialog.xaml.
	/// </summary>
	public partial class Dialog : Window
	{
		public Dialog()
		{
			this.InitializeComponent();
		}

		private void OnTitleBarMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.DragMove();
			}
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			this.ActiveBorder.Visibility = Visibility.Visible;
			this.InActiveBorder.Visibility = Visibility.Collapsed;
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			this.ActiveBorder.Visibility = Visibility.Collapsed;
			this.InActiveBorder.Visibility = Visibility.Visible;
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
