// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Files;
using Anamnesis.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

/// <summary>
/// Interaction logic for BackupAndRecoverySettingsPage.xaml.
/// </summary>
public partial class BackupAndRecoverySettingsPage : System.Windows.Controls.UserControl, ISettingSection
{
	public BackupAndRecoverySettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Initialize setting categories
		this.SettingCategories = new()
		{
			{ "Backup", new SettingCategory("Backup", this.BackupGroupBox) },
		};

		// Set up backup category settings
		this.SettingCategories["Backup"].Settings.Add(new Setting("Settings_EnableAutoSave", this.BnR_Backup_EnableAutoSave));
		this.SettingCategories["Backup"].Settings.Add(new Setting("Settings_AutoSaveDirectory", this.BnR_Backup_AutoSaveDirectory));
		this.SettingCategories["Backup"].Settings.Add(new Setting("Settings_AutoSaveInterval", this.BnR_Backup_AutoSaveInterval));
		this.SettingCategories["Backup"].Settings.Add(new Setting("Settings_AutoSaveSaveLast", this.BnR_Backup_AutoSaveSaveLast));
		this.SettingCategories["Backup"].Settings.Add(new Setting("Settings_AutoSaveOnlyInGpose", this.BnR_Backup_AutoSaveOnlyInGpose));
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static int LabelColumnWidth => 150;
	public Dictionary<string, SettingCategory> SettingCategories { get; }

	private void OnBrowseAutoSave(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new()
		{
			SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultAutoSaveDirectory),
		};
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultAutoSaveDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}
}
