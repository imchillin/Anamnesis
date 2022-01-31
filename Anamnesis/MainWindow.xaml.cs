// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI
{
	using System;
	using System.ComponentModel;
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
	using Anamnesis.Windows;
	using PropertyChanged;
	using XivToolsWpf.Windows;

	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class MainWindow : ChromedWindow
	{
		private MiniWindow? mini;
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

		public bool IsClosing { get; private set; } = false;

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

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			if (SettingsService.Current.Opacity == 1.0)
			{
				this.Opacity = 1.0;
				return;
			}

			this.Opacity = 1.0;
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);

			if (SettingsService.Current.Opacity == 1.0)
				return;

			this.Opacity = SettingsService.Current.Opacity;
		}

		private void OnSettingsChanged(object? sender = null, PropertyChangedEventArgs? args = null)
		{
			this.WindowScale.ScaleX = SettingsService.Current.Scale;
			this.WindowScale.ScaleY = SettingsService.Current.Scale;

			if (SettingsService.Current.EnableTranslucency != this.EnableTranslucency ||
				SettingsService.Current.ExtendIntoWindowChrome != this.ExtendIntoChrome)
			{
				this.EnableTranslucency = SettingsService.Current.EnableTranslucency;
				this.ExtendIntoChrome = SettingsService.Current.ExtendIntoWindowChrome;

				// relaod the window
				if (this.IsLoaded)
				{
					App.Current.MainWindow = new MainWindow();
					this.Close();
					App.Current.MainWindow.Show();
					return;
				}
			}

			if (SettingsService.Current.Opacity < 1)
				this.TransprentWhenNotInFocus = true;

			if (!this.hasSetPosition && SettingsService.Current.WindowPosition.X != 0)
			{
				this.hasSetPosition = true;

				if (ScreenUtils.IsOnScreen(SettingsService.Current.WindowPosition))
				{
					this.Left = SettingsService.Current.WindowPosition.X;
					this.Top = SettingsService.Current.WindowPosition.Y;
				}
			}

			if (this.mini != null && !SettingsService.Current.OverlayWindow)
			{
				this.mini.Close();
				this.mini = null;
			}
			else if (this.mini == null && SettingsService.Current.OverlayWindow)
			{
				this.mini = new MiniWindow(this);
				this.mini.Show();
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

		private async void OnSettingsClick(object sender, RoutedEventArgs e)
		{
			if (this.DrawerHost.IsRightDrawerOpen)
			{
				this.DrawerHost.IsRightDrawerOpen = false;
				return;
			}

			if (PoseService.Exists && PoseService.Instance.IsEnabled)
			{
				bool? result = await GenericDialog.ShowLocalizedAsync("Pose_WarningQuit", "Common_Confirm", MessageBoxButton.OKCancel);

				if (result != true)
				{
					return;
				}

				PoseService.Instance.IsEnabled = false;
			}

			await ViewService.ShowDrawer<SettingsView>();
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
			if (sender is FrameworkElement el && el.DataContext is TargetService.PinnedActor actor)
			{
				TargetService.UnpinActor(actor);
			}
		}

		private void OnTargetActorClicked(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement el && el.DataContext is TargetService.PinnedActor actor)
			{
				TargetService.SetPlayerTarget(actor);
			}
		}

		private void OnActorPinPreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
				this.OnUnpinActorClicked(sender, new RoutedEventArgs());
			}
		}

		private void OnGameServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (!GameService.Instance.IsSignedIn)
			{
				this.Dispatcher.Invoke(() => this.Tabs.SelectedIndex = 0);
			}
		}

		private async void Window_Closing(object sender, CancelEventArgs e)
		{
			if (PoseService.Exists && PoseService.Instance.IsEnabled)
			{
				bool? result = await GenericDialog.ShowAsync(LocalizationService.GetString("Pose_WarningQuit"), LocalizationService.GetString("Common_Confirm"), MessageBoxButton.OKCancel);

				if (result != true)
				{
					e.Cancel = true;
					return;
				}
			}

			this.IsClosing = true;
			this.mini?.Close();

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
			this.mini?.Show();

			if (SettingsService.Instance.FirstTimeUser)
			{
				this.Ftue.Visibility = Visibility.Visible;
				ViewService.ShowDrawer<SettingsView>();
			}
		}

		private void OnFtueOkClicked(object sender, RoutedEventArgs e)
		{
			this.Ftue.Visibility = Visibility.Collapsed;
			this.DrawerHost.IsRightDrawerOpen = false;
		}

		private void OnWikiClicked(object sender, RoutedEventArgs e)
		{
			UrlUtility.Open("https://github.com/imchillin/Anamnesis/wiki");
		}
	}
}