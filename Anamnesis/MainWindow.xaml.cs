// © Anamnesis.
// Developed by W and A Walsh.
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
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.GUI.Views;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;
	using Anamnesis.Updater;
	using Anamnesis.Utils;
	using Anamnesis.Views;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class MainWindow : Window
	{
		private bool hasSetPosition = false;

		public MainWindow()
		{
			this.InitializeComponent();

			this.DataContext = this;

			ViewService.ShowingDrawer += this.OnShowDrawer;

			SettingsService.SettingsChanged += this.OnSettingsChanged;
			this.OnSettingsChanged();

			GameService.Instance.PropertyChanged += this.OnGameServicePropertyChanged;
		}

		public GameService GameService => GameService.Instance;
		public SettingsService SettingsService => SettingsService.Instance;
		public GposeService GposeService => GposeService.Instance;
		public TargetService TargetService => TargetService.Instance;
		public MemoryService MemoryService => MemoryService.Instance;
		public LogService LogService => LogService.Instance;
		public UpdateService UpdateService => UpdateService.Instance;

#if DEBUG
		public bool IsDebug => true;
#else
		public bool IsDebug => false;
#endif

		private void OnSettingsChanged(object? sender = null, PropertyChangedEventArgs? args = null)
		{
			this.Opacity = SettingsService.Current.Opacity;
			this.WindowScale.ScaleX = SettingsService.Current.Scale;
			this.WindowScale.ScaleY = SettingsService.Current.Scale;

			if (SettingsService.Current.UseCustomBorder != this.AllowsTransparency)
			{
				if (this.IsLoaded)
				{
					App.Current.MainWindow = new MainWindow();
					this.Close();
					App.Current.MainWindow.Show();
					return;
				}

				if (SettingsService.Current.UseCustomBorder)
				{
					this.WindowStyle = WindowStyle.None;
					this.AllowsTransparency = true;
				}
				else
				{
					this.AllowsTransparency = false;
					this.WindowStyle = WindowStyle.ThreeDBorderWindow;
				}
			}

			if (!SettingsService.Current.UseCustomBorder || !SettingsService.Current.StayTransparent)
				this.Opacity = 1.0;

			this.ContentArea.Margin = new Thickness(SettingsService.Current.UseCustomBorder ? 10 : 0);

			if (!this.hasSetPosition && SettingsService.Current.WindowPosition.X != 0)
			{
				this.hasSetPosition = true;

				if (ScreenUtils.IsOnScreen(SettingsService.Current.WindowPosition))
				{
					this.Left = SettingsService.Current.WindowPosition.X;
					this.Top = SettingsService.Current.WindowPosition.Y;
				}
			}
		}

		private async Task OnShowDrawer(UserControl view, DrawerDirection direction)
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

				if (view is IDrawer drawer2)
				{
					drawer2.OnClosed();
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
			if (!SettingsService.Current.UseCustomBorder)
			{
				this.ActiveBorder.Visibility = Visibility.Collapsed;
				return;
			}

			this.ActiveBorder.Visibility = Visibility.Visible;
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			this.ActiveBorder.Visibility = Visibility.Collapsed;
		}

		private async void OnCloseClick(object sender, RoutedEventArgs e)
		{
			if (PoseService.Exists && PoseService.Instance.IsEnabled)
			{
				bool? result = await GenericDialog.Show(LocalizationService.GetString("Pose_WarningQuit"), LocalizationService.GetString("Common_Confirm"), MessageBoxButton.OKCancel);

				if (result != true)
				{
					return;
				}
			}

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

			ViewService.ShowDrawer<SettingsView>();
		}

		private void OnAboutClick(object sender, RoutedEventArgs e)
		{
			if (this.DrawerHost.IsRightDrawerOpen)
			{
				this.DrawerHost.IsRightDrawerOpen = false;
				return;
			}

			ViewService.ShowDrawer<AboutView>();
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

			this.Opacity = 1.0;
			////this.Animate(Window.OpacityProperty, 1.0, 100);
		}

		private void Window_MouseLeave(object sender, MouseEventArgs e)
		{
			if (SettingsService.Current.Opacity == 1.0)
				return;

			////this.Animate(Window.OpacityProperty, SettingsService.Current.Opacity, 250);
			this.Opacity = SettingsService.Current.Opacity;
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
			ViewService.ShowDrawer<TargetSelectorView>(DrawerDirection.Left);
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

		private void OnGameServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (!GameService.Instance.IsSignedIn)
			{
				this.Dispatcher.Invoke(() => this.Tabs.SelectedIndex = 0);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SettingsService.SettingsChanged -= this.OnSettingsChanged;
			ViewService.ShowingDrawer -= this.OnShowDrawer;

			SettingsService.Current.WindowPosition = new Point(this.Left, this.Top);
			SettingsService.Save();
		}

		private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter == null)
				return;

			string? url = e.Parameter.ToString();

			if (url == null)
				return;

			UrlUtility.Open(url);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (SettingsService.Instance.FirstTimeUser)
			{
				this.Ftue.Visibility = Visibility.Visible;
				ViewService.ShowDrawer<SettingsView>();
			}
		}

		private void OnFtueOkClicked(object sender, RoutedEventArgs e)
		{
			this.Ftue.Visibility = Visibility.Collapsed;
		}
	}
}
