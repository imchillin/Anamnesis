// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using Anamnesis;
	using Anamnesis.GUI.Services;
	using Anamnesis.GUI.Views;
	using Anamnesis.Memory;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class MainWindow : Window
	{
		private readonly ViewService viewService;

		private UserControl? currentView;
		private Dictionary<ViewService.Page, UserControl> pages = new Dictionary<ViewService.Page, UserControl>();

		public MainWindow()
		{
			this.InitializeComponent();

			this.DataContext = this;

			this.viewService = App.Services.Get<ViewService>();
			this.viewService.ShowingDrawer += this.OnShowDrawer;

			TargetService.ActorSelected += this.OnActorSelected;

			////this.IconArea.DataContext = this;

			this.HasSelection = false;
			this.Zodiark = App.Settings.ThemeDark;
			this.Opacity = App.Settings.Opacity;
			this.AlwaysOnTopToggle.IsChecked = App.Settings.AlwaysOnTop;
			this.WindowScale.ScaleX = App.Settings.Scale;
			this.WindowScale.ScaleY = App.Settings.Scale;
			App.Settings.Changed += this.OnSettingsChanged;
		}

		public bool Zodiark { get; set; }
		public bool HasSelection { get; set; }

		private void OnSettingsChanged(SettingsBase settings)
		{
			this.Zodiark = App.Settings.ThemeDark;
			this.WindowScale.ScaleX = App.Settings.Scale;
			this.WindowScale.ScaleY = App.Settings.Scale;
		}

		private async Task OnShowDrawer(string? title, UserControl view, DrawerDirection direction)
		{
			await Application.Current.Dispatcher.InvokeAsync(async () =>
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

				GC.Collect();
			});
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
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			this.ActiveBorder.Visibility = Visibility.Collapsed;
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

		private void OnAboutClick(object sender, RoutedEventArgs e)
		{
			if (this.DrawerHost.IsRightDrawerOpen)
			{
				this.DrawerHost.IsRightDrawerOpen = false;
				return;
			}

			App.Services.Get<IViewService>().ShowDrawer<AboutView>("About");
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

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			await Task.Delay(50);
			this.OnActorSelected(TargetService.SelectedActor);
		}

		private void Window_MouseEnter(object sender, MouseEventArgs e)
		{
			if (App.Settings.Opacity == 1.0)
			{
				this.Opacity = 1.0;
				return;
			}

			if (App.Settings.StayTransparent)
				return;

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

		private void OnActorSelected(ActorViewModel? actor)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.ActorArea.DataContext = actor;
				this.ActorName.Text = actor?.Name;
				this.HasSelection = actor != null;
			});
		}

		private void OnSelectPage(ViewService.Page page)
		{
			if (page == null)
			{
				this.currentView = null;
				this.ViewArea.Content = null;
				return;
			}

			if (!this.pages.ContainsKey(page))
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					this.pages.Add(page, page.Create());
				});
			}

			this.currentView = this.pages[page];
			this.ViewArea.Content = this.currentView;
		}
	}
}
