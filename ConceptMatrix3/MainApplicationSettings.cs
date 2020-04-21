// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI
{
	using System.Threading.Tasks;
	using MaterialDesignThemes.Wpf;

	public class MainApplicationSettings : SettingsBase
	{
		public bool AlwaysOnTop { get; set; } = true;
		public bool ThemeDark { get; set; } = false;
		public string ThemeSwatch { get; set; } = @"deeppurple";
		public double Opacity { get; set; } = 1.0;

		public override async Task OnLoaded(ISettingsService settingsService)
		{
			await base.OnLoaded(settingsService);

			PaletteHelper palette = new PaletteHelper();
			palette.Apply(this.ThemeSwatch, this.ThemeDark);
		}
	}
}
