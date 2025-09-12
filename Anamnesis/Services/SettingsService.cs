// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Serialization;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf;

/// <summary>
/// A service that manages application settings, including theme and other user preferences.
/// </summary>
public class SettingsService : ServiceBase<SettingsService>
{
	private static readonly string s_settingsPath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Settings.json");

	/// <summary>
	/// Event that is raised when the settings have changed.
	/// </summary>
	public static event PropertyChangedEventHandler? SettingsChanged;

	/// <summary>
	/// Gets the current settings stored in the singleton instance of the service.
	/// </summary>
	public static Settings Current => Instance.Settings!;

	/// <summary>
	/// Gets the current settings for the application.
	/// </summary>
	public Settings? Settings { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the user is using the application for the first time.
	/// </summary>
	public bool FirstTimeUser { get; private set; }

	/// <summary>
	/// Opens the settings directory in Windows Explorer.
	/// </summary>
	public static void ShowDirectory()
	{
		FileService.OpenDirectory(FileService.StoreDirectory);
	}

	/// <summary>
	/// Saves the current settings to the local file system.
	/// </summary>
	public static void Save()
	{
		string json = SerializerService.Serialize(Instance.Settings!);
		File.WriteAllText(s_settingsPath, json);
	}

	/// <summary>
	/// Applies the current theme settings to the application.
	/// </summary>
	public static void ApplyTheme()
	{
		if (Current.OverrideSystemTheme)
		{
			Themes.ApplyCustomTheme(Current.ThemeLight, Current.ThemeTrimColor);
		}
		else
		{
			Themes.ApplySystemTheme();
		}
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		await base.Initialize();

		if (!File.Exists(s_settingsPath))
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
				await Dispatch.MainThread();

				if (Keyboard.IsKeyDown(Key.LeftShift))
					throw new Exception("User Abort");

				string json = File.ReadAllText(s_settingsPath);
				this.Settings = SerializerService.Deserialize<Settings>(json);

				if (SystemParameters.WindowGlassColor == Colors.Black)
				{
					this.Settings.OverrideSystemTheme = true;
					this.Settings.ThemeTrimColor = Colors.Pink;
				}
			}
			catch (Exception ex)
			{
				Log.Warning(ex, $"Failed to load settings:\n{ex.StackTrace}");
				await GenericDialog.ShowAsync("Failed to load Settings. Your settings have been reset.", "Error", MessageBoxButton.OK);
				this.FirstTimeUser = true;
				this.Settings = new Settings();
				Save();
			}
		}

		this.Settings.PropertyChanged += this.OnSettingsChanged;
		this.OnSettingsChanged(null, new PropertyChangedEventArgs(null));
	}

	private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.Settings == null)
			return;

		if (sender is Settings settings)
		{
			Save();
		}

		ApplyTheme();
		SettingsChanged?.Invoke(sender, e);
	}
}
