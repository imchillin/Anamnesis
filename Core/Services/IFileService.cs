// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;

	#pragma warning disable SA1402

	public interface IFileService : IService
	{
		void AddFileSource(IFileSource source);

		Task<T> Open<T>(FileType fileType, string? path = null)
			where T : FileBase;

		Task<FileBase> OpenAny(params FileType[] fileTypes);

		Task<string> OpenDirectory(string title, params string[] defaults);

		/// <summary>
		/// Saves a file.
		/// </summary>
		/// <param name="writeFile">a callback executed once the location and advanced mode has been selected.</param>
		/// <param name="type">the type of file to save.</param>
		/// <param name="path">the default path to write to.</param>
		Task Save(Func<bool, Task<FileBase>> writeFile, FileType type, string? path = null);
	}

	public interface IFileSource
	{
		public interface IEntry
		{
			public string Path { get; }
			public string Name { get; }

			public Task Delete();
		}

		public interface IFile : IEntry
		{
			public FileType Type { get; }
		}

		public interface IDirectory : IEntry
		{
		}

		public string Name { get; }

		public bool CanOpen(FileType type);
		public IDirectory GetDefaultDirectory(FileType[] fileTypes);
		public Task<IEnumerable<IEntry>> GetEntries(IDirectory current, FileType[] fileTypes);
	}

	public class FileType
	{
		public readonly string Extension;
		public readonly string Name;
		public readonly Type Type;
		public readonly bool SupportsAdvancedMode;
		public readonly string? DefaultDirectoryName;

		public Func<Stream, FileBase>? Deserialize;
		public Action<Stream, FileBase>? Serialize;

		public FileType(string extension, string name, Type type, bool canAdvancedLoad = false, string? defaultdirectoryName = null)
		{
			if (extension.StartsWith("."))
				extension = extension.Substring(1, extension.Length - 1);

			this.Extension = extension;
			this.Name = name;
			this.Type = type;
			this.SupportsAdvancedMode = canAdvancedLoad;
			this.DefaultDirectoryName = defaultdirectoryName;
		}

		public bool IsExtension(string extension)
		{
			if (extension.StartsWith("."))
				extension = extension.Substring(1, extension.Length - 1);

			return this.Extension == extension;
		}
	}

	[Serializable]
	public abstract class FileBase
	{
		public string? Path;
		public bool UseAdvancedLoad;
		public abstract FileType GetFileType();
	}
}
