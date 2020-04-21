// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization.Converters
{
	using System;
	using ConceptMatrix;
	using Newtonsoft.Json;

	public class Color4Converter : JsonConverter<Color4>
	{
		public override Color4 ReadJson(JsonReader reader, Type objectType, Color4 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			string s = (string)reader.Value;
			return Color4.FromString(s);
		}

		public override void WriteJson(JsonWriter writer, Color4 value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}
	}
}
