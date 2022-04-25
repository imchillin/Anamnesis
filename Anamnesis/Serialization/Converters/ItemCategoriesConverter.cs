// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.GameData;

public class ItemCategoriesConverter : JsonConverter<ItemCategories>
{
	public override ItemCategories Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Enum.Parse<ItemCategories>(reader.GetString() ?? nameof(ItemCategories.None));
	}

	public override void Write(Utf8JsonWriter writer, ItemCategories value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
