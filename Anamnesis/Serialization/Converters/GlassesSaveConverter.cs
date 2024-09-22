// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.Services;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using Serilog;
using static Anamnesis.Files.CharacterFile;

internal class GlassesSaveConverter : JsonConverter<GlassesSave>
{
	public override GlassesSave Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		ushort glassesId = 0;

		try
		{
			// Legacy chara files will have Glasses serialized as "Glasses" : #.
			// Current chara files have it as an object with a GlassesId property.
			if (reader.TokenType == JsonTokenType.Number)
			{
				glassesId = reader.GetUInt16();
			}
			else if (reader.TokenType == JsonTokenType.StartObject)
			{
				JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
				if (jsonDoc.RootElement.TryGetProperty("GlassesId", out JsonElement glassesIdEl))
				{
					glassesId = glassesIdEl.GetUInt16();
				}
			}
		}
		catch(Exception ex)
		{
			Log.Error(ex, "Error reading glasses from file.");
		}

		return new GlassesSave(GameDataService.Glasses.Get((byte)glassesId).GlassesId);
	}

	public override void Write(Utf8JsonWriter writer, GlassesSave value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value);
	}
}
