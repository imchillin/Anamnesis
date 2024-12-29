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
public partial class GeneralSettingsPage : System.Windows.Controls.UserControl
{
	public GeneralSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

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
