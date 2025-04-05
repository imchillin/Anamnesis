// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.GameData.Sheets;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ImageReferenceConverter : JsonConverter<ImgRef>
{
	public override ImgRef Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		uint iconId = reader.GetUInt32();
		return new ImgRef(iconId);
	}

	public override void Write(Utf8JsonWriter writer, ImgRef value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.ImageId);
	}
}
