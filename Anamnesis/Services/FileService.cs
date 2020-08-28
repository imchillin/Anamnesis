// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Files;
	using Anamnesis.GUI.Views;
	using Microsoft.Win32;

	public class FileService : IFileService
	{
		private static ISerializerService serializer = Services.Get<ISerializerService>();
		private List<IFileSource> fileSources = new List<IFileSource>();

		public static string StoreDirectory
		{
			get
			{
				string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				dir += "/Concept Matrix 3/";
				return dir;
			}
		}

		public Task Initialize()
		{
			this.AddFileSource(new LocalFileSource());
			this.AddFileSource(new LegacyFileSource());

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public void AddFileSource(IFileSource source)
		{
			this.fileSources.Add(source);
		}

		public async Task<T?> Open<T>(FileType fileType, string? path)
			where T : FileBase
		{
			try
			{
				if (path == null)
					return null;

				if (!path.EndsWith(fileType.Extension))
					path = path + "." + fileType.Extension;

				if (path == null)
					return null;

				FileBase file = await Open(path, fileType);
				if (file is T tFile)
					return tFile;

				throw new Exception("file loaded was incorrect type");
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to open file", ex), "Files", Log.Severity.Error);
			}

			return null;
		}

		public async Task<FileBase?> OpenAny(params FileType[] fileTypes)
		{
			try
			{
				string? path = null;
				bool advancedMode = false;

				bool useExplorerBrowser = App.Settings.UseWindowsExplorer;

				if (!useExplorerBrowser)
				{
					FileBrowserView browser = new FileBrowserView(this.fileSources, fileTypes, FileBrowserView.Modes.Load);
					await App.Services.Get<IViewService>().ShowDrawer(browser);

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
						dlg.Filter = ToAnyFilter(fileTypes);
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
				FileType type = GetFileType(fileTypes, extension);

				FileBase file = await Open(path, type);
				file.UseAdvancedLoad = advancedMode;
				return file;
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to open file", ex), "Files", Log.Severity.Error);
			}

			return null;
		}

		public async Task Save(Func<bool, Task<FileBase>> writeFile, FileType type, string? path = null)
		{
			try
			{
				bool advancedMode = false;

				if (path == null)
				{
					bool useExplorerBrowser = App.Settings.UseWindowsExplorer;

					if (!useExplorerBrowser)
					{
						List<FileType> fileTypes = new List<FileType>();
						fileTypes.Add(type);
						FileBrowserView browser = new FileBrowserView(this.fileSources, fileTypes.ToArray(), FileBrowserView.Modes.Save);
						await App.Services.Get<IViewService>().ShowDrawer(browser);

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
							dlg.Filter = ToFilter(type);
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

				path += "." + type.Extension;

				FileBase file = await writeFile.Invoke(advancedMode);

				using FileStream stream = new FileStream(path, FileMode.Create);
				if (type.Serialize != null)
				{
					type.Serialize.Invoke(stream, file);
				}
				else
				{
					using TextWriter writer = new StreamWriter(stream);
					string json = serializer.Serialize(file);
					writer.Write(json);
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to save file", ex), "Files", Log.Severity.Error);
			}
		}

		public Task<string> OpenDirectory(string title, params string[] defaults)
		{
			string defaultDir;
			foreach (string pDefaultDir in defaults)
			{
				if (Directory.Exists(pDefaultDir))
				{
					defaultDir = pDefaultDir;
					break;
				}
			}

			throw new NotImplementedException();

			/*return await App.Current.Dispatcher.InvokeAsync<string>(() =>
			{
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.IsFolderPicker = true;
				dlg.Title = title;
				dlg.DefaultDirectory = defaultDir;
				bool? result = dlg.ShowDialog();

				if (result != true)
					return null;

				return dlg.FileName;
			});*/
		}

		private static Task<FileBase> Open(string path, FileType type)
		{
			FileBase file;
			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				if (type.Deserialize != null)
				{
					file = type.Deserialize.Invoke(stream);
				}
				else
				{
					using TextReader reader = new StreamReader(stream);
					string json = reader.ReadToEnd();
					file = (FileBase)serializer.Deserialize(json, type.Type);
				}
			}

			if (file == null)
				throw new Exception("File failed to deserialize");

			file.Path = path;

			return Task.FromResult<FileBase>(file);
		}

		private static FileType GetFileType(FileType[] fileTypes, string extension)
		{
			if (extension.StartsWith("."))
				extension = extension.Substring(1, extension.Length - 1);

			foreach (FileType fileType in fileTypes)
			{
				if (fileType.Extension == extension)
				{
					return fileType;
				}
			}

			throw new Exception($"Unable to determine file type from extension: \"{extension}\"");
		}

		private static string ToAnyFilter(params FileType[] types)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Any|");

			foreach (FileType type in types)
				builder.Append("*." + type.Extension + ";");

			foreach (FileType type in types)
			{
				builder.Append("|");
				builder.Append(type.Name);
				builder.Append("|");
				builder.Append("*." + type.Extension);
			}

			return builder.ToString();
		}

		private static string ToFilter(FileType fileType)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(fileType.Name);
			builder.Append("|");
			builder.Append("*." + fileType.Extension);
			return builder.ToString();
		}
	}
}
