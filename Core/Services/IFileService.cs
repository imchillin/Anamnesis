// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;

	public interface IFileService : IService
	{
		Task<T> Open<T>(FileType fileType)
			where T : FileBase;

		Task<FileBase> OpenAny(params FileType[] fileTypes);

		Task Save(FileBase file);
		Task SaveAs(FileBase file);
	}

	#pragma warning disable SA1402

	public class FileType
	{
		public readonly string Extension;
		public readonly string Name;
		public readonly Type Type;

		public Func<Stream, FileBase> Deserialize;
		public Action<Stream, FileBase> Serialize;

		public FileType(string extension, string name, Type type)
		{
			if (!extension.StartsWith("."))
				extension = "." + extension;

			this.Extension = extension;
			this.Name = name;
			this.Type = type;
		}
	}

	[Serializable]
	public abstract class FileBase
	{
		public IFileService FileService;
		public string Path;

		public abstract FileType GetFileType();

		public void Save()
		{
			Task.Run(this.SaveAsync);
		}

		public void SaveAs()
		{
			Task.Run(this.SaveAsAsync);
		}

		public Task SaveAsync()
		{
			return this.FileService.Save(this);
		}

		public Task SaveAsAsync()
		{
			return this.FileService.SaveAs(this);
		}
	}
}
