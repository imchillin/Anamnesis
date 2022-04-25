// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.GameData.Sheets;

public class ImageReferenceConverter : JsonConverter<ImageReference>
{
	public override ImageReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		uint iconId = reader.GetUInt32();
		return new ImageReference(iconId);
	}

	public override void Write(Utf8JsonWriter writer, ImageReference value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.ImageId);
	}
}
