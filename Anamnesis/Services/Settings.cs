// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using PropertyChanged;

	[Serializable]
	[AddINotifyPropertyChangedInterface]
	public class Settings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Language { get; set; } = "EN";
		public bool AlwaysOnTop { get; set; } = true;
		public bool ThemeDark { get; set; } = true;
		public string ThemeSwatch { get; set; } = @"deeporange";
		public double Opacity { get; set; } = 1.0;
		public bool StayTransparent { get; set; } = false;
		public double Scale { get; set; } = 1.0;
		public bool UseWindowsExplorer { get; set; } = false;
		public Point WindowPosition { get; set; }
		public string DefaultPoseDirectory { get; set; } = "%MyDocuments%/Anamnesis/Poses/";
		public string DefaultCharacterDirectory { get; set; } = "%MyDocuments%/Anamnesis/Characters/";
		public string DefaultSceneDirectory { get; set; } = "%MyDocuments%/Anamnesis/Scenes/";
		public bool UseCustomBorder { get; set; } = true;
		public bool ShowAdvancedOptions { get; set; } = true;
		public bool FlipPoseGuiSides { get; set; } = false;
		public bool IsDyslexic { get; set; } = false;
		public bool ShowGallery { get; set; } = true;

		public DateTimeOffset LastUpdateCheck { get; set; } = DateTimeOffset.MinValue;
	}
}
