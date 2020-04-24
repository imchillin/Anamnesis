// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.GUI.Views;
	using MaterialDesignThemes.Wpf;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class MainWindow : Window
	{
		private UserControl currentView;
		private ViewService viewService;

		private Dictionary<Type, UserControl> pageCache = new Dictionary<Type, UserControl>();

		public MainWindow()
		{
			this.InitializeComponent();

			this.viewService = App.Services.Get<ViewService>();
			this.viewService.ShowingDrawer += this.OnShowDrawer;
			this.viewService.ShowingPage += this.OnShowPage;

			this.currentView = new HomeView();
			this.ViewArea.Content = this.currentView;
			this.IconArea.DataContext = this;

			this.Zodiark = App.Settings.ThemeDark;
			this.Opacity = App.Settings.Opacity;
			this.AlwaysOnTopToggle.IsChecked = App.Settings.AlwaysOnTop;
			this.WindowScale.ScaleX = App.Settings.Scale;
			this.WindowScale.ScaleY = App.Settings.Scale;
			App.Settings.Changed += this.OnSettingsChanged;
		}

		public bool Zodiark
		{
			get;
			set;
		}

		private void OnSettingsChanged(SettingsBase settings)
		{
			this.Zodiark = App.Settings.ThemeDark;
			this.WindowScale.ScaleX = App.Settings.Scale;
			this.WindowScale.ScaleY = App.Settings.Scale;
		}

		private void OnShowPage(ViewService.Page page)
		{
			this.currentView = page.Instance;
			this.ViewArea.Content = this.currentView;

			PaletteHelper ph = new PaletteHelper();

			this.MainAreaBackground.Visibility = page.DrawBackground ? Visibility.Visible : Visibility.Collapsed;
		}

		private async Task OnShowDrawer(string title, UserControl view, DrawerDirection direction)
		{
			// Close all current drawers.
			this.DrawerHost.IsLeftDrawerOpen = false;
			this.DrawerHost.IsTopDrawerOpen = false;
			this.DrawerHost.IsRightDrawerOpen = false;
			this.DrawerHost.IsBottomDrawerOpen = false;

			// If this is a drawer view, bind to its events.
			if (view is IDrawer drawer)
			{
				drawer.Close += () => this.DrawerHost.IsLeftDrawerOpen = false;
				drawer.Close += () => this.DrawerHost.IsTopDrawerOpen = false;
				drawer.Close += () => this.DrawerHost.IsRightDrawerOpen = false;
				drawer.Close += () => this.DrawerHost.IsBottomDrawerOpen = false;
			}

			switch (direction)
			{
				case DrawerDirection.Left:
				{
					this.DrawerLeft.Content = view;
					this.DrawerHost.IsLeftDrawerOpen = true;
					this.LeftTitle.Content = title;
					break;
				}

				case DrawerDirection.Top:
				{
					this.DrawerTop.Content = view;
					this.DrawerHost.IsTopDrawerOpen = true;
					break;
				}

				case DrawerDirection.Right:
				{
					this.DrawerRight.Content = view;
					this.DrawerHost.IsRightDrawerOpen = true;
					this.RightTitle.Content = title;
					break;
				}

				case DrawerDirection.Bottom:
				{
					this.DrawerBottom.Content = view;
					this.DrawerHost.IsBottomDrawerOpen = true;
					break;
				}
			}

			// Wait while any of the drawer areas remain open
			while (this.DrawerHost.IsLeftDrawerOpen
				|| this.DrawerHost.IsRightDrawerOpen
				|| this.DrawerHost.IsBottomDrawerOpen
				|| this.DrawerHost.IsTopDrawerOpen)
			{
				await Task.Delay(250);
			}
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

			// close any drawers that were left open
			this.DrawerHost.IsLeftDrawerOpen = false;
			this.DrawerHost.IsTopDrawerOpen = false;
			this.DrawerHost.IsRightDrawerOpen = false;
			this.DrawerHost.IsBottomDrawerOpen = false;
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OnMinimiseClick(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void OnSettingsClick(object sender, RoutedEventArgs e)
		{
			if (this.DrawerHost.IsRightDrawerOpen)
			{
				this.DrawerHost.IsRightDrawerOpen = false;
				return;
			}

			App.Services.Get<IViewService>().ShowDrawer<SettingsView>("Settings");
		}

		private void OnAlwaysOnTopChecked(object sender, RoutedEventArgs e)
		{
			this.Topmost = true;
			App.Settings.AlwaysOnTop = true;
		}

		private void OnAlwaysOnTopUnchecked(object sender, RoutedEventArgs e)
		{
			this.Topmost = false;
			App.Settings.AlwaysOnTop = false;
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
	}
}
