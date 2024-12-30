// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Files;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

/// <summary>
/// Interaction logic for GeneralSettingsPage.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class GeneralSettingsPage : System.Windows.Controls.UserControl, ISettingSection
{
	public GeneralSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Initialize setting categories
		this.SettingCategories = new()
		{
			{ "Interface", new SettingCategory("Interface", this.InterfaceGroupBox) },
			{ "Files", new SettingCategory("Files", this.FilesGroupBox) },
			{ "Directories", new SettingCategory("Directories", this.DirectoriesGroupBox) },
		};

		// Set up interface category settings
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_Language", this.General_Interface_Language));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_AlwaysOnTop", this.General_Interface_AlwaysOnTop));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_Overlay", this.General_Interface_MiniMode));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_WindowSize", this.General_Interface_WindowSize));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_Translucency", this.General_Interface_EnableTranslucency));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_WindowOpacity", this.General_Interface_WindowOpacity));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_Font", this.General_Interface_Font));
		this.SettingCategories["Interface"].Settings.Add(new Setting("Settings_Developer", this.General_Interface_Developer));

		// Set up files category settings
		this.SettingCategories["Files"].Settings.Add(new Setting("Settings_DefaultAuthor", this.General_Files_DefaultAuthor));
		this.SettingCategories["Files"].Settings.Add(new Setting("Settings_ShowFileExtensions", this.General_Files_ShowFileExtensions));
		this.SettingCategories["Files"].Settings.Add(new Setting("Settings_UseWindowsExplorer", this.General_Files_UseWindowsExplorer));

		// Set up directories category settings
		this.SettingCategories["Directories"].Settings.Add(new Setting("Settings_Dir_Characters", this.General_Directories_Char));
		this.SettingCategories["Directories"].Settings.Add(new Setting("Settings_Dir_Poses", this.General_Directories_Poses));
		this.SettingCategories["Directories"].Settings.Add(new Setting("Settings_Dir_CameraShots", this.General_Directories_CamShots));
		/* this.SettingCategories["Directories"].Settings.Add(new Setting("Settings_Dir_Scenes", this.General_Directories_Scenes)); */

		// Set up window size options
		this.SizeSelector.ItemsSource = new List<double>() { 0.75, 1.0, 1.25, 1.5, 1.75, 2.0 };

		// Set up font options
		this.Fonts = Enum.GetValues<Settings.Fonts>()
			.Cast<Settings.Fonts>()
			.Select(font => new FontOption(font))
			.ToList();

		// Set up language options
		this.Languages = LocalizationService.GetAvailableLocales()
			.Select(locale => new LanguageOption(locale.Key, locale.Value))
			.ToList();
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static int LabelColumnWidth => 150;
	public Dictionary<string, SettingCategory> SettingCategories { get; }
	public IEnumerable<FontOption> Fonts { get; }

	[DependsOn(nameof(this.Fonts))]
	public FontOption SelectedFont
	{
		get => this.Fonts.FirstOrDefault(font => font.Font == SettingsService.Current.Font) ?? this.Fonts.First();
		set => SettingsService.Current.Font = value.Font;
	}

	public IEnumerable<LanguageOption> Languages { get; }
	[DependsOn(nameof(this.Languages))]
	public LanguageOption SelectedLanguage
	{
		get => this.Languages.FirstOrDefault(language => language.Key == SettingsService.Current.Language) ?? this.Languages.First();
		set
		{
			SettingsService.Current.Language = value.Key;
			LocalizationService.SetLocale(value.Key);
		}
	}

	private void OnBrowseCharacter(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new()
		{
			SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultCharacterDirectory),
		};
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultCharacterDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnBrowsePose(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new()
		{
			SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultPoseDirectory),
		};
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultPoseDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnBrowseCamera(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new()
		{
			SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultCameraShotDirectory),
		};
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultCameraShotDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnBrowseScene(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new()
		{
			SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultSceneDirectory),
		};
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultSceneDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	public class FontOption
	{
		public FontOption(Settings.Fonts font)
		{
			this.Key = "Settings_Font_" + font.ToString();
			this.Font = font;
		}

		public string Key { get; }
		public Settings.Fonts Font { get; }
	}

	public class LanguageOption
	{
		public LanguageOption(string key, string display)
		{
			this.Key = key;
			this.Display = display;
		}

		public string Key { get; }
		public string Display { get; }
	}
}
