// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files.Infos
{
	using System.IO;
	using Anamnesis.Serialization;

	public abstract class JsonFileInfoBase<T> : FileInfoBase<T>
		where T : FileBase, new()
	{
		protected override void Serialize(T file, Stream stream)
		{
			using TextWriter writer = new StreamWriter(stream);
			string json = SerializerService.Serialize(file);
			writer.Write(json);
		}

		protected override T Deserialize(Stream stream)
		{
			using TextReader reader = new StreamReader(stream);
			string json = reader.ReadToEnd();
			return SerializerService.Deserialize<T>(json);
		}
	}
}
