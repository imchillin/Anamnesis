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
	using Paths = System.IO.Path;

	public class DatFileSource : IFileSource
	{
		public string Name => "FFXIV Saved Appearance Data";

		public IFileSource.IDirectory GetDefaultDirectory()
		{
			string startdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games/FINAL FANTASY XIV - A Realm Reborn/";
			return new Directory(startdir, this);
		}

		public Task<IEnumerable<IFileSource.IEntry>> GetEntries(IFileSource.IDirectory current, bool recursive)
		{
			return Task.FromResult<IEnumerable<IFileSource.IEntry>>(this.GetFiles(current, recursive));
		}

		public List<IFileSource.IEntry> GetFiles(IFileSource.IDirectory current, bool recursive)
		{
			Directory? currentDir = current as Directory;

			List<IFileSource.IEntry> results = new List<IFileSource.IEntry>();

			if (currentDir != null)
			{
				SearchOption op = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				string[] filePaths = Directories.GetFiles(currentDir.Path, "FFXIV_CHARA_*.DAT", op);

				foreach (string filePath in filePaths)
				{
					File file = new File(this);
					file.Path = filePath;
					file.Type = FileService.GetFileInfo<DatCharacterFile>();

					string name = Paths.GetFileNameWithoutExtension(filePath);
					file.Name = name.Substring(12);

					results.Add(file);
				}
			}

			return results;
		}

		public class File : IFileSource.IFile
		{
			public File(DatFileSource source)
			{
				this.Source = source;
			}

			public FileInfoBase? Type { get; set; }
			public string? Path { get; set; }
			public string? Name { get; set; }
			public IFileSource Source { get; private set; }

			public Task Delete()
			{
				return Task.CompletedTask;
			}
		}

		public class Directory : IFileSource.IDirectory
		{
			public Directory(string path, DatFileSource source)
			{
				this.Path = path;
				this.Name = Paths.GetFileName(path);
				this.Source = source;
			}

			public string Name { get; private set; }
			public string Path { get; private set; }
			public IFileSource Source { get; private set; }

			public Task Delete()
			{
				Directories.Delete(this.Path, true);
				return Task.CompletedTask;
			}
		}
	}
}
