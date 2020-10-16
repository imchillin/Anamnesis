// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System.Windows;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class MainApplicationSettings : SettingsBase
	{
		public string Language { get; set; } = "EN";
		public bool AlwaysOnTop { get; set; } = true;
		public bool ThemeDark { get; set; } = false;
		public string ThemeSwatch { get; set; } = @"deeppurple";
		public double Opacity { get; set; } = 1.0;
		public bool StayTransparent { get; set; } = false;
		public double Scale { get; set; } = 1.0;
		public bool UseWindowsExplorer { get; set; } = false;
		public Point WindowPosition { get; set; }
	}
}
