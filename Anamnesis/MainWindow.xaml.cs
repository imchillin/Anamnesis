// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using Anamnesis;
	using Anamnesis.GUI.Views;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Views;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();

			this.DataContext = this;

			ViewService.ShowingDrawer += this.OnShowDrawer;

			this.Zodiark = SettingsService.Current.ThemeDark;
			this.Opacity = SettingsService.Current.Opacity;
			this.AlwaysOnTopToggle.IsChecked = SettingsService.Current.AlwaysOnTop;
			this.WindowScale.ScaleX = SettingsService.Current.Scale;
			this.WindowScale.ScaleY = SettingsService.Current.Scale;

			if (SettingsService.Current.WindowPosition.X != 0)
			{
				this.Left = SettingsService.Current.WindowPosition.X;
				this.Top = SettingsService.Current.WindowPosition.Y;
			}

			SettingsService.Current.PropertyChanged += this.OnSettingsChanged;
		}

		public SettingsService SettingsService => SettingsService.Instance;
		public GposeService GposeService => GposeService.Instance;
		public TargetService TargetService => TargetService.Instance;

		public bool Zodiark { get; set; }

		private void OnSettingsChanged(object sender, PropertyChangedEventArgs args)
		{
			this.Zodiark = SettingsService.Current.ThemeDark;
			this.WindowScale.ScaleX = SettingsService.Current.Scale;
			this.WindowScale.ScaleY = SettingsService.Current.Scale;
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

			ViewService.ShowDrawer<SettingsView>(LocalizationService.GetString("Settings_Header"));
		}

		private void OnAboutClick(object sender, RoutedEventArgs e)
		{
			if (this.DrawerHost.IsRightDrawerOpen)
			{
				this.DrawerHost.IsRightDrawerOpen = false;
				return;
			}

			ViewService.ShowDrawer<AboutView>(LocalizationService.GetString("About_Header"));
		}

		private void Window_MouseEnter(object sender, MouseEventArgs e)
		{
			if (SettingsService.Current.Opacity == 1.0)
			{
				this.Opacity = 1.0;
				return;
			}

			if (SettingsService.Current.StayTransparent)
				return;

			this.Animate(Window.OpacityProperty, 1.0, 100);
		}

		private void Window_MouseLeave(object sender, MouseEventArgs e)
		{
			if (SettingsService.Current.Opacity == 1.0)
				return;

			this.Animate(Window.OpacityProperty, SettingsService.Current.Opacity, 250);
		}

		private void OnResizeDrag(object sender, DragDeltaEventArgs e)
		{
			double scale = this.WindowScale.ScaleX;

			double delta = Math.Max(e.HorizontalChange / 1024, e.VerticalChange / 576);
			scale += delta;

			scale = Math.Clamp(scale, 0.5, 2.0);
			this.WindowScale.ScaleX = scale;
			this.WindowScale.ScaleY = scale;
			SettingsService.Current.Scale = scale;
		}

		private void OnAddActorClicked(object sender, RoutedEventArgs e)
		{
			ViewService.ShowDrawer<TargetSelectorView>(null, DrawerDirection.Left);
		}

		private void OnUnpinActorClicked(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement el && el.DataContext is TargetService.ActorTableActor actor)
			{
				TargetService.UnpinActor(actor);
			}
		}

		private void OnActorPinPreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
				this.OnUnpinActorClicked(sender, new RoutedEventArgs());
			}
		}

		private void OnConvertActorClicked(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement el && el.DataContext is TargetService.ActorTableActor actor)
			{
				ActorViewModel? vm = actor.GetViewModel();

				if (vm == null)
					return;

				Task.Run(vm.ConvertToPlayer);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SettingsService.Current.WindowPosition = new Point(this.Left, this.Top);
			SettingsService.Save();
		}
	}
}
