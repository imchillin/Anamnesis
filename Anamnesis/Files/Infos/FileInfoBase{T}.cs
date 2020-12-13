// © Anamnesis.
// Developed by W and A Walsh.
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

		protected abstract T Deserialize(Stream stream);
		protected abstract void Serialize(T file, Stream stream);
	}
}
