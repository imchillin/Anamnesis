// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;
	using ConceptMatrix.Memory.Serialization.Converters;

	public class SerializerService : ISerializerService
	{
		public static JsonSerializerOptions Options = new JsonSerializerOptions();

		static SerializerService()
		{
			Options.WriteIndented = true;
			Options.PropertyNameCaseInsensitive = false;
			Options.IgnoreNullValues = true;

			Options.Converters.Add(new JsonStringEnumConverter());
			Options.Converters.Add(new Color4Converter());
			Options.Converters.Add(new ColorConverter());
			Options.Converters.Add(new OffsetConverter());
			Options.Converters.Add(new QuaternionConverter());
			Options.Converters.Add(new VectorConverter());
		}

		public string Serialize(object obj)
		{
			return JsonSerializer.Serialize(obj, Options);
		}

		public T Deserialize<T>(string json)
			where T : new()
		{
			return JsonSerializer.Deserialize<T>(json, Options);
		}

		public object Deserialize(string json, Type type)
		{
			return JsonSerializer.Deserialize(json, type, Options);
		}

		public Task Initialize()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}
	}
}
