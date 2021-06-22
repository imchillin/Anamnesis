// © Anamnesis.
// Licensed under the MIT license.

namespace MaterialDesignThemes.Wpf
{
	using System.Collections.Generic;
	using MaterialDesignColors;

	public static class PaletteHelperExtensions
	{
		private static SwatchesProvider swatchesProvider = new SwatchesProvider();

		public static IEnumerable<Swatch> GetSwatches(this PaletteHelper self)
		{
			return swatchesProvider.Swatches;
		}

		public static Swatch? GetSwatch(this PaletteHelper self, string name)
		{
			foreach (Swatch swatch in new SwatchesProvider().Swatches)
			{
				if (swatch.Name == name)
				{
					return swatch;
				}
			}

			return null;
		}

		public static void Apply(this PaletteHelper self, string swatchName, bool dark)
		{
			Swatch? swatch = self.GetSwatch(swatchName);
			if (swatch != null)
			{
				self.Apply(swatch, dark);
			}
		}

		public static void Apply(this PaletteHelper self, Swatch swatch, bool dark)
		{
			ITheme theme = self.GetTheme();
			theme.SetBaseTheme(dark ? Theme.Dark : Theme.Light);

			if (swatch != null)
			{
				theme.SetPrimaryColor(swatch.ExemplarHue.Color);

				if (swatch.AccentExemplarHue != null)
				{
					theme.SetSecondaryColor(swatch.AccentExemplarHue.Color);
				}
			}

			self.SetTheme(theme);
		}

		public static bool IsDark(this PaletteHelper self)
		{
			ITheme theme = self.GetTheme();
			return theme.Background == Theme.Dark.MaterialDesignBackground;
		}

		public static Swatch? GetCurrentSwatch(this PaletteHelper self)
		{
			ITheme theme = self.GetTheme();
			foreach (Swatch swatch in self.GetSwatches())
			{
				if (theme.PrimaryMid.Color == swatch.ExemplarHue.Color &&
					theme.SecondaryMid.Color == swatch.AccentExemplarHue?.Color)
				{
					return swatch;
				}
			}

			return null;
		}
	}
}
