// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using ConceptMatrix.Memory;

	public class VectorConverter : JsonConverter<Vector>
	{
		public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Vector.FromString(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
