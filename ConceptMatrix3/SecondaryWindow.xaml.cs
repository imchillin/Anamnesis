// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI
{
	using System;
	using System.Windows;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for SecondaryWindow.xaml.
	/// </summary>
	public partial class SecondaryWindow : Window
	{
		public SecondaryWindow()
		{
			this.InitializeComponent();

			this.Opacity = App.Settings.Opacity;
			this.WindowScale.ScaleX = App.Settings.Scale;
			this.WindowScale.ScaleY = App.Settings.Scale;
			App.Settings.Changed += this.OnSettingsChanged;
		}

		private void OnResizeDrag(object sender, DragDeltaEventArgs e)
		{
			double scale = this.WindowScale.ScaleX;

			double delta = Math.Max(e.HorizontalChange / 1024, e.VerticalChange / 576);
			scale += delta;

			scale = Math.Clamp(scale, 0.5, 2.0);
			this.WindowScale.ScaleX = scale;
			this.WindowScale.ScaleY = scale;
			App.Settings.Scale = scale;
		}

		private void Window_MouseEnter(object sender, MouseEventArgs e)
		{
			if (App.Settings.Opacity == 1.0)
			{
				this.Opacity = 1.0;
				return;
			}

			this.Animate(Window.OpacityProperty, 1.0, 100);
		}

		private void Window_MouseLeave(object sender, MouseEventArgs e)
		{
			if (App.Settings.Opacity == 1.0)
				return;

			this.Animate(Window.OpacityProperty, App.Settings.Opacity, 250);
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OnMinimiseClick(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
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

		private void OnSettingsChanged(SettingsBase settings)
		{
			this.WindowScale.ScaleX = App.Settings.Scale;
			this.WindowScale.ScaleY = App.Settings.Scale;
		}
	}
}
