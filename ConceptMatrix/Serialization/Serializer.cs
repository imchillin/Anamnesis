// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Serialization
{
	using System;
	using System.Collections.Generic;
	using ConceptMatrix.GUI.Serialization.Converters;
	using Newtonsoft.Json;

	// Consider making this a service
	public static class Serializer
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = new List<JsonConverter>()
			{
				new ColorConverter(),
				new VectorConverter(),
				new QuaternionConverter(),
			},
		};

		public static string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, Settings);
		}

		public static object Deserialize(string json, Type type)
		{
			return JsonConvert.DeserializeObject(json, type, Settings);
		}
	}
}
