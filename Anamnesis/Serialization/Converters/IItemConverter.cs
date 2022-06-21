// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Services;

public class IItemConverter : JsonConverter<IItem>
{
	public static (ushort modelSet, ushort modelBase, ushort modelVariant) SplitString(string str)
	{
		string[] parts = str.Split(",", StringSplitOptions.RemoveEmptyEntries);

		ushort modelSet = 0;
		ushort modelBase = 0;
		ushort modelVariant = 0;

		if (parts.Length == 3)
		{
			modelSet = ushort.Parse(parts[0].Trim());
			modelBase = ushort.Parse(parts[1].Trim());
			modelVariant = ushort.Parse(parts[2].Trim());
		}
		else
		{
			modelBase = ushort.Parse(parts[0].Trim());
			modelVariant = ushort.Parse(parts[1].Trim());
		}

		return (modelSet, modelBase, modelVariant);
	}

	public static IItem FromString(string str)
	{
		if (str.Contains(","))
		{
			(ushort modelSet, ushort modelBase, ushort modelVariant) = SplitString(str);
			return ItemUtility.GetDummyItem(modelSet, modelBase, modelVariant);
		}
		else
		{
			if (GameDataService.Items == null)
				throw new Exception("No items in game data service");

			return GameDataService.Items.Get(uint.Parse(str));
		}
	}

	public override IItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception($"Invalid serialized item value");

		return FromString(str);
	}

	public override void Write(Utf8JsonWriter writer, IItem value, JsonSerializerOptions options)
	{
		if (value.RowId != 0)
		{
			writer.WriteStringValue($"{value.RowId}");
		}
		else
		{
			if (value.IsWeapon)
			{
				writer.WriteStringValue($"{value.ModelSet}, {value.ModelBase}, {value.ModelVariant}");
			}
			else
			{
				writer.WriteStringValue($"{value.ModelBase}, {value.ModelVariant}");
			}
		}
	}
}
