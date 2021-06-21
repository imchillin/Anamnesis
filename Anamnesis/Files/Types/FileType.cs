// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files.Types
{
	using System;
	using System.IO;

	public class FileType
	{
		public readonly string Extension;
		public readonly string Name;
		public readonly Type Type;
		public readonly bool SupportsAdvancedMode;
		public readonly string? DefaultDirectoryName;

		public Func<Stream, FileBase>? Deserialize;
		public Action<Stream, FileBase>? Serialize;

		public FileType(string extension, string name, Type type, bool canAdvancedLoad = false, string? defaultdirectoryName = null, Func<Stream, FileBase>? deserialize = null, Action<Stream, FileBase>? serialize = null)
		{
			if (extension.StartsWith("."))
				extension = extension.Substring(1, extension.Length - 1);

			this.Extension = extension;
			this.Name = name;
			this.Type = type;
			this.SupportsAdvancedMode = canAdvancedLoad;
			this.DefaultDirectoryName = defaultdirectoryName;
			this.Serialize = serialize;
			this.Deserialize = deserialize;
		}

		public bool IsExtension(string extension)
		{
			if (extension.StartsWith("."))
				extension = extension.Substring(1, extension.Length - 1);

			return this.Extension == extension;
		}
	}
}
