// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Forms;
	using Anamnesis.Files;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ThemeSettingsView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SettingsView : System.Windows.Controls.UserControl
	{
		public SettingsView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			List<double> sizes = new List<double>();
			sizes.Add(0.5);
			sizes.Add(0.6);
			sizes.Add(0.8);
			sizes.Add(0.9);
			sizes.Add(1.0);
			sizes.Add(1.25);
			sizes.Add(1.5);
			sizes.Add(1.75);
			sizes.Add(2.0);
			this.SizeSelector.ItemsSource = sizes;

			List<LanguageOption> languages = new List<LanguageOption>();

			foreach ((string key, string name) in LocalizationService.GetAvailableLocales())
			{
				languages.Add(new LanguageOption(key, name));
			}

			this.Languages = languages;
		}

		public SettingsService SettingsService => SettingsService.Instance;

		public IEnumerable<LanguageOption> Languages { get; }

		public LanguageOption SelectedLanguage
		{
			get
			{
				foreach (LanguageOption language in this.Languages)
				{
					if (language.Key == SettingsService.Current.Language)
					{
						return language;
					}
				}

				return this.Languages.First();
			}

			set
			{
				SettingsService.Current.Language = value.Key;
				LocalizationService.SetLocale(value.Key);
			}
		}

		private void OnBrowseCharacter(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultCharacterDirectory);
			DialogResult result = dlg.ShowDialog();

			if (result != DialogResult.OK)
				return;

			SettingsService.Current.DefaultCharacterDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
		}

		private void OnBrowsePose(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultPoseDirectory);
			DialogResult result = dlg.ShowDialog();

			if (result != DialogResult.OK)
				return;

			SettingsService.Current.DefaultPoseDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
		}

		private void OnBrowseScene(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultSceneDirectory);
			DialogResult result = dlg.ShowDialog();

			if (result != DialogResult.OK)
				return;

			SettingsService.Current.DefaultSceneDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
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
}