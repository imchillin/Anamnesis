// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using System.Windows;
using System.Windows.Forms;

public static class ScreenUtils
{
	public static bool IsOnScreen(Point val)
	{
		var dpi = GUI.MainWindow.GetDpi();

		bool found = false;

		foreach (Screen screen in Screen.AllScreens)
		{
			found |= screen.Bounds.Contains(new System.Drawing.Point((int)(val.X * dpi.DpiScaleX), (int)(val.Y * dpi.DpiScaleY)));
		}

		return found;
	}
}
