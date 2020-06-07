// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization
{
	using System;
	using System.Text.Json;
	using Anamnesis.Serialization.Converters;

	// Consider making this a service
	public static class Serializer
	{
		public static JsonSerializerOptions Options = new JsonSerializerOptions();

		static Serializer()
		{
			Options.WriteIndented = true;
			Options.PropertyNameCaseInsensitive = false;
			Options.IgnoreNullValues = true;

			Options.Converters.Add(new Color4Converter());
			Options.Converters.Add(new ColorConverter());
			Options.Converters.Add(new OffsetConverter());
			Options.Converters.Add(new QuaternionConverter());
			Options.Converters.Add(new VectorConverter());
		}

		public static string Serialize(object obj)
		{
			return JsonSerializer.Serialize(obj, Options);
		}

		public static T Deserialize<T>(string json)
			where T : new()
		{
			return JsonSerializer.Deserialize<T>(json, Options);
		}

		public static object Deserialize(string json, Type type)
		{
			return JsonSerializer.Deserialize(json, type, Options);
		}
	}
}
