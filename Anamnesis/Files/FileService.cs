// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Files.Infos;
	using Anamnesis.GUI.Views;
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

			new LegacyCharacterFileInfo(),
			new LegacyEquipmentSetFileInfo(),
			new LegacyPoseFileInfo(),
		};

		public static string StoreDirectory
		{
			get
			{
				string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				dir += "/Anamnesis/";
				return dir;
			}
		}

		public static async Task<T?> Open<T>()
			where T : FileBase
		{
			FileBase? file = await Open(typeof(T));
			if (file is T tFile)
				return tFile;

			throw new Exception("Faield to open file as expected type");
		}

		public static async Task<FileBase?> Open<T1, T2>()
			where T1 : FileBase
			where T2 : FileBase
		{
			return await Open(typeof(T1), typeof(T2));
		}

		public static async Task<FileBase?> Open<T1, T2, T3>()
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
		{
			return await Open(typeof(T1), typeof(T2), typeof(T3));
		}

		public static async Task<FileBase?> Open<T1, T2, T3, T4>()
			where T1 : FileBase
			where T2 : FileBase
			where T3 : FileBase
			where T4 : FileBase
		{
			return await Open(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		public static Task<FileBase?> Open(params Type[] fileTypes)
		{
			List<FileInfoBase>? fileInfos = new List<FileInfoBase>();
			foreach (Type fileType in fileTypes)
			{
				fileInfos.Add(GetFileInfo(fileType));
			}

			return Open(fileInfos.ToArray());
		}

		public static async Task<FileBase?> Open(params FileInfoBase[] fileInfos)
		{
			try
			{
				string? path = null;
				bool advancedMode = false;

				bool useExplorerBrowser = App.Settings.UseWindowsExplorer;

				if (!useExplorerBrowser)
				{
					FileBrowserView browser = new FileBrowserView(fileInfos, FileBrowserView.Modes.Load);
					await ViewService.ShowDrawer(browser);

					while (browser.IsOpen)
						await Task.Delay(10);

					path = browser.FilePath;
					advancedMode = browser.AdvancedMode;
					useExplorerBrowser = browser.UseFileBrowser;
				}

				if (useExplorerBrowser)
				{
					path = await App.Current.Dispatcher.InvokeAsync<string?>(() =>
					{
						OpenFileDialog dlg = new OpenFileDialog();
						dlg.Filter = ToAnyFilter(fileInfos);
						bool? result = dlg.ShowDialog();

						if (result != true)
							return null;

						return dlg.FileName;
					});

					advancedMode = true;
				}

				if (path == null)
					return null;

				string extension = Path.GetExtension(path);
				FileInfoBase info = GetFileInfo(extension);

				using FileStream stream = new FileStream(path, FileMode.Open);
				FileBase file = info.DeserializeFile(stream);

				if (file == null)
					throw new Exception("File failed to deserialize");

				file.Path = path;

				return file;
			}
			catch (Exception ex)
			{
				Log.Write(Severity.Error, new Exception("Failed to open file", ex));
			}

			return null;
		}

		public static async Task Save<T>(Func<bool, Task<T?>> writeFile, string? path = null)
			where T : FileBase, new()
		{
			try
			{
				bool advancedMode = false;

				FileInfoBase info = GetFileInfo<T>();

				if (path == null)
				{
					bool useExplorerBrowser = App.Settings.UseWindowsExplorer;

					if (!useExplorerBrowser)
					{
						FileBrowserView browser = new FileBrowserView(info, FileBrowserView.Modes.Save);
						await ViewService.ShowDrawer(browser);

						while (browser.IsOpen)
							await Task.Delay(10);

						path = browser.FilePath;
						advancedMode = browser.AdvancedMode;
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

				FileBase? file = await writeFile.Invoke(advancedMode);

				if (file == null)
					return;

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
	public interface IFileSource
	{
		public interface IEntry
		{
			public string? Path { get; }
			public string? Name { get; }

			public Task Delete();
		}

		public interface IFile : IEntry
		{
			public FileInfoBase? Type { get; }
		}

		public interface IDirectory : IEntry
		{
		}

		public string Name { get; }

		public IDirectory GetDefaultDirectory();
		public Task<IEnumerable<IEntry>> GetEntries(IDirectory current, bool recursive);
	}
}
