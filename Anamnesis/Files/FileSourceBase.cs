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
	using Anamnesis.Services;

	using Directories = System.IO.Directory;
	using Files = System.IO.File;
	using Paths = System.IO.Path;

	public abstract class FileSourceBase : IFileSource
	{
		protected string startdir = string.Empty;

		public FileSourceBase(string name)
		{
			this.Name = name;
		}

		public string Name { get; private set; }

		public abstract bool CanOpen(FileType type);

		/// <summary>
		/// If all filetypes have the same default directory name, return that subdirectory, otherwise returns the raw starting directory.
		/// </summary>
		public IFileSource.IDirectory GetDefaultDirectory(FileType[] fileTypes)
		{
			string? dir = null;
			foreach (FileType type in fileTypes)
			{
				if (!this.CanOpen(type))
					continue;

				if (dir == null)
				{
					dir = type.DefaultDirectoryName;
				}
				else
				{
					if (dir != type.DefaultDirectoryName)
					{
						return new Directory(this.startdir);
					}
				}
			}

			string defaultDir = this.startdir + dir + "/";

			if (!Directories.Exists(defaultDir))
				Directories.CreateDirectory(defaultDir);

			return new Directory(defaultDir);
		}

		public Task<IEnumerable<IFileSource.IEntry>> GetEntries(IFileSource.IDirectory current, FileType[] fileTypes, bool recursive)
		{
			if (recursive)
			{
				return Task.FromResult<IEnumerable<IFileSource.IEntry>>(this.GetFiles(current, fileTypes, true));
			}
			else
			{
				List<IFileSource.IEntry> results = new List<IFileSource.IEntry>();
				results.AddRange(this.GetDirectories(current));
				results.AddRange(this.GetFiles(current, fileTypes, false));
				return Task.FromResult<IEnumerable<IFileSource.IEntry>>(results);
			}
		}

		public List<Directory> GetDirectories(IFileSource.IDirectory current)
		{
			Directory? currentDir = current as Directory;

			List<Directory> results = new List<Directory>();

			if (currentDir != null)
			{
				string[] dirPaths = Directories.GetDirectories(currentDir.Path);
				foreach (string dirPath in dirPaths)
				{
					results.Add(new Directory(dirPath));
				}
			}

			return results;
		}

		public List<File> GetFiles(IFileSource.IDirectory current, FileType[] fileTypes, bool recursive)
		{
			Directory? currentDir = current as Directory;

			List<File> results = new List<File>();

			if (currentDir != null)
			{
				SearchOption op = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				string[] filePaths = Directories.GetFiles(currentDir.Path, "*.*", op);

				foreach (string filePath in filePaths)
				{
					File file = new File(filePath, fileTypes);

					if (file.Type == null)
						continue;

					results.Add(file);
				}
			}

			return results;
		}

		public class File : IFileSource.IFile
		{
			public File(string path, FileType[] knownTypes)
			{
				this.Path = path;
				this.Name = Paths.GetFileNameWithoutExtension(path);

				string extension = Paths.GetExtension(path);
				foreach (FileType type in knownTypes)
				{
					if (type.IsExtension(extension))
					{
						this.Type = type;
					}
				}
			}

			public string Name { get; private set; }
			public string Path { get; private set; }
			public FileType? Type { get; private set; }

			public Task Delete()
			{
				Files.Delete(this.Path);
				return Task.CompletedTask;
			}
		}

		public class Directory : IFileSource.IDirectory
		{
			public Directory(string path)
			{
				this.Path = path;
				this.Name = Paths.GetFileName(path);
			}

			public string Name { get; private set; }
			public string Path { get; private set; }

			public Task Delete()
			{
				Directories.Delete(this.Path, true);
				return Task.CompletedTask;
			}
		}
	}
}
