// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;

	using Directories = System.IO.Directory;
	using Paths = System.IO.Path;

	public class DatFileSource : IFileSource
	{
		public string Name => "FFXIV Saved Appearance Data";
		public bool CanRead => true;
		public bool CanWrite => false;

		public IFileSource.IDirectory GetDefaultDirectory()
		{
			string startdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games/FINAL FANTASY XIV - A Realm Reborn/";
			return new Directory(startdir, this);
		}

		public Task<IEnumerable<IFileSource.IEntry>> GetEntries(IFileSource.IDirectory current, bool recursive, FileInfoBase[] fileTypes)
		{
			return Task.FromResult<IEnumerable<IFileSource.IEntry>>(this.GetFiles(current, recursive, fileTypes));
		}

		public List<IFileSource.IEntry> GetFiles(IFileSource.IDirectory current, bool recursive, FileInfoBase[] fileTypes)
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

					string fileName = Paths.GetFileNameWithoutExtension(filePath);
					string slotNumber = fileName.Substring(12);

					FileStream stream = new FileStream(filePath, FileMode.Open);
					stream.Seek(0x30, SeekOrigin.Begin);
					using BinaryReader reader = new BinaryReader(stream);
					string desc = Regex.Replace(Encoding.ASCII.GetString(reader.ReadBytes(164)), @"(?![ -~]|\r|\n).", string.Empty);
					file.Name = desc.Substring(0, Math.Min(desc.Length, 50));
					file.Metadata = slotNumber;

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
			public bool Exists => System.IO.File.Exists(this.Path);
			public string? Metadata { get; set; }

			public Task Delete()
			{
				return Task.CompletedTask;
			}

			public Task Rename(string newName)
			{
				throw new NotSupportedException();
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
			public bool Exists => true;
			public string? Metadata { get; set; }

			public IFileSource.IDirectory CreateSubDirectory()
			{
				throw new NotSupportedException();
			}

			public Task Rename(string newName)
			{
				throw new NotSupportedException();
			}

			public Task Delete()
			{
				Directories.Delete(this.Path, true);
				return Task.CompletedTask;
			}
		}
	}
}
