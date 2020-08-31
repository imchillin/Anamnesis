// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Services;
	using Directories = System.IO.Directory;
	using Files = System.IO.File;
	using Paths = System.IO.Path;

	public class DatAppearanceFileSource : IFileSource
	{
		public string Name => "FFXIV Saved Appearance Data";

		public bool CanOpen(FileType type)
		{
			return type == DatAppearanceFile.FileType;
		}

		public IFileSource.IDirectory GetDefaultDirectory(FileType[] fileTypes)
		{
			string startdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games/FINAL FANTASY XIV - A Realm Reborn/";
			return new Directory(startdir);
		}

		public Task<IEnumerable<IFileSource.IEntry>> GetEntries(IFileSource.IDirectory current, FileType[] fileTypes, bool recursive)
		{
			return Task.FromResult<IEnumerable<IFileSource.IEntry>>(this.GetFiles(current, fileTypes, recursive));
		}

		public List<IFileSource.IEntry> GetFiles(IFileSource.IDirectory current, FileType[] fileTypes, bool recursive)
		{
			Directory currentDir = current as Directory;

			List<IFileSource.IEntry> results = new List<IFileSource.IEntry>();

			if (current != null)
			{
				SearchOption op = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				string[] filePaths = Directories.GetFiles(currentDir.Path, "FFXIV_CHARA_*.DAT", op);

				foreach (string filePath in filePaths)
				{
					DatAppearanceFile file = DatAppearanceFile.FromDat(filePath);

					if (file.Type == null)
						continue;

					results.Add(file);
				}
			}

			return results;
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
