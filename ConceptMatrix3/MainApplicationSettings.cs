// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI
{
	using System.Threading.Tasks;
	using MaterialDesignThemes.Wpf;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class MainApplicationSettings : SettingsBase
	{
		public bool AlwaysOnTop { get; set; } = true;
		public bool ThemeDark { get; set; } = false;
		public string ThemeSwatch { get; set; } = @"deeppurple";
		public double Opacity { get; set; } = 1.0;
		public double Scale { get; set; } = 1.0;
	}
}
