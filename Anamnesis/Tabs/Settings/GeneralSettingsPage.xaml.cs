// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Files;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Services;
using Anamnesis.Updater;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using XivToolsWpf.Utility;

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
			{ "Updates", new SettingCategory("Updates", this.UpdatesGroupBox) },
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

		// Set up updates category settings
		this.SettingCategories["Updates"].Settings.Add(new Setting("Settings_ReleaseChannel", this.General_Updates_ReleaseChannel));

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

		if (UpdateService.GlobalManifest?.Channels != null)
		{
			this.ReleaseChannels = UpdateService.GlobalManifest.Channels
				.Select(channel =>
				{
					string display = channel.Key == Settings.DEFAULT_UPDATE_CHANNEL
						? $"{channel.Value.Name} ({LocalizationService.GetString("Common_Default")})"
						: channel.Value.Name;

					return new ReleaseChannelOption(channel.Key, display);
				})
				.ToList();
		}
		else
		{
			this.ReleaseChannels = new List<ReleaseChannelOption>();
		}
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static int LabelColumnWidth => 150;
	public static bool IsWindows11 => Win32.IsWindows11();
	public Dictionary<string, SettingCategory> SettingCategories { get; }

	public IEnumerable<FontOption> Fonts { get; }

	[DependsOn(nameof(Fonts))]
	public FontOption SelectedFont
	{
		get => this.Fonts.FirstOrDefault(font => font.Font == SettingsService.Current.Font) ?? this.Fonts.First();
		set => SettingsService.Current.Font = value.Font;
	}

	public IEnumerable<LanguageOption> Languages { get; }

	[DependsOn(nameof(Languages))]
	public LanguageOption SelectedLanguage
	{
		get => this.Languages.FirstOrDefault(language => language.Key.Equals(SettingsService.Current.Language, StringComparison.CurrentCultureIgnoreCase)) ?? this.Languages.First();
		set
		{
			SettingsService.Current.Language = value.Key;
			LocalizationService.SetLocale(value.Key);
		}
	}

	public IEnumerable<ReleaseChannelOption> ReleaseChannels { get; }

	[DependsOn(nameof(ReleaseChannels))]
	public bool HasReleaseChannels => this.ReleaseChannels.Any();

	[DependsOn(nameof(ReleaseChannels))]
	public ReleaseChannelOption? SelectedReleaseChannel
	{
		get => this.ReleaseChannels.FirstOrDefault(channel => channel.Key.Equals(SettingsService.Current.UpdateChannel, StringComparison.CurrentCultureIgnoreCase));
		set
		{
			if (value == null)
				return;

			SettingsService.Current.UpdateChannel = value.Key;
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

	private async void OnCheckForUpdates(object sender, RoutedEventArgs e)
	{
		bool didUpdate = await UpdateService.Instance.CheckForUpdates();

		if (!didUpdate)
		{
			await GenericDialog.ShowLocalizedAsync("Update_NoUpdate", "Update_Title", MessageBoxButton.OK);
		}
	}

	private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		e.Handled = true;
	}

	public class FontOption(Settings.Fonts font)
	{
		public string Key { get; } = "Settings_Font_" + font.ToString();
		public Settings.Fonts Font { get; } = font;
	}

	public class LanguageOption(string key, string display)
	{
		public string Key { get; } = key;
		public string Display { get; } = display;
	}

	public class ReleaseChannelOption(string key, string display)
	{
		public string Key { get; } = key;
		public string Display { get; } = display;
	}
}
