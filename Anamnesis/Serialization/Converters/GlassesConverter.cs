// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.Services;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GlassesConverter : JsonConverter<Glasses>
{
	public override Glasses Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		uint glassesRowId;

		// Favorites for glasses should be stored as an array of numbers (as strings), for example: ["1", "88"]
		// Legacy favorites files mistakenly had the entire object, so the second condition accounts for this.
		//    (It will be converted to normal if the user favortes something else.)
		// If none of those work, throw.
		if (reader.TokenType == JsonTokenType.String)
		{
			string? str = reader.GetString();

			if (!string.IsNullOrEmpty(str))
			{
				glassesRowId = uint.Parse(str);
				return GameDataService.Glasses.GetRow(glassesRowId);
			}
		}
		else if (reader.TokenType == JsonTokenType.StartObject)
		{
			JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
			if (jsonDoc.RootElement.TryGetProperty("GlassesId", out JsonElement glassesIdEl))
			{
				glassesRowId = glassesIdEl.GetUInt32();
				return GameDataService.Glasses.GetRow(glassesRowId);
			}
		}

		throw new Exception($"Invalid serialized glasses value");
	}

	public override void Write(Utf8JsonWriter writer, Glasses value, JsonSerializerOptions options)
	{
		writer.WriteStringValue($"{value.RowId}");
	}
}
