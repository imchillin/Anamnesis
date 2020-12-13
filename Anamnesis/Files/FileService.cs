// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Files.Infos;
	using Anamnesis.GUI.Views;
	using Anamnesis.Scenes;
	using Anamnesis.Services;
	using Microsoft.Win32;
	using SimpleLog;

	public class FileService : ServiceBase<FileService>
	{
		public static readonly List<FileInfoBase> FileInfos = new List<FileInfoBase>()
		{
			new CharacterFileInfo(),
			new PoseFileInfo(),
			new DatCharacterFileInfo(),
			new SceneFileInfo(),

			new LegacyCharacterFileInfo(),
			new LegacyEquipmentSetFileInfo(),
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
				fileInfos.Add(GetFileInfo(fileType));
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
				Log.Write(Severity.Error, new Exception("Failed to open file", ex));
			}

			return result;
		}

		public static async Task Save<T>(T file, string? path = null)
			where T : FileBase, new()
		{
			try
			{
				FileInfoBase info = GetFileInfo<T>();

				if (path == null)
				{
					bool useExplorerBrowser = SettingsService.Current.UseWindowsExplorer;

					if (!useExplorerBrowser)
					{
						FileBrowserView browser = new FileBrowserView(info, FileBrowserView.Modes.Save);
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
							dlg.Filter = ToFilter(info);
							bool? result = dlg.ShowDialog();

							if (result != true)
								return null;

							string? dirName = Path.GetDirectoryName(dlg.FileName);

							if (dirName == null)
								throw new Exception("Failed to parse file name: " + dlg.FileName);

							return Path.Combine(dirName, Path.GetFileNameWithoutExtension(dlg.FileName));
						});
					}

					if (path == null)
					{
						return;
					}
				}

				path += "." + info.Extension;

				using FileStream stream = new FileStream(path, FileMode.Create);
				info.SerializeFile(file, stream);
			}
			catch (Exception ex)
			{
				Log.Write(Severity.Error, new Exception("Failed to save file", ex));
			}
		}

		public static FileInfoBase GetFileInfo<T>()
			where T : FileBase
		{
			return GetFileInfo(typeof(T));
		}

		public static FileInfoBase GetFileInfo(FileBase file)
		{
			return GetFileInfo(file.GetType());
		}

		public static FileInfoBase GetFileInfo(Type type)
		{
			foreach (FileInfoBase fileInfo in FileInfos)
			{
				if (fileInfo.IsFile(type))
				{
					return fileInfo;
				}
			}

			throw new Exception($"No file Info for file type: {type}");
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
	}

	public interface IFileSource
	{
		public interface IEntry
		{
			public string? Path { get; }
			public string? Name { get; }
			public IFileSource Source { get; }

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

		public string Name { get; }

		public IDirectory GetDefaultDirectory();
		public Task<IEnumerable<IEntry>> GetEntries(IDirectory current, bool recursive, FileInfoBase[] fileTypes);
	}
}
