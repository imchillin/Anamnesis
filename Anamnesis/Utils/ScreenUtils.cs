// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils
{
	using System.Windows.Forms;

	public static class ScreenUtils
	{
		public static bool IsOnScreen(System.Windows.Point val)
		{
			bool found = false;

			foreach (Screen screen in Screen.AllScreens)
			{
				found |= screen.Bounds.Contains(new System.Drawing.Point((int)val.X, (int)val.Y));
			}

			return found;
		}
	}
}
