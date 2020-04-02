// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Services;
	using Microsoft.Win32;
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
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.Filter = ToFilter(fileType);
				bool? selected = dlg.ShowDialog();

				if (selected == false)
					return null;

				FileBase file = await Open(dlg.FileName, fileType);
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
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.Filter = ToFilter(fileTypes);
				bool? selected = dlg.ShowDialog();

				if (selected == false)
					return null;

				string filePath = dlg.FileName;
				string extension = Path.GetExtension(filePath);
				FileType type = GetFileType(fileTypes, extension);

				return await Open(filePath, type);
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to open file", ex));
			}

			return null;
		}

		public Task Save(FileBase file)
		{
			try
			{
				FileType type = file.GetFileType();

				string path = file.Path;

				if (path == null)
				{
					SaveFileDialog dlg = new SaveFileDialog();
					dlg.Filter = ToFilter(type);
					bool? selected = dlg.ShowDialog();

					if (selected == false)
						return Task.CompletedTask;

					path = dlg.FileName;
				}

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
							string json = JsonConvert.SerializeObject(file, Formatting.Indented);
							writer.Write(json);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to save file", ex));
			}

			return Task.CompletedTask;
		}

		public Task SaveAs(FileBase file)
		{
			file.Path = null;
			return this.Save(file);
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
						file = (FileBase)JsonConvert.DeserializeObject(json, type.Type);
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
			foreach (FileType fileType in fileTypes)
			{
				if (fileType.Extension == extension)
				{
					return fileType;
				}
			}

			throw new Exception($"Unable to determine file type from extension: \"{extension}\"");
		}

		private static string ToFilter(FileType fileType)
		{
			StringBuilder filterbuilder = new StringBuilder();
			filterbuilder.Append(fileType.Name);
			filterbuilder.Append(" (*");
			filterbuilder.Append(fileType.Extension);
			filterbuilder.Append(")|*");
			filterbuilder.Append(fileType.Extension);
			return filterbuilder.ToString();
		}

		private static string ToFilter(FileType[] fileTypes)
		{
			StringBuilder filterbuilder = new StringBuilder();

			if (fileTypes.Length > 1)
			{
				filterbuilder.Append("All Files (*");
				for (int i = 0; i < fileTypes.Length; i++)
				{
					if (i > 0)
						filterbuilder.Append(", *");

					filterbuilder.Append(fileTypes[i].Extension);
				}

				filterbuilder.Append(")|*");

				for (int i = 0; i < fileTypes.Length; i++)
				{
					if (i > 0)
						filterbuilder.Append(";*");

					filterbuilder.Append(fileTypes[i].Extension);
				}

				filterbuilder.Append("|");
			}

			for (int i = 0; i < fileTypes.Length; i++)
			{
				if (i > 0)
					filterbuilder.Append("|");

				filterbuilder.Append(fileTypes[i].Name);
				filterbuilder.Append("(*");
				filterbuilder.Append(fileTypes[i].Extension);
				filterbuilder.Append(")|*");
				filterbuilder.Append(fileTypes[i].Extension);
			}

			return filterbuilder.ToString();
		}
	}
}
