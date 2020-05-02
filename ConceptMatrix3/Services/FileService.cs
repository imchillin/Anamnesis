// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Serialization;
	using Microsoft.Win32;

	public class FileService : IFileService
	{
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

		public async Task<T> Open<T>(FileType fileType, string path = null)
			where T : FileBase
		{
			try
			{
				if (string.IsNullOrEmpty(path))
				{
					path = await App.Current.Dispatcher.InvokeAsync<string>(() =>
					{
						OpenFileDialog dlg = new OpenFileDialog();
						dlg.Filter = ToFilter(fileType);
						bool? result = dlg.ShowDialog();

						if (result != true)
							return null;

						return dlg.FileName;
					});
				}
				else if (!path.EndsWith(fileType.Extension))
				{
					path = path + "." + fileType.Extension;
				}

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

		public async Task<FileBase> OpenAny(params FileType[] fileTypes)
		{
			try
			{
				string path = await App.Current.Dispatcher.InvokeAsync<string>(() =>
				{
					OpenFileDialog dlg = new OpenFileDialog();
					dlg.Filter = ToAnyFilter(fileTypes);
					bool? result = dlg.ShowDialog();

					if (result != true)
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
				Log.Write(new Exception("Failed to open file", ex), "Files", Log.Severity.Error);
			}

			return null;
		}

		public async Task Save(FileBase file, string path = null)
		{
			try
			{
				FileType type = file.GetFileType();

				if (path == null)
					path = file.Path;

				if (path == null)
				{
					path = await App.Current.Dispatcher.InvokeAsync<string>(() =>
					{
						SaveFileDialog dlg = new SaveFileDialog();
						dlg.Filter = ToFilter(type);
						bool? result = dlg.ShowDialog();

						if (result != true)
							return null;

						return Path.Combine(Path.GetDirectoryName(dlg.FileName), Path.GetFileNameWithoutExtension(dlg.FileName));
					});

					if (path == null)
					{
						return;
					}
				}

				path += "." + type.Extension;

				using FileStream stream = new FileStream(path, FileMode.Create);
				if (type.Serialize != null)
				{
					type.Serialize.Invoke(stream, file);
				}
				else
				{
					using TextWriter writer = new StreamWriter(stream);
					string json = Serializer.Serialize(file);
					writer.Write(json);
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to save file", ex), "Files", Log.Severity.Error);
			}
		}

		public Task SaveAs(FileBase file)
		{
			file.Path = null;
			return this.Save(file);
		}

		public Task<string> OpenDirectory(string title, params string[] defaults)
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
					file = (FileBase)Serializer.Deserialize(json, type.Type);
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
