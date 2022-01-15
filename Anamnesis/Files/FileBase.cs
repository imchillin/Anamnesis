// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.IO;
	using System.Text.Json.Serialization;
	using Anamnesis.Serialization;

	[Serializable]
	public abstract class FileBase
	{
		public string? Author { get; set; }

		[JsonIgnore] public virtual string TypeName => this.GetType().Name;
		[JsonIgnore] public abstract string FileExtension { get; }
		[JsonIgnore] public virtual string? FileRegex => null;
		[JsonIgnore] public virtual Func<FileSystemInfo, string> GetFilename => (f) => Path.GetFileNameWithoutExtension(f.FullName);
		[JsonIgnore] public virtual Func<FileSystemInfo, string> GetFullFilename => (f) => Path.GetFileName(f.FullName);

		public abstract void Serialize(Stream stream);
		public abstract FileBase Deserialize(Stream stream);

		public FileFilter GetFilter()
		{
			FileFilter filter = new FileFilter(this.FileExtension, this.FileRegex);
			filter.GetNameCallback = this.GetFilename;
			filter.GetFullNameCallback = this.GetFullFilename;

			return filter;
		}
	}

	#pragma warning disable SA1402
	[Serializable]
	public abstract class JsonFileBase : FileBase
	{
		public override void Serialize(Stream stream)
		{
			using TextWriter writer = new StreamWriter(stream);
			string json = SerializerService.Serialize(this);
			writer.Write(json);
		}

		public override FileBase Deserialize(Stream stream)
		{
			using TextReader reader = new StreamReader(stream);
			string json = reader.ReadToEnd();
			return (FileBase)SerializerService.Deserialize(json, this.GetType());
		}
	}
}