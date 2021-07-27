// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Anamnesis.GameData;
	using Anamnesis.Services;

	public class INpcResidentConverter : JsonConverter<INpcResident>
	{
		public override INpcResident Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string? str = reader.GetString();

			if (str == null)
				throw new Exception($"Invalid serialized item value");

			uint dyeID = uint.Parse(str);
			return GameDataService.ResidentNPCs.Get(dyeID);
		}

		public override void Write(Utf8JsonWriter writer, INpcResident value, JsonSerializerOptions options)
		{
			writer.WriteStringValue($"{value.Key}");
		}
	}
}
