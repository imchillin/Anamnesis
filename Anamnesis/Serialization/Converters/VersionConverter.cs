// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class VersionConverter : JsonConverter<string?>
{
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		try
		{
			return reader.GetString();
		}
		catch
		{
			// Check for a ktisis style version int.
			// NOTE: Only _some_ ktisis files will have this.
			int ver = reader.GetInt32();
			return $"Ktisis_{ver}";
		}
	}

	public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value);
	}
}
