// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using XivToolsWpf.Math3D.Extensions;

public class Vector2Converter : JsonConverter<Vector2>
{
	public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			// New format: "X, Y"
			string? str = reader.GetString() ?? throw new Exception("Cannot convert null to Vector2");
			return VectorExtensions.FromString2D(str);
		}
		else if (reader.TokenType == JsonTokenType.StartObject)
		{
			// Fallback to old format: { "X": 0, "Y": 0 }
			using JsonDocument document = JsonDocument.ParseValue(ref reader);
			JsonElement root = document.RootElement;

			if (root.TryGetProperty("X", out JsonElement propX) && propX.TryGetSingle(out float x) &&
				root.TryGetProperty("Y", out JsonElement propY) && propY.TryGetSingle(out float y))
				return new Vector2(x, y);
			else
				throw new JsonException("Invalid JSON format for Vector2. Expected properties 'X' and 'Y' with float values.");
		}
		else
		{
			throw new Exception("Unexpected token type encountered during string to Vector2 conversion");
		}
	}

	public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToInvariantString());
	}
}

public class Vector3Converter : JsonConverter<Vector3>
{
	public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString() ?? throw new Exception("Cannot convert null to Vector3");
		return VectorExtensions.FromString3D(str);
	}

	public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToInvariantString());
	}
}
