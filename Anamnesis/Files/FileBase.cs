// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using Anamnesis.Serialization;
using Anamnesis.Serialization.Converters;
using Anamnesis.Services;

[Serializable]
public abstract class FileBase
{
	public string? Author { get; set; }
	public string? Description { get; set; }
	[Newtonsoft.Json.JsonConverter(typeof(VersionConverter))] public string? Version { get; set; }
	public string? Base64Image { get; set; }

	[JsonIgnore] public virtual string TypeName => this.GetType().Name;
	[JsonIgnore] public abstract string FileExtension { get; }
	[JsonIgnore] public virtual string? FileRegex => null;
	[JsonIgnore] public virtual Func<FileSystemInfo, string> GetFilename => (f) => Path.GetFileNameWithoutExtension(f.FullName);
	[JsonIgnore] public virtual Func<FileSystemInfo, string> GetFullFilename => (f) => Path.GetFileName(f.FullName);
	[JsonIgnore] public BitmapImage? ImageSource => this.GetImage();

	public virtual void Serialize(Stream stream)
	{
		if (this.Author == null)
		{
			this.Author = SettingsService.Current.DefaultAuthor;
		}
	}

	public abstract FileBase Deserialize(Stream stream);

	public FileFilter GetFilter()
	{
		FileFilter filter = new FileFilter(this.GetType(), this.FileExtension, this.FileRegex);
		filter.GetNameCallback = this.GetFilename;
		filter.GetFullNameCallback = this.GetFullFilename;

		return filter;
	}

	public BitmapImage? GetImage()
	{
		if (this.Base64Image == null)
			return null;

		byte[] binaryData = Convert.FromBase64String(this.Base64Image);

		BitmapImage bi = new BitmapImage();
		bi.BeginInit();
		bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
		bi.StreamSource = new MemoryStream(binaryData);
		bi.EndInit();

		return bi;
	}

	public void SetImage(byte[] binaryData)
	{
		this.Base64Image = Convert.ToBase64String(binaryData);
	}
}

#pragma warning disable SA1402
[Serializable]
public abstract class JsonFileBase : FileBase
{
	public override void Serialize(Stream stream)
	{
		base.Serialize(stream);

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
