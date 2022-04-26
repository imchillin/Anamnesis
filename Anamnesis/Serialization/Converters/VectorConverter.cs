// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.Memory;

public class VectorConverter : JsonConverter<Vector>
{
	public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Vector");

		return Vector.FromString(str);
	}

	public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
