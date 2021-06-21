// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files.Infos
{
	using System;
	using System.IO;

	public abstract class FileInfoBase<T> : FileInfoBase
		where T : FileBase
	{
		public sealed override FileBase DeserializeFile(Stream stream)
		{
			return this.Deserialize(stream);
		}

		public override void SerializeFile(FileBase file, Stream stream)
		{
			if (file is T tFile)
			{
				this.Serialize(tFile, stream);
			}
			else
			{
				throw new Exception("Attempt to serialize file as incorrect type");
			}
		}

		public override bool IsFile(Type type)
		{
			return type == typeof(T);
		}

		public override string? GetMetadata(IFileSource.IFile file)
		{
			if (file.Path == null)
				return null;

			using FileStream stream = new FileStream(file.Path, FileMode.Open);
			T tFile = this.Deserialize(stream);

			if (tFile.Author == null)
				return base.GetMetadata(file);

			return tFile.Author + " - " + base.GetMetadata(file);
		}

		protected abstract T Deserialize(Stream stream);
		protected abstract void Serialize(T file, Stream stream);
	}
}
