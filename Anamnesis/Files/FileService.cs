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

		public static DirectoryInfo DefaultPoseDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultPoseDirectory));
		public static DirectoryInfo DefaultCharacterDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultCharacterDirectory));
		public static DirectoryInfo DefaultSceneDirectory => new DirectoryInfo(ParseToFilePath(SettingsService.Current.DefaultSceneDirectory));
		public static DirectoryInfo BuiltInPoseDirectory => new DirectoryInfo(ParseToFilePath("%InstallDir%/Data/BuiltInPoses/"));
		public static DirectoryInfo CMToolSaveDir => new DirectoryInfo(ParseToFilePath("%MyDocuments%/CMTool/Matrix Saves/"));
		public static DirectoryInfo FFxivDatCharacterDirectory => new DirectoryInfo(ParseToFilePath("%MyDocuments%/My Games/FINAL FANTASY XIV - A Realm Reborn/"));

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

		public static async Task<OpenResult> Open<T>(params DirectoryInfo[] directories)
			where T : FileBase
		{
			return await Open(directories, typeof(T));
		}

		public static async Task<OpenResult> Open<T1, T2>(params DirectoryInfo[] directories)
			where T1 : FileBase
			where T2 : FileBase
		{
			return await Open(directories, typeof(T1), typeof(T2));
		}

		public static async Task<OpenResult> Open<T1, T2, T3>(params DirectoryInfo[] directories)
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
		{
			return await Open(directories, typeof(T1), typeof(T2), typeof(T3));
		}

		public static async Task<OpenResult> Open<T1, T2, T3, T4>(params DirectoryInfo[] directories)
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
			where T4 : FileBase
		{
			return await Open(directories, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		public static async Task<OpenResult> Open(DirectoryInfo[] directories, params Type[] fileTypes)
		{
			OpenResult result = default;

			try
			{
				bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

				if (!useExplorerBrowser)
				{
					HashSet<string> extensions = ToExtensions(fileTypes);
					FileBrowserView browser = new FileBrowserView(directories, extensions, string.Empty, FileBrowserView.Modes.Load);
					await ViewService.ShowDrawer(browser);

					while (browser.IsOpen)
						await Task.Delay(10);

					result.Path = browser.FilePath;
					useExplorerBrowser = browser.UseFileBrowser;
				}

				if (useExplorerBrowser)
				{
					result.Path = await App.Current.Dispatcher.InvokeAsync<string?>(() =>
					{
						OpenFileDialog dlg = new OpenFileDialog();
						dlg.Filter = ToAnyFilter(fileTypes);
						bool? result = dlg.ShowDialog();

						if (result != true)
							return null;

						return dlg.FileName;
					});
				}

				if (result.Path == null)
					return result;

				using FileStream stream = new FileStream(result.Path, FileMode.Open);
				string extension = Path.GetExtension(result.Path);

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

		public static Task<SaveResult> Save<T>(params DirectoryInfo[] directories)
			where T : FileBase
		{
			return Save<T>(null, directories);
		}

		public static async Task<SaveResult> Save<T>(string? defaultPath, params DirectoryInfo[] directories)
			where T : FileBase
		{
			SaveResult result = default;

			string? path = defaultPath;

			try
			{
				if (path == null)
				{
					bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

					string typeName = GetFileTypeName(typeof(T));

					if (!useExplorerBrowser)
					{
						HashSet<string> extensions = new HashSet<string>()
						{
							GetFileTypeExtension(typeof(T)),
						};

						FileBrowserView browser = new FileBrowserView(directories, extensions, typeName, FileBrowserView.Modes.Save);
						await ViewService.ShowDrawer(browser);

						while (browser.IsOpen)
							await Task.Delay(10);

						path = browser.FilePath;
						useExplorerBrowser = browser.UseFileBrowser;
					}

					if (useExplorerBrowser)
					{
						path = await App.Current.Dispatcher.InvokeAsync<string?>(() =>
						{
							SaveFileDialog dlg = new SaveFileDialog();
							dlg.Filter = ToFilter(typeof(T));
							bool? dlgResult = dlg.ShowDialog();

							if (dlgResult != true)
								return null;

							return dlg.FileName;
						});
					}

					if (path == null)
					{
						return result;
					}
				}

				result.Path = path + GetFileTypeExtension(typeof(T));

				if (File.Exists(result.Path))
				{
					string fileName = Path.GetFileNameWithoutExtension(result.Path);
					bool? overwrite = await GenericDialog.Show(LocalizationService.GetStringFormatted("FileBrowser_ReplaceMessage", fileName), LocalizationService.GetString("FileBrowser_ReplaceTitle"), MessageBoxButton.YesNo);
					if (overwrite != true)
					{
						return await Save<T>(defaultPath, directories);
					}
				}

				string? dir = Path.GetDirectoryName(path);
				if (dir == null)
					throw new Exception("No directory in save path");

				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
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
		public string? Path;
	}

	public struct SaveResult
	{
		public string? Path;
	}
}
