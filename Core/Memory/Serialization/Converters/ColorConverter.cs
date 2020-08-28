// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using ConceptMatrix.Memory;

	public class ColorConverter : JsonConverter<Color>
	{
		public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Color.FromString(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
