// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Anamnesis;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.GUI.Views;
	using Anamnesis.Services;
	using Microsoft.Win32;

	public class FileService : ServiceBase<FileService>
	{
		public static readonly string StoreDirectory = "%AppData%/Anamnesis/";

		private static readonly Dictionary<Type, string> TypeNameLookup = new Dictionary<Type, string>();
		private static readonly Dictionary<Type, string> TypeExtensionLookup = new Dictionary<Type, string>();

		public static Shortcut Desktop => new Shortcut(
			new DirectoryInfo(ParseToFilePath("%Desktop%")),
			"Shortcuts/Desktop.png",
			"Shortcut_Desktop");

		public static Shortcut DefaultPoseDirectory => new Shortcut(
			new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultPoseDirectory)),
			"Shortcuts/Anamnesis.png",
			"Shortcut_AnamnesisPose");

		public static Shortcut DefaultCharacterDirectory => new Shortcut(
			new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultCharacterDirectory)),
			"Shortcuts/Anamnesis.png",
			"Shortcut_AnamnesisCharacter");

		public static Shortcut DefaultSceneDirectory => new Shortcut(
			new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultSceneDirectory)),
			"Shortcuts/Anamnesis.png",
			"Shortcut_AnamnesisScenes");

		public static Shortcut StandardPoseDirectory => new Shortcut(
			new DirectoryInfo(ParseToFilePath("%AppData%/Anamnesis/StandardPoses/")),
			"Shortcuts/AnamnesisBuiltIn.png",
			"Shortcut_BuiltInPose");

		public static Shortcut CMToolPoseSaveDir => new Shortcut(
			new DirectoryInfo(ParseToFilePath("%MyDocuments%/CMTool/Matrix Saves/")),
			"Shortcuts/cmtool.png",
			"Shortcut_CMToolPose");

		public static Shortcut CMToolAppearanceSaveDir => new Shortcut(
			new DirectoryInfo(ParseToFilePath("%MyDocuments%/CMTool/")),
			"Shortcuts/cmtool.png",
			"Shortcut_CMToolPose");

		public static Shortcut FFxivDatCharacterDirectory => new Shortcut(
			new DirectoryInfo(ParseToFilePath("%MyDocuments%/My Games/FINAL FANTASY XIV - A Realm Reborn/")),
			"Shortcuts/ffxiv.png",
			"Shortcut_FfxivAppearance");

		/// <summary>
		/// Replaces special folders (%ApplicationData%) with the actual path.
		/// </summary>
		public static string ParseToFilePath(string path)
		{
			foreach (Environment.SpecialFolder? specialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
			{
				if (specialFolder == null)
					continue;

				path = path.Replace($"%{specialFolder}%", Environment.GetFolderPath((Environment.SpecialFolder)specialFolder));
			}

			// Special case for "AppData" instead of "ApplicationData"
			path = path.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			path = path.Replace("%InstallDir%", Directory.GetCurrentDirectory());
			path = path.Replace('/', '\\');
			return path;
		}

		/// <summary>
		/// Replaces special fodler paths with special fodler notation (%appdata%).
		/// </summary>
		public static string ParseFromFilePath(string path)
		{
			// Special case for "AppData" instead of "ApplicationData"
			path = path.Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"%AppData%");

			foreach (Environment.SpecialFolder? specialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
			{
				if (specialFolder == null)
					continue;

				string specialPath = Environment.GetFolderPath((Environment.SpecialFolder)specialFolder);

				if (string.IsNullOrEmpty(specialPath))
					continue;

				path = path.Replace(specialPath, $"%{specialFolder}%");
			}

			path = path.Replace('\\', '/');
			return path;
		}

		public static void OpenDirectory(string path)
		{
			string dir = FileService.ParseToFilePath(path);
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", $"\"{dir}\"");
		}

		public static async Task<OpenResult> Open<T>(DirectoryInfo? defaultDirectory, params Shortcut[] shortcuts)
			where T : FileBase
		{
			return await Open(defaultDirectory, shortcuts, typeof(T));
		}

		public static async Task<OpenResult> Open<T1, T2>(DirectoryInfo? defaultDirectory, params Shortcut[] shortcuts)
			where T1 : FileBase
			where T2 : FileBase
		{
			return await Open(defaultDirectory, shortcuts, typeof(T1), typeof(T2));
		}

		public static async Task<OpenResult> Open<T1, T2, T3>(DirectoryInfo? defaultDirectory, params Shortcut[] shortcuts)
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
		{
			return await Open(defaultDirectory, shortcuts, typeof(T1), typeof(T2), typeof(T3));
		}

		public static async Task<OpenResult> Open<T1, T2, T3, T4>(DirectoryInfo? defaultDirectory, params Shortcut[] shortcuts)
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
			where T4 : FileBase
		{
			return await Open(defaultDirectory, shortcuts, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		public static async Task<OpenResult> Open(DirectoryInfo? defaultDirectory, Shortcut[] shortcuts, params Type[] fileTypes)
		{
			OpenResult result = default;

			try
			{
				bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

				if (!useExplorerBrowser)
				{
					HashSet<string> extensions = ToExtensions(fileTypes);
					FileBrowserView browser = new FileBrowserView(shortcuts, extensions, defaultDirectory, null, FileBrowserView.Modes.Load);
					await ViewService.ShowDrawer(browser);

					while (browser.IsOpen)
						await Task.Delay(10);

					result.Path = browser.FinalSelection as FileInfo;
					useExplorerBrowser = browser.UseFileBrowser;
				}

				if (useExplorerBrowser)
				{
					result.Path = await App.Current.Dispatcher.InvokeAsync<FileInfo?>(() =>
					{
						OpenFileDialog dlg = new OpenFileDialog();

						if (defaultDirectory == null)
						{
							Shortcut? defaultShortcut = null;
							foreach (Shortcut shortcut in shortcuts)
							{
								if (defaultDirectory == null && shortcut.Directory.Exists && defaultShortcut == null)
								{
									defaultDirectory = shortcut.Directory;
								}
							}
						}

						if (defaultDirectory != null)
							dlg.InitialDirectory = defaultDirectory.FullName;

						foreach (Shortcut? shortcut in shortcuts)
						{
							dlg.CustomPlaces.Add(new FileDialogCustomPlace(ParseToFilePath(shortcut.Path)));
						}

						dlg.Filter = ToAnyFilter(fileTypes);
						bool? result = dlg.ShowDialog();

						if (result != true)
							return null;

						return new FileInfo(dlg.FileName);
					});
				}

				if (result.Path == null)
					return result;

				using FileStream stream = new FileStream(result.Path.FullName, FileMode.Open);
				string extension = Path.GetExtension(result.Path.FullName);

				Exception? lastException = null;
				foreach (Type fileType in fileTypes)
				{
					string typeExtension = GetFileTypeExtension(fileType);

					if (typeExtension == extension)
					{
						try
						{
							FileBase? file = Activator.CreateInstance(fileType) as FileBase;

							if (file == null)
								throw new Exception($"Failed to create instance of file type: {fileType}");

							result.File = file.Deserialize(stream);
						}
						catch (Exception ex)
						{
							Log.Verbose(ex, $"Attempted to deserialize file: {result.Path} as type: {fileType} failed.");
							lastException = ex;
						}
					}
				}

				if (result.File == null)
				{
					if (lastException == null)
						throw new Exception($"Unrecognised file: {result.Path}");

					throw lastException;
				}

				return result;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to open file");
			}

			return result;
		}

		public static async Task<SaveResult> Save<T>(DirectoryInfo? defaultDirectory, params Shortcut[] directories)
			where T : FileBase
		{
			SaveResult result = default;
			////result.Path = defaultPath;

			string ext = GetFileTypeExtension(typeof(T));

			try
			{
				bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

				string typeName = GetFileTypeName(typeof(T));

				if (!useExplorerBrowser)
				{
					HashSet<string> extensions = new HashSet<string>()
					{
						GetFileTypeExtension(typeof(T)),
					};

					FileBrowserView browser = new FileBrowserView(directories, extensions, defaultDirectory, typeName, FileBrowserView.Modes.Save);
					await ViewService.ShowDrawer(browser);

					while (browser.IsOpen)
						await Task.Delay(10);

					result.Path = browser.FinalSelection as FileInfo;
					useExplorerBrowser = browser.UseFileBrowser;
				}

				if (useExplorerBrowser)
				{
					result.Path = await App.Current.Dispatcher.InvokeAsync<FileInfo?>(() =>
					{
						SaveFileDialog dlg = new SaveFileDialog();
						dlg.Filter = ToFilter(typeof(T));
						dlg.InitialDirectory = defaultDirectory?.FullName ?? string.Empty;
						bool? dlgResult = dlg.ShowDialog();

						if (dlgResult != true)
							return null;

						return new FileInfo(dlg.FileName);
					});
				}

				if (result.Path == null)
					return result;

				if (result.Path.Exists)
				{
					string fileName = Path.GetFileNameWithoutExtension(result.Path.FullName);
					bool? overwrite = await GenericDialog.Show(LocalizationService.GetStringFormatted("FileBrowser_ReplaceMessage", fileName), LocalizationService.GetString("FileBrowser_ReplaceTitle"), MessageBoxButton.YesNo);
					if (overwrite != true)
					{
						return await Save<T>(defaultDirectory, directories);
					}
				}

				DirectoryInfo? dir = result.Path.Directory;
				if (dir == null)
					throw new Exception("No directory in save path");

				if (!dir.Exists)
				{
					dir.Create();
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save file");
			}

			return result;
		}

		private static string ToAnyFilter(params Type[] types)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Any|");

			foreach (Type type in types)
				builder.Append("*" + GetFileTypeExtension(type) + ";");

			foreach (Type type in types)
			{
				builder.Append("|");
				builder.Append(GetFileTypeName(type));
				builder.Append("|");
				builder.Append("*" + GetFileTypeExtension(type));
			}

			return builder.ToString();
		}

		private static string ToFilter(Type type)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(GetFileTypeName(type));
			builder.Append("|");
			builder.Append("*" + GetFileTypeExtension(type));
			return builder.ToString();
		}

		private static HashSet<string> ToExtensions(params Type[] types)
		{
			HashSet<string> results = new();

			foreach (Type type in types)
				results.Add(GetFileTypeExtension(type));

			return results;
		}

		private static string GetFileTypeName(Type fileType)
		{
			string? name;
			if (!TypeNameLookup.TryGetValue(fileType, out name))
			{
				FileBase? file = Activator.CreateInstance(fileType) as FileBase;

				if (file == null)
					throw new Exception($"Failed to create instance of file type: {fileType}");

				name = file.TypeName;
				TypeNameLookup.Add(fileType, name);
			}

			return name;
		}

		private static string GetFileTypeExtension(Type fileType)
		{
			string? name;
			if (!TypeExtensionLookup.TryGetValue(fileType, out name))
			{
				FileBase? file = Activator.CreateInstance(fileType) as FileBase;

				if (file == null)
					throw new Exception($"Failed to create instance of file type: {fileType}");

				name = file.FileExtension;
				TypeExtensionLookup.Add(fileType, name);
			}

			return name;
		}
	}

	#pragma warning disable SA1201, SA1402
	public struct OpenResult
	{
		public FileBase? File;
		public FileInfo? Path;

		public DirectoryInfo? Directory => this.Path?.Directory;
	}

	public struct SaveResult
	{
		public FileInfo? Path;

		public DirectoryInfo? Directory => this.Path?.Directory;
	}

	public class Shortcut
	{
		public DirectoryInfo Directory { get; private set; }
		public ImageSource? Icon { get; private set; }
		public string LabelKey { get; private set; }

		public string Path => FileService.ParseFromFilePath(this.Directory.FullName);

		public Shortcut(DirectoryInfo dir, string icon, string key)
		{
			this.Directory = dir;
			this.LabelKey = key;

			if (!string.IsNullOrEmpty(icon))
			{
				BitmapImage newIcon = new BitmapImage(new Uri($"pack://application:,,,/Anamnesis;component/Assets/{icon}"));
				newIcon.Freeze();
				this.Icon = newIcon;
			}
		}
	}
}
