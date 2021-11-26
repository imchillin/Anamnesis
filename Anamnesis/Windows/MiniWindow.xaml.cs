// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using Anamnesis.Services;
	using Anamnesis.Utils;
	using Serilog;

	/// <summary>
	/// Interaction logic for MiniWindow.xaml.
	/// </summary>
	public partial class MiniWindow : Window
	{
		private readonly Window main;
		private bool isHidden = false;
		private Point downPos;
		private bool mouseDown = false;
		private bool isDeactivating = false;

		public MiniWindow(Window main)
		{
			this.main = main;
			this.InitializeComponent();

			this.main.Deactivated += this.Main_Deactivated;

			this.Left = SettingsService.Current.OverlayWindowPosition.X;
			this.Top = SettingsService.Current.OverlayWindowPosition.Y;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnLocationChanged(null, null);
		}

		private async void Main_Deactivated(object? sender, System.EventArgs e)
		{
			this.isHidden = true;
			this.main.Hide();
			this.isDeactivating = true;
			await Task.Delay(200);
			this.isDeactivating = false;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (this.isDeactivating)
				return;

			this.mouseDown = true;

			this.downPos.X = this.Left;
			this.downPos.Y = this.Top;

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				this.DragMove();
			}
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!this.mouseDown)
				return;

			this.mouseDown = false;

			if (this.isDeactivating)
				return;

			if (!this.isHidden)
				return;

			Point upPos = default;
			upPos.X = this.Left;
			upPos.Y = this.Top;

			Vector delta = upPos - this.downPos;

			if (delta.Length > 100)
				return;

			this.OnLocationChanged(null, null);
			this.main.Show();
			this.isHidden = false;
		}

		private void OnLocationChanged(object? sender, System.EventArgs? e)
		{
			Point p = default;
			p.X = this.Left;
			p.Y = this.Top;
			SettingsService.Current.OverlayWindowPosition = p;

			if (double.IsNaN(this.main.Height) || double.IsNaN(this.main.Width))
				return;

			Point tl = new Point(this.Left + this.Width, this.Top);
			Point tr = new Point(this.Left + this.Width + this.main.Width, this.Top);
			Point bl = new Point(this.Left + this.Width, this.Top + this.main.Height);

			if (!ScreenUtils.IsOnScreen(tr))
				tl.X = this.Left - this.main.Width;

			if (!ScreenUtils.IsOnScreen(bl))
				tl.Y = (this.Top + this.Height) - this.main.Height;

			this.main.Left = tl.X;
			this.main.Top = tl.Y;
			SettingsService.Current.WindowPosition = tl;
		}

		private void OnClosing(object sender, CancelEventArgs e)
		{
			this.main.Deactivated -= this.Main_Deactivated;
		}
	}
}
