// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.Memory;

public class ColorConverter : JsonConverter<Color>
{
	public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Color");

		return Color.FromString(str);
	}

	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
