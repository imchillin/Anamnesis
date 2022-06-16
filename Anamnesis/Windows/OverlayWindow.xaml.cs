// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Services;
using FontAwesome.Sharp;
using PropertyChanged;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

[AddINotifyPropertyChangedInterface]
public partial class OverlayWindow : Window, IPanelGroupHost
{
	private readonly WindowInteropHelper windowInteropHelper;

	public OverlayWindow()
	{
		this.windowInteropHelper = new(this);
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ContentPresenter PanelGroupArea => this.ContentPresenter;

	public new IconChar Icon { get; set; }
	public new string Title
	{
		get => base.Title;
		set
		{
			base.Title = value;
			this.TitleText.Text = value;
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	private void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		if (MemoryService.Process == null)
		{
			this.Close();
			return;
		}

		GetWindowRect(MemoryService.Process.MainWindowHandle, out Rect rect);

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
	}

	private void UpdatePosition()
	{
		if (!this.IsActive)
			return;

		int w = (int)this.Width - 1;
		int h = (int)this.Height - 1;

		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, 0, 0, w, h, /*SHOWWINDOW */ 0x0040);
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, 0, 0, 0, 0, /*NOSIZE*/ 0x0001);
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
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

	[StructLayout(LayoutKind.Sequential)]
	public struct Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}
}
