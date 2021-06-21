// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;
	using Microsoft.VisualBasic.FileIO;
	using Serilog;
	using Directories = System.IO.Directory;
	using Files = System.IO.File;
	using Paths = System.IO.Path;
	using SearchOption = System.IO.SearchOption;

	public class LocalFileSource : IFileSource
	{
		public LocalFileSource(string name, string baseDir)
		{
			this.Name = name;

			this.BaseDirectory = FileService.ParseToFilePath(baseDir);
		}

		public bool CanRead => true;
		public virtual bool CanWrite => true;
		public string Name { get; private set; }
		public string BaseDirectory { get; private set; }

		/// <summary>
		/// If all filetypes have the same default directory name, return that subdirectory, otherwise returns the raw starting directory.
		/// </summary>
		public IFileSource.IDirectory GetDefaultDirectory()
		{
			string defaultDir = this.BaseDirectory;

			if (!Directories.Exists(defaultDir))
				Directories.CreateDirectory(defaultDir);

			return new Directory(defaultDir, this);
		}

		public Task<IEnumerable<IFileSource.IEntry>> GetEntries(IFileSource.IDirectory current, bool recursive, FileInfoBase[] fileTypes)
		{
			Directory? currentDir = current as Directory;
			if (currentDir == null || !currentDir.Exists)
				throw new Exception("Current directory does not exist");

			if (recursive)
			{
				return Task.FromResult<IEnumerable<IFileSource.IEntry>>(this.GetFiles(current, true, fileTypes));
			}
			else
			{
				List<IFileSource.IEntry> results = new List<IFileSource.IEntry>();
				results.AddRange(this.GetDirectories(current));
				results.AddRange(this.GetFiles(current, false, fileTypes));
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
					results.Add(new Directory(dirPath, this));
				}
			}

			return results;
		}

		public List<File> GetFiles(IFileSource.IDirectory current, bool recursive, FileInfoBase[] fileTypes)
		{
			Directory? currentDir = current as Directory;
			List<File> results = new List<File>();

			HashSet<string> validExtensions = new HashSet<string>();
			foreach (FileInfoBase info in fileTypes)
				validExtensions.Add("." + info.Extension);

			if (currentDir != null)
			{
				SearchOption op = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				string[] filePaths = Directories.GetFiles(currentDir.Path, "*.*", op);

				foreach (string filePath in filePaths)
				{
					try
					{
						string extension = Paths.GetExtension(filePath);

						if (!validExtensions.Contains(extension))
							continue;

						FileInfoBase info = FileService.GetFileInfo(extension);

						File file = new File(filePath, info, this);
						file.Metadata = info.GetMetadata(file);
						results.Add(file);
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to get file information");
					}
				}
			}

			return results;
		}

		public class File : IFileSource.IFile
		{
			public File(string path, FileInfoBase info, LocalFileSource source)
			{
				this.Path = path;
				this.Name = Paths.GetFileNameWithoutExtension(path);
				this.Type = info;
				this.Source = source;
			}

			public string Name { get; private set; }
			public string Path { get; private set; }
			public FileInfoBase? Type { get; private set; }
			public IFileSource Source { get; private set; }
			public bool Exists => Files.Exists(this.Path);
			public string? Metadata { get; set; }

			public Task Delete()
			{
				FileSystem.DeleteFile(this.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
				////Files.Delete(this.Path);
				return Task.CompletedTask;
			}

			public Task Rename(string newName)
			{
				newName = newName.Trim('.');
				string newPath = Paths.GetDirectoryName(this.Path) + "\\" + newName + Paths.GetExtension(this.Path);
				Files.Move(this.Path, newPath);
				this.Path = newPath;
				return Task.CompletedTask;
			}
		}

		public class Directory : IFileSource.IDirectory
		{
			public Directory(string path, LocalFileSource source)
			{
				while (path.EndsWith('\\'))
					path = path.Substring(0, path.Length - 1);

				this.Path = path;
				this.Name = Paths.GetFileName(path);
				this.Source = source;
			}

			public string Name { get; private set; }
			public string Path { get; private set; }
			public IFileSource Source { get; private set; }
			public bool Exists => Directories.Exists(this.Path);
			public string? Metadata { get; set; }

			public IFileSource.IDirectory CreateSubDirectory()
			{
				string basePath = this.Path + "\\New Directory";
				string newPath = basePath;

				int i = 1;
				while (Directories.Exists(newPath))
				{
					newPath = $"{basePath} ({i})";
					i++;
				}

				Directories.CreateDirectory(newPath);
				return new Directory(newPath, (LocalFileSource)this.Source);
			}

			public Task Delete()
			{
				FileSystem.DeleteDirectory(this.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
				////Directories.Delete(this.Path, true);
				return Task.CompletedTask;
			}

			public Task Rename(string newName)
			{
				string newPath = Paths.GetDirectoryName(this.Path) + "\\" + newName;

				if (this.Path == newPath)
					return Task.CompletedTask;

				Directories.Move(this.Path, newPath);
				this.Path = newPath;
				return Task.CompletedTask;
			}
		}
	}
}
