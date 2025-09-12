// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.GameData;
using Anamnesis.Services;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class IDyeConverter : JsonConverter<IDye>
{
	public override IDye Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString() ?? throw new Exception($"Invalid serialized item value");

		uint dyeID = uint.Parse(str);
		return GameDataService.Dyes.GetRow(dyeID);
	}

	public override void Write(Utf8JsonWriter writer, IDye value, JsonSerializerOptions options)
	{
		writer.WriteStringValue($"{value.RowId}");
	}
}
