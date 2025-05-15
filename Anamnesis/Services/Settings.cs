// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Keyboard;
using Anamnesis.Styles.Controls;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

[Serializable]
[AddINotifyPropertyChangedInterface]
public class Settings : INotifyPropertyChanged
{
	private const int MinAutoSaveIntervalMinutes = 1;
	private int autoSaveIntervalMinutes = 5;
	private bool enableAutoSave = true;

	public event PropertyChangedEventHandler? PropertyChanged;

	public enum Fonts
	{
		Default,
		Hyperlegible,
	}

	public enum GizmoDragModes
	{
		Linear,
		Circular,
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
	public string DefaultCameraShotDirectory { get; set; } = "%MyDocuments%/Anamnesis/CameraShots/";
	public string DefaultSceneDirectory { get; set; } = "%MyDocuments%/Anamnesis/Scenes/";
	public string DefaultAutoSaveDirectory { get; set; } = "%MyDocuments%/Anamnesis/AutoSave/";
	public bool ShowAdvancedOptions { get; set; } = true;
	public bool FlipPoseGuiSides { get; set; } = false;
	public Fonts Font { get; set; } = Fonts.Default;
	public bool ShowGallery { get; set; } = true;
	public string? GalleryDirectory { get; set; }
	public bool EnableTranslucency { get; set; } = true;
	public bool ExtendIntoWindowChrome { get; set; } = true;
	public bool UseExternalRefresh { get; set; } = false;
	public bool UseExternalRefreshBrio { get; set; } = false;
	public bool EnableNpcHack { get; set; } = false;
	public bool EnableGameHotkeyHooks { get; set; } = false;
	public bool EnableHotkeys { get; set; } = true;
	public bool ForwardKeys { get; set; } = true;
	public bool EnableDeveloperTab { get; set; } = false;
	public bool ReapplyAppearance { get; set; } = false;
	public bool OverrideSystemTheme { get; set; } = false;
	public Color ThemeTrimColor { get; set; } = Color.FromArgb(255, 247, 99, 12);
	public bool ThemeLight { get; set; } = false;
	public bool WrapRotationSliders { get; set; } = true;
	public string? DefaultAuthor { get; set; }
	public DateTimeOffset LastUpdateCheck { get; set; } = DateTimeOffset.MinValue;
	public string? GamePath { get; set; }
	public Binds KeyboardBindings { get; set; } = new();
	public Dictionary<string, int> ActorTabOrder { get; set; } = new();
	public Dictionary<string, bool> PosingBoneLinks { get; set; } = new();
	public double ViewportPanSpeed { get; set; } = 1;
	public double ViewportZoomSpeed { get; set; } = 1;
	public double ViewportRotationSpeed { get; set; } = 1;
	public bool EnableAutoSave
	{
		get => this.enableAutoSave;
		set
		{
			if (value == this.enableAutoSave)
				return;

			this.enableAutoSave = value;

			if (AutoSaveService.Instance != null && AutoSaveService.Instance.IsAlive)
				AutoSaveService.Instance.RestartUpdateTask();
		}
	}

	public int AutoSaveFileCount { get; set; } = 12;
	public int AutoSaveIntervalMinutes
	{
		get => Math.Max(this.autoSaveIntervalMinutes, MinAutoSaveIntervalMinutes);
		set
		{
			if (value == this.autoSaveIntervalMinutes)
				return;

			// Limit the minimum value
			if (value < MinAutoSaveIntervalMinutes)
				value = MinAutoSaveIntervalMinutes;

			this.autoSaveIntervalMinutes = value;

			if (AutoSaveService.Instance != null && AutoSaveService.Instance.IsAlive)
				AutoSaveService.Instance.RestartUpdateTask();
		}
	}
	public bool AutoSaveOnlyInGpose { get; set; } = true;

	public GizmoDragModes GizmoDragMode { get; set; } = GizmoDragModes.Linear;

	public double SliderSensitivity { get; set; } = 1;
	public double SliderPrecision { get; set; } = 3;
	public bool SliderKeepInputOpenOnCommit { get; set; } = false;

	public SliderInputBox.SliderTypes TimeAndWeatherSliderType { get; set; } = SliderInputBox.SliderTypes.Classic;
	public SliderInputBox.SliderTypes CameraSliderType { get; set; } = SliderInputBox.SliderTypes.Modern;
	public SliderInputBox.SliderTypes BoneRotationSliderType { get; set; } = SliderInputBox.SliderTypes.Modern;
	public SliderInputBox.SliderTypes BonePositionSliderType { get; set; } = SliderInputBox.SliderTypes.Modern;
	public SliderInputBox.SliderTypes BoneScaleSliderType { get; set; } = SliderInputBox.SliderTypes.Modern;
	public bool ShowSliderThumb { get; set; } = false;

