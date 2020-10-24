// Concept Matrix 3.
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
		private static string settingsPath = FileService.StoreDirectory + "/Settings.json";

		private string currentThemeSwatch = string.Empty;
		private bool? currentThemeDark = null;

		public static Settings Current => Instance.Settings!;

		public Settings? Settings { get; private set; }

		public static void ShowDirectory()
		{
			string? dir = Path.GetDirectoryName(FileService.StoreDirectory);
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", dir);
		}

		public static void Save()
		{
			string json = SerializerService.Serialize(Instance.Settings!);
			File.WriteAllText(settingsPath, json);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			string path = FileService.StoreDirectory;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = FileService.StoreDirectory + "/Settings.json";

			if (!File.Exists(path))
			{
				this.Settings = new Settings();
				Save();
			}
			else
			{
				try
				{
					string json = File.ReadAllText(path);
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
