// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Anamnesis;
	using ConceptMatrix;

	public class Color4Converter : JsonConverter<Color4>
	{
		public override Color4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Color4.FromString(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, Color4 value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
