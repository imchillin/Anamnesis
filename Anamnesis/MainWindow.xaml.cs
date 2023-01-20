// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Anamnesis;
using Anamnesis.GUI.Dialogs;
using Anamnesis.GUI.Views;
using Anamnesis.Memory;
using Anamnesis.Actor;
using Anamnesis.Services;
using Anamnesis.Updater;
using Anamnesis.Utils;
using Anamnesis.Views;
using Anamnesis.Windows;
using PropertyChanged;
using XivToolsWpf;
using XivToolsWpf.Windows;
using XivToolsWpf.Extensions;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class MainWindow : ChromedWindow
{
	private static MainWindow? instance;
	private MiniWindow? mini;
	private bool hasSetPosition = false;
	private Tabs tab = Tabs.Home;

	public MainWindow()
	{
		instance = this;

		this.InitializeComponent();

		this.DataContext = this;

		ViewService.ShowingDrawer += this.OnShowDrawer;
		TargetService.ActorSelected += this.OnActorSelected;

		SettingsService.SettingsChanged += this.OnSettingsChanged;
		this.OnSettingsChanged();

		GameService.Instance.PropertyChanged += this.OnGameServicePropertyChanged;
	}

	public enum Tabs
	{
		Home,
		Settings,
		Developer,
		Actor,
	}

	public static new bool IsActive => instance?.GetIsActive() ?? false;

	public bool IsClosing { get; private set; } = false;
	public bool IsDrawerOpen { get; private set; } = false;

	public Tabs Tab
	{
		get => this.tab;
		set
		{
			if (value != Tabs.Actor)
				this.TargetService.ClearSelection();

			this.tab = value;
		}
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

	[DependsOn(nameof(Tab))]
	public bool ShowHome
	{
		get => this.Tab == Tabs.Home;
		set => this.Tab = Tabs.Home;
	}

	[DependsOn(nameof(Tab))]
	public bool ShowSettings
	{
		get => this.Tab == Tabs.Settings;
		set => this.Tab = Tabs.Settings;
	}

	[DependsOn(nameof(Tab))]
	public bool ShowActor
	{
		get => this.Tab == Tabs.Actor;
		set => this.Tab = Tabs.Actor;
	}

	[DependsOn(nameof(Tab))]
	public bool ShowDeveloper
	{
		get => this.Tab == Tabs.Developer;
		set => this.Tab = Tabs.Developer;
	}

	public static DpiScale GetDpi()
	{
		return VisualTreeHelper.GetDpi(instance);
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		this.Opacity = 1.0;
	}

	protected override void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);
		this.Opacity = SettingsService.Current.WindowOpcaticy;
	}

	private void OnSettingsChanged(object? sender = null, PropertyChangedEventArgs? args = null)
	{
		this.WindowScale.ScaleX = SettingsService.Current.Scale;
		this.WindowScale.ScaleY = SettingsService.Current.Scale;

		this.TitlebarButtonsScale.ScaleX = 1 / SettingsService.Current.Scale;
		this.TitlebarButtonsScale.ScaleY = 1 / SettingsService.Current.Scale;

		if (SettingsService.Current.EnableTranslucency != this.EnableTranslucency ||
			SettingsService.Current.ExtendIntoWindowChrome != this.ExtendIntoChrome)
		{
			this.EnableTranslucency = SettingsService.Current.EnableTranslucency;
			this.ExtendIntoChrome = SettingsService.Current.ExtendIntoWindowChrome;

			// relaod the window
			if (this.IsLoaded)
			{
				App.Current.MainWindow = new MainWindow();
				this.IsClosing = true; // Force the close
				this.Close();
				App.Current.MainWindow.Show();
				return;
			}
		}

		if (SettingsService.Current.WindowOpcaticy < 1)
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

	private void OnActorSelected(ActorMemory? actor)
	{
		if (actor != null)
		{
			this.Tab = Tabs.Actor;
			FrameworkElement? container = this.PinnedActorList.ItemContainerGenerator.ContainerFromItem(this.TargetService.CurrentlyPinned) as FrameworkElement;
			container?.BringIntoView();
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
				drawer.OnClosing += () => this.DrawerHost.IsLeftDrawerOpen = false;
				drawer.OnClosing += () => this.DrawerHost.IsTopDrawerOpen = false;
				drawer.OnClosing += () => this.DrawerHost.IsRightDrawerOpen = false;
				drawer.OnClosing += () => this.DrawerHost.IsBottomDrawerOpen = false;
			}

			this.IsDrawerOpen = true;

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

			this.IsDrawerOpen = false;

			GC.Collect();
		});
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

		scale = Math.Clamp(scale, 0.75, 2.0);
		this.WindowScale.ScaleX = scale;
		this.WindowScale.ScaleY = scale;
		SettingsService.Current.Scale = scale;
	}

	private void OnAddActorClicked(object sender, RoutedEventArgs e)
	{
		TargetSelectorView.Show((a) =>
		{
			TargetService.PinActor(a, true).Run();
		});
	}

	private void OnUnpinActorClicked(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement el && el.DataContext is PinnedActor actor)
		{
			TargetService.UnpinActor(actor);
		}
	}

	private void OnTargetActorClicked(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement el && el.DataContext is PinnedActor actor)
		{
			TargetService.SetPlayerTarget(actor);
		}
	}

	private async void OnDespawnActorClicked(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement el && el.DataContext is PinnedActor actor)
		{
			if (!actor.IsValid || actor.Memory == null)
				return;

			if(await Brio.Brio.Despawn(actor.Memory.ObjectIndex))
			{
				TargetService.UnpinActor(actor);
			}
		}
	}

	private void OnActorPinPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle)
		{
			this.OnUnpinActorClicked(sender, new RoutedEventArgs());
		}
	}

	private async void OnGameServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (!GameService.Instance.IsSignedIn)
		{
			await Dispatch.MainThread();
			this.Tab = Tabs.Home;
		}
	}

	private async void Window_Closing(object sender, CancelEventArgs e)
	{
		if (PoseService.Exists && PoseService.Instance.IsEnabled && !this.IsClosing)
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
			this.Tab = Tabs.Settings;
		}
	}

	private void OnFtueOkClicked(object sender, RoutedEventArgs e)
	{
		this.Ftue.Visibility = Visibility.Collapsed;
	}

	private void OnWikiClicked(object sender, RoutedEventArgs e)
	{
		UrlUtility.Open("https://github.com/imchillin/Anamnesis/wiki");
	}

	private void OnPinnedActorsPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		this.PinnedActorsList.ScrollToHorizontalOffset(this.PinnedActorsList.HorizontalOffset - (e.Delta / 5));
		e.Handled = true;
	}

	private void OnTitlebarMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed)
			return;

		this.DragMove();
	}
}