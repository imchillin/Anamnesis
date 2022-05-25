// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Anamnesis.Files;
using Anamnesis.Keyboard;
using Anamnesis.Services;
using PropertyChanged;

/// <summary>
/// Interaction logic for ThemeSettingsView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class SettingsTab : System.Windows.Controls.UserControl
{
	public SettingsTab()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		List<double> sizes = new List<double>();
		sizes.Add(0.75);
		sizes.Add(1.0);
		sizes.Add(1.25);
		sizes.Add(1.5);
		sizes.Add(1.75);
		sizes.Add(2.0);
		this.SizeSelector.ItemsSource = sizes;

		List<FontOption> fonts = new();
		foreach (Settings.Fonts font in Enum.GetValues<Settings.Fonts>())
		{
			fonts.Add(new FontOption(font));
		}

		this.Fonts = fonts;

		List<LanguageOption> languages = new();
		foreach ((string key, string name) in LocalizationService.GetAvailableLocales())
		{
			languages.Add(new LanguageOption(key, name));
		}

		this.Languages = languages;

		List<HotkeyOption> hotkeys = new();
		foreach ((string function, KeyCombination keys) in SettingsService.Current.KeyboardBindings.GetBinds())
		{
			hotkeys.Add(new HotkeyOption(function, keys));
		}

		this.Hotkeys = hotkeys;

		ICollectionView view = CollectionViewSource.GetDefaultView(this.Hotkeys);
		view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
		view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
		view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		this.HotkeyList.ItemsSource = view;

		if (!SettingsService.Current.ShowGallery)
		{
			this.GalleryCombobox.SelectedIndex = 0;
		}
		else if (string.IsNullOrEmpty(SettingsService.Current.GalleryDirectory))
		{
			this.GalleryCombobox.SelectedIndex = 1;
		}
		else
		{
			this.GalleryCombobox.SelectedIndex = 2;
		}
	}

	public SettingsService SettingsService => SettingsService.Instance;

	public IEnumerable<FontOption> Fonts { get; }
	public IEnumerable<LanguageOption> Languages { get; }
	public IEnumerable<HotkeyOption> Hotkeys { get; }

	public FontOption SelectedFont
	{
		get
		{
			foreach (FontOption font in this.Fonts)
			{
				if (font.Font == SettingsService.Current.Font)
				{
					return font;
				}
			}

			return this.Fonts.First();
		}

		set
		{
			SettingsService.Current.Font = value.Font;
		}
	}

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

	private void OnBrowseCamera(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new FolderBrowserDialog();
		dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.DefaultCameraShotDirectory);
		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.DefaultCameraShotDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
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

	private void OnBrowseGallery(object sender, RoutedEventArgs e)
	{
		FolderBrowserDialog dlg = new FolderBrowserDialog();

		if (SettingsService.Current.GalleryDirectory != null)
			dlg.SelectedPath = FileService.ParseToFilePath(SettingsService.Current.GalleryDirectory);

		DialogResult result = dlg.ShowDialog();

		if (result != DialogResult.OK)
			return;

		SettingsService.Current.GalleryDirectory = FileService.ParseFromFilePath(dlg.SelectedPath);
	}

	private void OnGalleryChanged(object sender, SelectionChangedEventArgs e)
	{
		// 0 - none
		// 1 - Curated
		// 2 - Local
		if (this.GalleryCombobox.SelectedIndex != 2)
			SettingsService.Current.GalleryDirectory = null;

		SettingsService.Current.ShowGallery = this.GalleryCombobox.SelectedIndex != 0;
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

	public class HotkeyOption
	{
		private readonly KeyCombination keys;
		private readonly string function;

		public HotkeyOption(string function, KeyCombination keys)
		{
			this.keys = keys;
			this.function = function;

			string[] parts = this.function.Split('.');
			if (parts.Length == 2)
			{
				this.Category = LocalizationService.GetString("HotkeyCategory_" + parts[0], true);
				if (this.Category == string.Empty)
					this.Category = parts[0];

				this.Name = LocalizationService.GetString("Hotkey_" + parts[1], true);
				if (this.Name == string.Empty)
					this.Name = parts[1];
			}
			else
			{
				this.Category = string.Empty;
				this.Name = LocalizationService.GetString("Hotkey_" + function, true);
				if (this.Name == string.Empty)
					this.Name = function;
			}
		}

		public string Category { get; }
		public string Name { get; }

		public string KeyName => this.keys.Key.ToString();
		public string? ModifierName
		{
			get
			{
				if (this.keys.Modifiers == ModifierKeys.None)
					return null;

				StringBuilder builder = new StringBuilder();
				bool hasContent = false;

				if (this.keys.Modifiers.HasFlag(ModifierKeys.Control))
				{
					builder.Append("Ctrl");
					hasContent = true;
				}

				if (this.keys.Modifiers.HasFlag(ModifierKeys.Shift))
				{
					if (hasContent)
						builder.Append(", ");

					builder.Append("Shift");
					hasContent = true;
				}

				if (this.keys.Modifiers.HasFlag(ModifierKeys.Alt))
				{
					if (hasContent)
						builder.Append(", ");

					builder.Append("Alt");
					hasContent = true;
				}

				if (this.keys.Modifiers.HasFlag(ModifierKeys.Windows))
				{
					if (hasContent)
						builder.Append(", ");

					builder.Append("Win");
					hasContent = true;
				}

				return builder.ToString();
			}
		}
	}
}
