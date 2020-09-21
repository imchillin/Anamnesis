// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;

	using Directories = System.IO.Directory;
	using Files = System.IO.File;
	using Paths = System.IO.Path;

	public class LocalFileSource : IFileSource
	{
		public LocalFileSource(string name, string baseDir, string defaultDir)
		{
			this.Name = name;
			this.BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + baseDir;
			this.DefaultDirectoryName = defaultDir;
		}

		public string Name { get; private set; }
		public string BaseDirectory { get; private set; }
		public string DefaultDirectoryName { get; private set; }

		/// <summary>
		/// If all filetypes have the same default directory name, return that subdirectory, otherwise returns the raw starting directory.
		/// </summary>
		public IFileSource.IDirectory GetDefaultDirectory()
		{
			string defaultDir = this.BaseDirectory + "/" + this.DefaultDirectoryName + "/";

			if (!Directories.Exists(defaultDir))
				Directories.CreateDirectory(defaultDir);

			return new Directory(defaultDir);
		}

		public Task<IEnumerable<IFileSource.IEntry>> GetEntries(IFileSource.IDirectory current, bool recursive)
		{
			if (recursive)
			{
				return Task.FromResult<IEnumerable<IFileSource.IEntry>>(this.GetFiles(current, true));
			}
			else
			{
				List<IFileSource.IEntry> results = new List<IFileSource.IEntry>();
				results.AddRange(this.GetDirectories(current));
				results.AddRange(this.GetFiles(current, false));
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

		public List<File> GetFiles(IFileSource.IDirectory current, bool recursive)
		{
			Directory? currentDir = current as Directory;

			List<File> results = new List<File>();

			if (currentDir != null)
			{
				SearchOption op = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				string[] filePaths = Directories.GetFiles(currentDir.Path, "*.*", op);

				foreach (string filePath in filePaths)
				{
					try
					{
						string extension = Paths.GetExtension(filePath);
						FileInfoBase info = FileService.GetFileInfo(extension);

						results.Add(new File(filePath, info));
					}
					catch (Exception)
					{
					}
				}
			}

			return results;
		}

		public class File : IFileSource.IFile
		{
			public File(string path, FileInfoBase info)
			{
				this.Path = path;
				this.Name = Paths.GetFileNameWithoutExtension(path);
				this.Type = info;
			}

			public string Name { get; private set; }
			public string Path { get; private set; }
			public FileInfoBase? Type { get; private set; }

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
