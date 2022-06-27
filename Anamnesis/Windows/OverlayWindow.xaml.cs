// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Services;
using FontAwesome.Sharp;
using MaterialDesignThemes.Wpf;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using XivToolsWpf;

using MediaColor = System.Windows.Media.Color;

[AddINotifyPropertyChangedInterface]
public partial class OverlayWindow : Window, IPanelGroupHost
{
	private readonly WindowInteropHelper windowInteropHelper;
	private readonly List<OverlayWindow> children = new();
	private OverlayWindow? parent;

	private string? titleKey;
	private string? titleText;

	private int x;
	private int y;
	private IconChar icon;
	private bool canResize = true;
	private bool autoClose = true;

	public OverlayWindow()
	{
		this.windowInteropHelper = new(this);
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.WindowContextMenu.DataContext = this;
		base.Topmost = false;

		this.TitleColor = Application.Current.Resources.GetTheme().ToolForeground;
	}

	public ContentPresenter PanelGroupArea => this.ContentPresenter;
	public bool ShowBackground { get; set; } = true;

	[AlsoNotifyFor(nameof(AutoClose))]
	public bool AllowAutoClose { get; set; } = true;

	[DependsOn(nameof(AllowAutoClose))]
	public bool AutoClose
	{
		get
		{
			if (!this.AllowAutoClose)
				return false;

			return this.autoClose;
		}

		set => this.autoClose = value;
	}

	public string? TitleKey
	{
		get => this.titleKey;
		set
		{
			this.titleKey = value;
			this.UpdateTitle();
		}
	}

	public new string? Title
	{
		get => this.titleText;
		set
		{
			this.titleText = value;
			this.UpdateTitle();
		}
	}

	public new IconChar Icon
	{
		get => this.icon;
		set
		{
			this.icon = value;
			this.TitleIcon.Icon = value;
		}
	}

	public new bool Topmost
	{
		get => base.Topmost;
		set
		{
			base.Topmost = value;
			this.UpdatePosition();
		}
	}

	public bool CanResize
	{
		get => this.canResize;
		set
		{
			this.canResize = value;
			this.ResizeMode = this.canResize ? ResizeMode.CanResize : ResizeMode.NoResize;
		}
	}

	public Rect Rect
	{
		get
		{
			if (MemoryService.Process != null)
			{
				GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect gameRect);
				GetWindowRect(this.windowInteropHelper.Handle, out Win32Rect selfRect);

				// TODO: get this from a windows api maybe?
				int titleBarHeight = 22;

				this.x = (int)(selfRect.Left - gameRect.Left);
				this.y = (int)(selfRect.Top - (gameRect.Top + titleBarHeight));
			}

			return new Rect(this.x, this.y, this.Width, this.Height);
		}
		set
		{
			this.x = (int)value.X;
			this.y = (int)value.Y;
			this.Width = value.Width;
			this.Height = value.Height;
			this.UpdatePosition();
		}
	}

	public IPanelGroupHost Host => this;
	public MediaColor? TitleColor { get; set; }

	public new void Show()
	{
		double screenHeight = 720;
		this.MaxHeight = screenHeight - this.x;

		base.Show();
	}

	public void AddChild(IPanel panel)
	{
		if (panel.Host is OverlayWindow wnd)
		{
			wnd.parent = this;
			this.children.Add(wnd);

			return;
		}

		throw new NotSupportedException("Panel host must be an overlay window to be a child of another overlay window");
	}

	public new void Close()
	{
		// base.Close();
		Storyboard? sb = this.Resources["CloseStoryboard"] as Storyboard;
		sb?.Begin();
	}

	protected override async void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);

		if (this.AutoClose && this.AllowAutoClose)
		{
			// If we have docked panels that are active,
			// then we dont close yet.
			foreach (IPanel docked in this.children)
			{
				if (docked.Host is OverlayWindow dockedWindow)
				{
					if (dockedWindow.IsVisible)
					{
						return;
					}
				}
			}

			this.Close();

			if (this.parent != null)
			{
				this.parent.children.Remove(this);

				// wait a moment to see if the parent is actually being focused
				await Task.Delay(250);
				await Dispatch.MainThread();

				if (!this.parent.IsActive)
				{
					this.parent.OnDeactivated(e);
				}
			}
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hwnd, out Win32Rect rect);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	private void UpdateTitle()
	{
		StringBuilder sb = new();

		if (this.titleKey != null)
			sb.Append(LocalizationService.GetString(this.titleKey, true));

		if (this.titleKey != null && this.titleText != null)
			sb.Append(" ");

		if (this.titleText != null)
			sb.Append(this.titleText);

		base.Title = sb.ToString();
		this.TitleText.Text = base.Title;
	}

	private void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		if (MemoryService.Process == null)
		{
			this.Close();
			return;
		}

		GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect rect);

		this.Left = rect.Left + 20;
		this.Top = rect.Top + 20;

		SetParent(this.windowInteropHelper.Handle, MemoryService.Process.MainWindowHandle);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;
		const int GWL_STYLE = -16;

		int style = GetWindowLong(this.windowInteropHelper.Handle, GWL_STYLE);
		style = (int)((style & ~WS_POPUP) | WS_CHILD);
		SetWindowLong(this.windowInteropHelper.Handle, GWL_STYLE, style);

		this.UpdatePosition();
		this.Activate();

		Storyboard? sb = this.Resources["OpenStoryboard"] as Storyboard;
		sb?.Begin();
	}

	private void UpdatePosition()
	{
		if (!this.IsActive)
			return;

		if (MemoryService.Process == null)
			return;

		int w = (int)this.Width - 1;
		int h = (int)this.Height - 1;

		GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect rect);
		int gameWindowWidth = rect.Right - rect.Left;
		int hameWindowHeight = rect.Bottom - rect.Top;

		this.y = Math.Clamp(this.y, 0, hameWindowHeight);
		this.x = Math.Clamp(this.x, 0, gameWindowWidth);

		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, 0, 0, w, h, /*SHOWWINDOW */ 0x0040);
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, this.x, this.y, 0, 0, /*NOSIZE*/ 0x0001);

		if (this.Topmost)
		{
			SetWindowPos(this.windowInteropHelper.Handle, (IntPtr)(-1), 0, 0, 0, 0, /*NOSIZE | NOMOVE*/ 0x0001 | 0x0003);
		}
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnTitlebarContextButtonClicked(object sender, RoutedEventArgs e)
	{
		this.WindowContextMenu.IsOpen = true;
	}

	private void OnTitlebarCloseButtonClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (this.Opacity == 1.0)
			return;

		this.Animate(Window.OpacityProperty, 1.0, 100);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (SettingsService.Current.Opacity != 1.0)
		{
			this.Animate(Window.OpacityProperty, SettingsService.Current.Opacity, 250);
		}
	}

	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.Activate();
		////this.UpdatePosition();
	}

	private void OnCloseStoryboardCompleted(object sender, EventArgs e)
	{
		base.Close();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}
}