	public double WindowOpacity
	{
		get
		{
			if (!this.EnableTranslucency)
				return 1.0;

			return this.Opacity;
		}
	}

	[Serializable]
	public class Binds
	{
		public KeyCombination QuaternionEditor_RotateZPlus { get; set; } = new(Key.S);
		public KeyCombination QuaternionEditor_RotateZMinus { get; set; } = new(Key.W);
		public KeyCombination QuaternionEditor_RotateXPlus { get; set; } = new(Key.A);
		public KeyCombination QuaternionEditor_RotateXMinus { get; set; } = new(Key.D);
		public KeyCombination QuaternionEditor_RotateYPlus { get; set; } = new(Key.Q);
		public KeyCombination QuaternionEditor_RotateYMinus { get; set; } = new(Key.E);
		public KeyCombination QuaternionEditor_RotateZPlusFast { get; set; } = new(Key.S, ModifierKeys.Shift);
		public KeyCombination QuaternionEditor_RotateZMinusFast { get; set; } = new(Key.W, ModifierKeys.Shift);
		public KeyCombination QuaternionEditor_RotateXPlusFast { get; set; } = new(Key.A, ModifierKeys.Shift);
		public KeyCombination QuaternionEditor_RotateXMinusFast { get; set; } = new(Key.D, ModifierKeys.Shift);
		public KeyCombination QuaternionEditor_RotateYPlusFast { get; set; } = new(Key.Q, ModifierKeys.Shift);
		public KeyCombination QuaternionEditor_RotateYMinusFast { get; set; } = new(Key.E, ModifierKeys.Shift);
		public KeyCombination QuaternionEditor_RotateZPlusSlow { get; set; } = new(Key.S, ModifierKeys.Control);
		public KeyCombination QuaternionEditor_RotateZMinusSlow { get; set; } = new(Key.W, ModifierKeys.Control);
		public KeyCombination QuaternionEditor_RotateXPlusSlow { get; set; } = new(Key.A, ModifierKeys.Control);
		public KeyCombination QuaternionEditor_RotateXMinusSlow { get; set; } = new(Key.D, ModifierKeys.Control);
		public KeyCombination QuaternionEditor_RotateYPlusSlow { get; set; } = new(Key.Q, ModifierKeys.Control);
		public KeyCombination QuaternionEditor_RotateYMinusSlow { get; set; } = new(Key.E, ModifierKeys.Control);
		public KeyCombination CharacterPage_ClearEquipment { get; set; } = new(Key.C, ModifierKeys.Control | ModifierKeys.Shift);
		public KeyCombination TargetService_SelectPinned1 { get; set; } = new(Key.F1);
		public KeyCombination TargetService_SelectPinned2 { get; set; } = new(Key.F2);
		public KeyCombination TargetService_SelectPinned3 { get; set; } = new(Key.F3);
		public KeyCombination TargetService_SelectPinned4 { get; set; } = new(Key.F4);
		public KeyCombination TargetService_SelectPinned5 { get; set; } = new(Key.F5);
		public KeyCombination TargetService_SelectPinned6 { get; set; } = new(Key.F6);
		public KeyCombination TargetService_SelectPinned7 { get; set; } = new(Key.F7);
		public KeyCombination TargetService_SelectPinned8 { get; set; } = new(Key.F8);
		public KeyCombination TargetService_NextPinned { get; set; } = new(Key.Right, ModifierKeys.Control);
		public KeyCombination TargetService_PrevPinned { get; set; } = new(Key.Left, ModifierKeys.Control);
		public KeyCombination MainWindow_AppearanceTab { get; set; } = new(Key.D1);
		public KeyCombination MainWindow_ActionTab { get; set; } = new(Key.D2);
		public KeyCombination MainWindow_PoseTab { get; set; } = new(Key.D3);
		public KeyCombination System_Undo { get; set; } = new(Key.Z, ModifierKeys.Control);
		public KeyCombination System_Redo { get; set; } = new(Key.Y, ModifierKeys.Control);
		public KeyCombination ActionPage_ResumeAll { get; set; } = new(Key.P, ModifierKeys.Control | ModifierKeys.Shift);
		public KeyCombination ActionPage_PauseAll { get; set; } = new(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

		public Dictionary<string, KeyCombination> GetBinds()
		{
			Dictionary<string, KeyCombination> results = new();
			PropertyInfo[]? properties = typeof(Binds).GetProperties();
			foreach (PropertyInfo property in properties)
			{
				KeyCombination? key = property.GetValue(SettingsService.Current.KeyboardBindings) as KeyCombination;

				if (key == null)
					continue;

				string function = property.Name.Replace('_', '.');
				results.Add(function, key);
			}

			return results;
		}
	}
}
