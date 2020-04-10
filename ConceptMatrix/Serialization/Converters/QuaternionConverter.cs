// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization.Converters
{
	using System;
	using ConceptMatrix;
	using Newtonsoft.Json;

	public class QuaternionConverter : JsonConverter<Quaternion>
	{
		public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			string s = (string)reader.Value;
			return Quaternion.FromString(s);
		}

		public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}
	}
}
