// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Anamnesis.Keyboard;

	public class KeyCombinationConverter : JsonConverter<KeyCombination>
	{
		public override KeyCombination Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string? str = reader.GetString();

			if (str == null)
				throw new Exception("Cannot convert null to Color");

			return KeyCombination.FromString(str);
		}

		public override void Write(Utf8JsonWriter writer, KeyCombination value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
