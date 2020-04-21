// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization.Converters
{
	using System;
	using ConceptMatrix;
	using Newtonsoft.Json;

	public class VectorConverter : JsonConverter<Vector>
	{
		public override Vector ReadJson(JsonReader reader, Type objectType, Vector existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			string s = (string)reader.Value;
			return Vector.FromString(s);
		}

		public override void WriteJson(JsonWriter writer, Vector value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}
	}
}
