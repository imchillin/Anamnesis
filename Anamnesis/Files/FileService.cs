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
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Files.Infos;
	using Anamnesis.GUI.Views;
	using Anamnesis.Scenes;
	using Anamnesis.Services;
	using Microsoft.Win32;
	using Serilog;

	public class FileService : ServiceBase<FileService>
	{
		public static readonly List<FileInfoBase> FileInfos = new List<FileInfoBase>()
		{
			new CharacterFileInfo(),
			new PoseFileInfo(),
			new DatCharacterFileInfo(),
			new SceneFileInfo(),

			new LegacyCharacterFileInfo(),
			new LegacyJsonCharacterFileInfo(),
			new LegacyPoseFileInfo(),
		};

		public static readonly string StoreDirectory = "%AppData%/Anamnesis/";

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
			path = path.Replace($"%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
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

		public static async Task<OpenResult> Open<T>()
			where T : FileBase
		{
			return await Open(typeof(T));
		}

		public static async Task<OpenResult> Open<T1, T2>()
			where T1 : FileBase
			where T2 : FileBase
		{
			return await Open(typeof(T1), typeof(T2));
		}

		public static async Task<OpenResult> Open<T1, T2, T3>()
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
		{
			return await Open(typeof(T1), typeof(T2), typeof(T3));
		}

		public static async Task<OpenResult> Open<T1, T2, T3, T4>()
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
			where T4 : FileBase
		{
			return await Open(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		public static Task<OpenResult> Open(params Type[] fileTypes)
		{
			List<FileInfoBase>? fileInfos = new List<FileInfoBase>();
			foreach (Type fileType in fileTypes)
			{
				fileInfos.AddRange(GetFileInfos(fileType));
			}

			return Open(fileInfos.ToArray());
		}

		public static async Task<OpenResult> Open(params FileInfoBase[] fileInfos)
		{
			OpenResult result = default;

			try
			{
				bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

				if (!useExplorerBrowser)
				{
					FileBrowserView browser = new FileBrowserView(fileInfos, FileBrowserView.Modes.Load);
					await ViewService.ShowDrawer(browser);

					while (browser.IsOpen)
						await Task.Delay(10);

					result.Path = browser.FilePath;
					result.Options = browser.OptionsControl;
					useExplorerBrowser = browser.UseFileBrowser;
				}

				if (useExplorerBrowser)
				{
					result.Path = await App.Current.Dispatcher.InvokeAsync<string?>(() =>
					{
						OpenFileDialog dlg = new OpenFileDialog();
						dlg.Filter = ToAnyFilter(fileInfos);
						bool? result = dlg.ShowDialog();

						if (result != true)
							return null;

						return dlg.FileName;
					});
				}

				if (result.Path == null)
					return result;

				string extension = Path.GetExtension(result.Path);
				result.Info = GetFileInfo(extension);

				using FileStream stream = new FileStream(result.Path, FileMode.Open);
				result.File = result.Info.DeserializeFile(stream);

				if (result.File == null)
					throw new Exception("File failed to deserialize");

				return result;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to open file");
			}

			return result;
		}

		public static async Task<SaveResult> Save<T>(string? path = null)
			where T : FileBase
		{
			SaveResult result = default;

			try
			{
				result.Info = GetFileInfo<T>();

				if (path == null)
				{
					bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

					if (!useExplorerBrowser)
					{
						FileBrowserView browser = new FileBrowserView(result.Info, FileBrowserView.Modes.Save);
						await ViewService.ShowDrawer(browser);

						while (browser.IsOpen)
							await Task.Delay(10);

						path = browser.FilePath;
						useExplorerBrowser = browser.UseFileBrowser;
						result.Options = browser.OptionsControl;
					}

					if (useExplorerBrowser)
					{
						path = await App.Current.Dispatcher.InvokeAsync<string?>(() =>
						{
							SaveFileDialog dlg = new SaveFileDialog();
							dlg.Filter = ToFilter(result.Info);
							bool? dlgResult = dlg.ShowDialog();

							if (dlgResult != true)
								return null;

							string? dirName = Path.GetDirectoryName(dlg.FileName);

							if (dirName == null)
								throw new Exception("Failed to parse file name: " + dlg.FileName);

							return Path.Combine(dirName, Path.GetFileNameWithoutExtension(dlg.FileName));
						});
					}

					if (path == null)
					{
						return result;
					}
				}

				path += "." + result.Info.Extension;
				result.Path = path;

				////using FileStream stream = new FileStream(path, FileMode.Create);
				////info.SerializeFile(file, stream);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save file");
			}

			return result;
		}

		public static FileInfoBase GetFileInfo<T>()
			where T : FileBase
		{
			List<FileInfoBase> results = GetFileInfos(typeof(T));

			if (results.Count > 1)
				Log.Warning($"Multiple file infos found for file: {typeof(T)}. Using first.");

			if (results.Count <= 0)
				throw new Exception($"No file info for file: {typeof(T)}");

			return results[0];
		}

		public static FileInfoBase GetFileInfo(FileBase file)
		{
			List<FileInfoBase> results = GetFileInfos(file.GetType());

			if (results.Count > 1)
				Log.Warning($"Multiple file infos found for file: {file}. Using first.");

			if (results.Count <= 0)
				throw new Exception($"No file info for file: {file}");

			return results[0];
		}

		public static List<FileInfoBase> GetFileInfos(Type type)
		{
			List<FileInfoBase> results = new List<FileInfoBase>();
			foreach (FileInfoBase fileInfo in FileInfos)
			{
				if (fileInfo.IsFile(type))
				{
					results.Add(fileInfo);
				}
			}

			return results;
		}

		public static FileInfoBase GetFileInfo(string extension)
		{
			if (extension.StartsWith("."))
				extension = extension.Substring(1, extension.Length - 1);

			foreach (FileInfoBase fileInfo in FileInfos)
			{
				if (fileInfo.Extension == extension)
				{
					return fileInfo;
				}
			}

			throw new Exception($"Unable to determine file info from extension: \"{extension}\"");
		}

		private static string ToAnyFilter(params FileInfoBase[] infos)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Any|");

			foreach (FileInfoBase type in infos)
				builder.Append("*." + type.Extension + ";");

			foreach (FileInfoBase type in infos)
			{
				builder.Append("|");
				builder.Append(type.Name);
				builder.Append("|");
				builder.Append("*." + type.Extension);
			}

			return builder.ToString();
		}

		private static string ToFilter(FileInfoBase info)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(info.Name);
			builder.Append("|");
			builder.Append("*." + info.Extension);
			return builder.ToString();
		}
	}

	#pragma warning disable SA1201, SA1402
	public struct OpenResult
	{
		public FileBase? File;
		public FileInfoBase? Info;
		public string? Path;
		public UserControl? Options;
	}

	public struct SaveResult
	{
		public string? Path;
		public UserControl? Options;
		public FileInfoBase? Info;
	}

	public interface IFileSource
	{
		public interface IEntry
		{
			public string? Path { get; }
			public string? Name { get; }
			public IFileSource Source { get; }
			public bool Exists { get; }
			public string? Metadata { get; }

			public Task Delete();
			public Task Rename(string newName);
		}

		public interface IFile : IEntry
		{
			public FileInfoBase? Type { get; }
		}

		public interface IDirectory : IEntry
		{
			public IDirectory CreateSubDirectory();
		}

		public bool CanRead { get; }
		public bool CanWrite { get; }
		public string Name { get; }

		public IDirectory GetDefaultDirectory();
		public Task<IEnumerable<IEntry>> GetEntries(IDirectory current, bool recursive, FileInfoBase[] fileTypes);
	}
}
