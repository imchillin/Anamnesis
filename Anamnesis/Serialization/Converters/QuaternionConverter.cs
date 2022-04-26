// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.Memory;

public class QuaternionConverter : JsonConverter<Quaternion>
{
	public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Quaternion");

		return Quaternion.FromString(str);
	}

	public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
