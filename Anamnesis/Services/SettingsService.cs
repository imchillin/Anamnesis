// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis;
	using Anamnesis.Files;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Serialization;
	using MaterialDesignThemes.Wpf;

	public class SettingsService : ServiceBase<SettingsService>
	{
		private static string settingsPath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Settings.json");

		private string currentThemeSwatch = string.Empty;
		private bool? currentThemeDark = null;

		public static Settings Current => Instance.Settings!;

		public Settings? Settings { get; private set; }
		public bool FirstTimeUser { get; private set; }

		public static void ShowDirectory()
		{
			FileService.OpenDirectory(FileService.StoreDirectory);
		}

		public static void Save()
		{
			string json = SerializerService.Serialize(Instance.Settings!);
			File.WriteAllText(settingsPath, json);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			if (!File.Exists(settingsPath))
			{
				this.FirstTimeUser = true;
				this.Settings = new Settings();
				Save();
			}
			else
			{
				this.FirstTimeUser = false;
				try
				{
					string json = File.ReadAllText(settingsPath);
					this.Settings = SerializerService.Deserialize<Settings>(json);
				}
				catch (Exception)
				{
					await GenericDialog.Show("Failed to load Settings. Your settings have been reset.", "Error", MessageBoxButton.OK);
					this.Settings = new Settings();
					Save();
				}
			}

			this.Settings.PropertyChanged += this.SettingsChanged;
			this.SettingsChanged(null, null);
		}

		private void SettingsChanged(object? sender, PropertyChangedEventArgs? e)
		{
			if (this.Settings == null)
				return;

			if (this.currentThemeSwatch != this.Settings.ThemeSwatch || this.currentThemeDark != this.Settings.ThemeDark)
			{
				this.currentThemeSwatch = this.Settings.ThemeSwatch;
				this.currentThemeDark = this.Settings.ThemeDark;
				new PaletteHelper().Apply(this.Settings.ThemeSwatch, this.Settings.ThemeDark);
			}

			if (sender is Settings settings)
			{
				Save();
			}
		}
	}
}
