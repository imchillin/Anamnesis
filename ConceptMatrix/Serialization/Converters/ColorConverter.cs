// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization.Converters
{
	using System;
	using ConceptMatrix;
	using Newtonsoft.Json;

	public class ColorConverter : JsonConverter<Color>
	{
		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			string s = (string)reader.Value;
			return Color.FromString(s);
		}

		public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}
	}
}
