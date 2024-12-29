// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Files;
using Anamnesis.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

/// <summary>
/// Interaction logic for PersonalizationSettingsPage.xaml.
/// </summary>
public partial class PersonalizationSettingsPage : System.Windows.Controls.UserControl
{
	public PersonalizationSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Set up gallery options
		if (!SettingsService.Current.ShowGallery)
			this.GalleryCombobox.SelectedIndex = 0;
		else if (string.IsNullOrEmpty(SettingsService.Current.GalleryDirectory))
			this.GalleryCombobox.SelectedIndex = 1;
		else
			this.GalleryCombobox.SelectedIndex = 2;
	}

	public static SettingsService SettingsService => SettingsService.Instance;

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
