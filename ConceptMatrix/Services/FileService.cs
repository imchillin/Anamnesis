// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Serialization;
	using ConceptMatrix.Services;
	using Microsoft.WindowsAPICodePack.Dialogs;
	using Newtonsoft.Json;

	public class FileService : IFileService
	{
		public Task Initialize(IServices services)
		{
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

		public async Task<T> Open<T>(FileType fileType)
			where T : FileBase
		{
			try
			{
				string path = await App.Current.Dispatcher.InvokeAsync<string>(() =>
				{
					CommonOpenFileDialog dlg = new CommonOpenFileDialog();
					dlg.Filters.Add(ToFilter(fileType));
					CommonFileDialogResult selected = dlg.ShowDialog();

					if (selected != CommonFileDialogResult.Ok)
						return null;

					return dlg.FileName;
				});

				if (path == null)
					return null;

				FileBase file = await Open(path, fileType);
				if (file is T tFile)
					return tFile;

				throw new Exception("file loaded was incorrect type");
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to open file", ex));
			}

			return null;
		}

		public async Task<FileBase> OpenAny(params FileType[] fileTypes)
		{
			try
			{
				string path = await App.Current.Dispatcher.InvokeAsync<string>(() =>
				{
					CommonOpenFileDialog dlg = new CommonOpenFileDialog();
					dlg.Filters.Add(ToAnyFilter(fileTypes));

					foreach (FileType fileType in fileTypes)
						dlg.Filters.Add(ToFilter(fileType));

					CommonFileDialogResult selected = dlg.ShowDialog();

					if (selected != CommonFileDialogResult.Ok)
						return null;

					return dlg.FileName;
				});

				if (path == null)
					return null;

				string extension = Path.GetExtension(path);
				FileType type = GetFileType(fileTypes, extension);

				return await Open(path, type);
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to open file", ex));
			}

			return null;
		}

		public async Task Save(FileBase file)
		{
			try
			{
				FileType type = file.GetFileType();

				string path = file.Path;

				if (path == null)
				{
					path = await App.Current.Dispatcher.InvokeAsync<string>(() =>
					{
						CommonSaveFileDialog dlg = new CommonSaveFileDialog();
						dlg.Filters.Add(ToFilter(type));
						CommonFileDialogResult selected = dlg.ShowDialog();

						if (selected != CommonFileDialogResult.Ok)
							return null;

						return Path.GetFileNameWithoutExtension(dlg.FileName);
					});

					if (path == null)
					{
						return;
					}
				}

				path += "." + type.Extension;

				using (FileStream stream = new FileStream(path, FileMode.Create))
				{
					if (type.Serialize != null)
					{
						type.Serialize.Invoke(stream, file);
					}
					else
					{
						using (TextWriter writer = new StreamWriter(stream))
						{
							string json = Serializer.Serialize(file);
							writer.Write(json);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to save file", ex));
			}
		}

		public Task SaveAs(FileBase file)
		{
			file.Path = null;
			return this.Save(file);
		}

		public async Task<string> OpenDirectory(string title, params string[] defaults)
		{
			string defaultDir = null;
			foreach (string pDefaultDir in defaults)
			{
				if (Directory.Exists(pDefaultDir))
				{
					defaultDir = pDefaultDir;
					break;
				}
			}

			return await App.Current.Dispatcher.InvokeAsync<string>(() =>
			{
				CommonOpenFileDialog dlg = new CommonOpenFileDialog();
				dlg.IsFolderPicker = true;
				dlg.Title = title;
				dlg.DefaultDirectory = defaultDir;
				CommonFileDialogResult selected = dlg.ShowDialog();

				if (selected != CommonFileDialogResult.Ok)
					return null;

				return dlg.FileName;
			});
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
					using (TextReader reader = new StreamReader(stream))
					{
						string json = reader.ReadToEnd();
						file = (FileBase)Serializer.Deserialize(json, type.Type);
					}
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

		private static CommonFileDialogFilter ToAnyFilter(params FileType[] types)
		{
			CommonFileDialogFilter filter = new CommonFileDialogFilter();
			filter.DisplayName = "Any";

			foreach (FileType type in types)
				filter.Extensions.Add(type.Extension);

			filter.ShowExtensions = true;
			return filter;
		}

		private static CommonFileDialogFilter ToFilter(FileType fileType)
		{
			CommonFileDialogFilter filter = new CommonFileDialogFilter();
			filter.DisplayName = fileType.Name;
			filter.Extensions.Add(fileType.Extension);
			filter.ShowExtensions = true;
			return filter;
		}
	}
}
