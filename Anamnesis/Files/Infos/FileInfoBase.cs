// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files.Infos
{
	using System;
	using System.IO;

	public abstract class FileInfoBase
	{
		private IFileSource[]? fileSource;

		public abstract string Extension { get; }
		public abstract string Name { get; }

		public abstract IFileSource[] FileSources { get; }

		public virtual Type? LoadOptionsViewType => null;
		public virtual Type? SaveOptionsViewType => null;

		public abstract FileBase DeserializeFile(Stream stream);
		public abstract void SerializeFile(FileBase file, Stream stream);

		public virtual string? GetMetadata(IFileSource.IFile file)
		{
			return this.Name;
		}

		public abstract bool IsFile(Type type);

		public IFileSource[] GetFileSources()
		{
			if (this.fileSource == null)
				this.fileSource = this.FileSources;

			return this.fileSource;
		}
	}
}
