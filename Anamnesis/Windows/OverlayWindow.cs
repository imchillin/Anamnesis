// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

public class OverlayWindow : FloatingWindow
{
	private readonly WindowInteropHelper windowInteropHelper;
	private int x = 0;
	private int y = 0;

	public OverlayWindow()
	{
		this.windowInteropHelper = new(this);
	}

	public override Rect Rect
	{
		get => base.Rect;
		set
		{
			this.Width = value.Width;
			this.Height = value.Height;

			if (MemoryService.Process != null)
			{
				GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect gameWindowRect);
				this.x = (int)value.Left - gameWindowRect.Left;
				this.y = (int)value.Top - gameWindowRect.Top;

				// Size of the window chrome.
				// No idea how to get this correctly...
				this.y -= 26;
				this.x -= 2;
			}

			this.UpdatePosition();
		}
	}

	public override Rect ScreenRect
	{
		get
		{
			if (MemoryService.Process == null)
				return new Rect(0, 0, 0, 0);

			GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect gameRect);
			return new Rect(0, 0, gameRect.Right - gameRect.Left, gameRect.Bottom - gameRect.Top);
		}
	}

	public override bool CanPopOut => true;
	public override bool CanPopIn => false;

	protected override void OnWindowLoaded()
	{
		if (MemoryService.Process == null)
			return;

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

		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, 0, 0, 128, 128, 0x0040);
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001);

		this.UpdatePosition();
	}

	protected override void UpdatePosition()
	{
		base.UpdatePosition();

		if (!this.IsActive)
			return;

		if (MemoryService.Process == null)
			return;

		int w = (int)this.Width - 1;
		int h = (int)this.Height - 1;

		GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect gameWindowRect);
		int gameWindowWidth = gameWindowRect.Right - gameWindowRect.Left;
		int hameWindowHeight = gameWindowRect.Bottom - gameWindowRect.Top;

		int x = Math.Clamp(this.x, 0, gameWindowWidth - w);
		int y = Math.Clamp(this.y, 0, hameWindowHeight - h);

		// SHOWWINDOW
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, 0, 0, w, h,  0x0040);

		// NOSIZE
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, x, y, 0, 0,  0x0001);

		if (this.Topmost)
		{
			// NOSIZE | NOMOVE
			SetWindowPos(this.windowInteropHelper.Handle, (IntPtr)(-1), 0, 0, 0, 0,  0x0001 | 0x0003);
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hwnd, out Win32Rect rect);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetClientRect(IntPtr hwnd, out Win32Rect rect);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}
}
