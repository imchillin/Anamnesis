// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Files;
using Anamnesis.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

/// <summary>
/// Interaction logic for PersonalizationSettingsPage.xaml.
/// </summary>
public partial class PersonalizationSettingsPage : System.Windows.Controls.UserControl, ISettingSection
{
	public PersonalizationSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Initialize setting categories
		this.SettingCategories = new()
		{
			{ "Theme", new SettingCategory("Theme", this.ThemeGroupBox) },
			{ "Gallery", new SettingCategory("Gallery", this.GalleryGroupBox) },
		};

		// Set up theme settings category
		this.SettingCategories["Theme"].Settings.Add(new Setting("Settings_Theme_Override", this.Personalization_Theme_Override));
		this.SettingCategories["Theme"].Settings.Add(new Setting("Settings_Theme_Light", this.Personalization_Theme_Light));
		this.SettingCategories["Theme"].Settings.Add(new Setting("Settings_Theme_Color", this.Personalization_Theme_Color));

		// Set up gallery settings category
		this.SettingCategories["Gallery"].Settings.Add(new Setting("Settings_Gallery", this.Personalization_Gallery_Mode));

		// Set up gallery options
		if (!SettingsService.Current.ShowGallery)
			this.GalleryCombobox.SelectedIndex = 0;
		else if (string.IsNullOrEmpty(SettingsService.Current.GalleryDirectory))
			this.GalleryCombobox.SelectedIndex = 1;
		else
			this.GalleryCombobox.SelectedIndex = 2;
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static int LabelColumnWidth => 150;
	public Dictionary<string, SettingCategory> SettingCategories { get; }

	private void OnBrowseGallery(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new();
		if (SettingsService.Current.GalleryDirectory != null)
			dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.GalleryDirectory);

		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.GalleryDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnGalleryChanged(object sender, SelectionChangedEventArgs e)
	{
		// 0 - None
		// 1 - Curated
		// 2 - Local
		if (this.GalleryCombobox.SelectedIndex != 2)
			SettingsService.Current.GalleryDirectory = null;

		SettingsService.Current.ShowGallery = this.GalleryCombobox.SelectedIndex != 0;
	}
}
