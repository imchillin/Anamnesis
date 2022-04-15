// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media;
	using Anamnesis.Keyboard;
	using PropertyChanged;

	[Serializable]
	[AddINotifyPropertyChangedInterface]
	public class Settings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public enum Fonts
		{
			Default,
			Hyperlegible,
		}

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
		public Fonts Font { get; set; } = Fonts.Default;
		public bool ShowGallery { get; set; } = true;
		public string? GalleryDirectory { get; set; }
		public bool EnableTranslucency { get; set; } = true;
		public bool ExtendIntoWindowChrome { get; set; } = true;
		public bool UseExternalRefresh { get; set; } = false;
		public bool EnableGameHotkeyHooks { get; set; } = false;
		public bool EnableHotkeys { get; set; } = true;

		public bool OverrideSystemTheme { get; set; } = false;
		public Color ThemeColor { get; set; } = Color.FromArgb(255, 247, 99, 12);
		public bool ThemeLight { get; set; } = false;
		public bool WrapRotationSliders { get; set; } = true;
		public string? DefaultAuthor { get; set; }

		public DateTimeOffset LastUpdateCheck { get; set; } = DateTimeOffset.MinValue;

		public string? DebugGamePath { get; set; }

		public Dictionary<string, KeyCombination> KeyboardBindings { get; set; } = new()
		{
			{ "QuaternionEditor.RotateZPlus", new KeyCombination(Key.S) },
			{ "QuaternionEditor.RotateZMinus", new KeyCombination(Key.W) },
			{ "QuaternionEditor.RotateXPlus", new KeyCombination(Key.A) },
			{ "QuaternionEditor.RotateXMinus", new KeyCombination(Key.D) },
			{ "QuaternionEditor.RotateYPlus", new KeyCombination(Key.Q) },
			{ "QuaternionEditor.RotateYMinus", new KeyCombination(Key.E) },
			{ "QuaternionEditor.RotateZPlusFast", new KeyCombination(Key.S, ModifierKeys.Shift) },
			{ "QuaternionEditor.RotateZMinusFast", new KeyCombination(Key.W, ModifierKeys.Shift) },
			{ "QuaternionEditor.RotateXPlusFast", new KeyCombination(Key.A, ModifierKeys.Shift) },
			{ "QuaternionEditor.RotateXMinusFast", new KeyCombination(Key.D, ModifierKeys.Shift) },
			{ "QuaternionEditor.RotateYPlusFast", new KeyCombination(Key.Q, ModifierKeys.Shift) },
			{ "QuaternionEditor.RotateYMinusFast", new KeyCombination(Key.E, ModifierKeys.Shift) },
			{ "QuaternionEditor.RotateZPlusSlow", new KeyCombination(Key.S, ModifierKeys.Control) },
			{ "QuaternionEditor.RotateZMinusSlow", new KeyCombination(Key.W, ModifierKeys.Control) },
			{ "QuaternionEditor.RotateXPlusSlow", new KeyCombination(Key.A, ModifierKeys.Control) },
			{ "QuaternionEditor.RotateXMinusSlow", new KeyCombination(Key.D, ModifierKeys.Control) },
			{ "QuaternionEditor.RotateYPlusSlow", new KeyCombination(Key.Q, ModifierKeys.Control) },
			{ "QuaternionEditor.RotateYMinusSlow", new KeyCombination(Key.E, ModifierKeys.Control) },
			{ "AppearancePage.ClearEquipment", new KeyCombination(Key.C, ModifierKeys.Control | ModifierKeys.Shift) },
			{ "TargetService.SelectPinned1", new KeyCombination(Key.F1) },
			{ "TargetService.SelectPinned2", new KeyCombination(Key.F2) },
			{ "TargetService.SelectPinned3", new KeyCombination(Key.F3) },
			{ "TargetService.SelectPinned4", new KeyCombination(Key.F4) },
			{ "TargetService.SelectPinned5", new KeyCombination(Key.F5) },
			{ "TargetService.SelectPinned6", new KeyCombination(Key.F6) },
			{ "TargetService.SelectPinned7", new KeyCombination(Key.F7) },
			{ "TargetService.SelectPinned8", new KeyCombination(Key.F8) },
			{ "TargetService.NextPinned", new KeyCombination(Key.Right, ModifierKeys.Control) },
			{ "TargetService.PrevPinned", new KeyCombination(Key.Left, ModifierKeys.Control) },
			{ "MainWindow.SceneTab", new KeyCombination(Key.D1) },
			{ "MainWindow.AppearanceTab", new KeyCombination(Key.D2) },
			{ "MainWindow.ActionTab", new KeyCombination(Key.D3) },
			{ "MainWindow.PoseTab", new KeyCombination(Key.D4) },
			{ "System.Undo", new KeyCombination(Key.Z, ModifierKeys.Control) },
			////{ "System.Redo", new KeyCombination(Key.Y, ModifierKeys.Control) },
		};
	}
}
