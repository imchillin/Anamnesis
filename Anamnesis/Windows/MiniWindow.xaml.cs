// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Anamnesis.GUI;
using Anamnesis.Services;
using Anamnesis.Utils;
using XivToolsWpf;

/// <summary>
/// Interaction logic for MiniWindow.xaml.
/// </summary>
public partial class MiniWindow : Window
{
	private readonly MainWindow main;
	private bool isHidden = false;
	private Point downPos;
	private bool mouseDown = false;
	private bool isDeactivating = false;

	public MiniWindow(MainWindow main)
	{
		this.main = main;
		this.InitializeComponent();

		this.main.Deactivated += this.Main_Deactivated;

		this.Left = SettingsService.Current.OverlayWindowPosition.X;
		this.Top = SettingsService.Current.OverlayWindowPosition.Y;

		_ = Task.Run(this.Ping);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnLocationChanged(null, null);
	}

	private async void Main_Deactivated(object? sender, System.EventArgs e)
	{
		this.isHidden = true;
		this.main.Hide();

		((Storyboard)this.Resources["AnimateCloseStoryboard"]).Begin(this);

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
		((Storyboard)this.Resources["AnimateOpenStoryboard"]).Begin(this);
		this.main.Show();

		if (this.main.WindowState == WindowState.Minimized)
			this.main.WindowState = WindowState.Normal;

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

		double centerX = this.Left + (this.Width / 2);
		double centerY = this.Top + (this.Width / 2);
		double iconRadius = 22;

		Point tl = new Point(centerX + iconRadius, centerY - iconRadius);
		Point tr = new Point(centerX + iconRadius + this.main.Width, centerY - iconRadius);
		Point bl = new Point(centerX + iconRadius, centerY - iconRadius + this.main.Height);

		if (!ScreenUtils.IsOnScreen(tr))
			tl.X = centerX - iconRadius - this.main.Width;

		if (!ScreenUtils.IsOnScreen(bl))
			tl.Y = (centerY + iconRadius) - this.main.Height;

		this.main.Left = tl.X;
		this.main.Top = tl.Y;
		SettingsService.Current.WindowPosition = tl;
	}

	private void OnClosing(object sender, CancelEventArgs e)
	{
		if (!this.main.IsClosing && SettingsService.Current.OverlayWindow)
		{
			e.Cancel = true;
		}
		else
		{
			this.main.Deactivated -= this.Main_Deactivated;
		}
	}

	private async Task Ping()
	{
		while (this.main != null && this.Dispatcher != null)
		{
			await Task.Delay(3000);
			await Dispatch.MainThread();

			if (!this.isHidden)
				continue;

			((Storyboard)this.Resources["AnimateIdleStoryboard"]).Begin(this);
		}
	}
}
