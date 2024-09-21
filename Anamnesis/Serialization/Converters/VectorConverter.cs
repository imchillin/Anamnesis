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
		string? str = reader.GetString() ?? throw new Exception("Cannot convert null to Vector2");
		return VectorExtensions.FromString2D(str);
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
