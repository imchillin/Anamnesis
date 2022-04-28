// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.Actor;
using Anamnesis.GameData;

public class INpcBaseConverter : JsonConverter<INpcBase>
{
	public override INpcBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception($"Invalid serialized item value");

		return INpcBaseExtensions.FromStringKey(str);
	}

	public override void Write(Utf8JsonWriter writer, INpcBase value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToStringKey());
	}
}
