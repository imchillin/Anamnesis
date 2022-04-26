// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.Memory;

public class Color4Converter : JsonConverter<Color4>
{
	public override Color4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Color4");

		return Color4.FromString(str);
	}

	public override void Write(Utf8JsonWriter writer, Color4 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
