// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Services;

	public class INpcBaseConverter : JsonConverter<INpcBase>
	{
		public override INpcBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string? str = reader.GetString();

			if (str == null)
				throw new Exception($"Invalid serialized item value");

			string[] parts = str.Split(':');
			if (parts.Length == 1)
			{
				uint key = uint.Parse(str);
				return GameDataService.ResidentNPCs.Get(key);
			}
			else
			{
				char type = parts[0][0];
				uint key = uint.Parse(parts[1]);

				if (type == 'R')
					return GameDataService.ResidentNPCs.Get(key);

				throw new Exception($"Unknown Npc Type key: {type}");
			}
		}

		public override void Write(Utf8JsonWriter writer, INpcBase value, JsonSerializerOptions options)
		{
			Type type = value.GetType();
			char t;

			if (type == typeof(NpcResidentViewModel))
			{
				t = 'R';
			}
			else
			{
				throw new Exception($"Unknown Npc Type: {type}");
			}

			writer.WriteStringValue($"{t}:{value.Key}");
		}
	}
}
