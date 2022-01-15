// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Media;
	using PropertyChanged;

	[Serializable]
	[AddINotifyPropertyChangedInterface]
	public class Settings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Language { get; set; } = "EN";
		public bool AlwaysOnTop { get; set; } = true;
		public bool OverlayWindow { get; set; } = false;
		public double Opacity { get; set; } = 1.0;
		public double Scale { get; set; } = 1.0;
		public bool ShowFileExtensions { get; set; } = false;
		public bool UseWindowsExplorer { get; set; } = false;
		public Point WindowPosition { get; set; }
		public Point OverlayWindowPosition { get; set; }
		public string DefaultPoseDirectory { get; set; } = "%MyDocuments%/Anamnesis/Poses/";
		public string DefaultCharacterDirectory { get; set; } = "%MyDocuments%/Anamnesis/Characters/";
		public string DefaultSceneDirectory { get; set; } = "%MyDocuments%/Anamnesis/Scenes/";
		public bool ShowAdvancedOptions { get; set; } = true;
		public bool FlipPoseGuiSides { get; set; } = false;
		public bool UseHyperlegibleFont { get; set; } = false;
		public bool ShowGallery { get; set; } = true;
		public string? GalleryDirectory { get; set; }
		public bool EnableTranslucency { get; set; } = true;
		public bool ExtendIntoWindowChrome { get; set; } = true;

		public bool OverrideSystemTheme { get; set; } = false;
		public Color ThemeColor { get; set; } = Color.FromArgb(255, 247, 99, 12);
		public bool ThemeLight { get; set; } = false;
		public bool WrapRotationSliders { get; set; } = true;

		public DateTimeOffset LastUpdateCheck { get; set; } = DateTimeOffset.MinValue;
	}
}
