// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.Windows;
	using System.Windows.Media;
	using FontAwesome.Sharp;

	public class IconBlock : IconBlockBase<IconChar>
	{
		private static readonly Typeface[] Typefaces = typeof(IconHelper).Assembly.LoadTypefaces("fonts", "Font Awesome 5 Free Solid");

		public IconBlock()
			: base(Font)
		{
		}

		private static FontFamily Font => Typefaces[0].FontFamily;

		protected override FontFamily FontFor(IconChar icon)
		{
			return Font;
		}
	}
}
