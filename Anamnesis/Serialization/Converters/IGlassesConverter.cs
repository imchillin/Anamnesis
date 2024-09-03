// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.Services;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using Anamnesis.GameData.Excel;

internal class IGlassesConverter : JsonConverter<Glasses>
{
	public override Glasses Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception($"Invalid serialized glasses value");

		uint glassesID = uint.Parse(str);
		return GameDataService.Glasses.Get(glassesID);
	}

	public override void Write(Utf8JsonWriter writer, Glasses value, JsonSerializerOptions options)
	{
		writer.WriteStringValue($"{value.GlassesId}");
	}
}
