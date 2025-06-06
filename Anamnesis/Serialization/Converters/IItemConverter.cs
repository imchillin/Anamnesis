// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Services;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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

	/// <summary>
	/// Splits a string based on the colon (:) character. Return a tuple such that:
	/// - The first part represents our favorite item prefix.
	/// - The second part represents the item's rowId, however it's derived.
	/// - The third (optional) part is for buddy equipment, representing the slot that was favorited in the set.
	/// </summary>
	public static (uint prefix, uint rowId, uint buddySlot) SplitPrefixedString(string str)
	{
		string[] parts = str.Split(":", StringSplitOptions.RemoveEmptyEntries);

		uint prefix = uint.Parse(parts[0].Trim());
		uint rowId = uint.Parse(parts[1].Trim());
		uint buddySlot = 0;

		if (parts.Length == 3)
		{
			buddySlot = ushort.Parse(parts[2].Trim());
		}

		return (prefix, rowId, buddySlot);
	}

	/// <summary>
	/// Reads a string and attempts to lookup the item it represents. There are three schemes:
	/// - #, #, # - Items which are favorited using their base, variant, set notation.
	/// - #:#:# - Items which are favorited using their prefix:rowId:buddySlot notation.
	/// - # - Items which are favorited using just their rowId, in the case of regular equipment.
	/// </summary>
	public static IItem FromString(string str)
	{
		if (str.Contains(","))
		{
			// For favorite items stored as #,#,#.
			(ushort modelSet, ushort modelBase, ushort modelVariant) = SplitString(str);
			return ItemUtility.GetDummyItem(0, modelSet, modelBase, modelVariant);
		}
		else if(str.Contains(":"))
		{
			// For favorite items stored as prefix:rowId:buddySlot.
			(uint prefix, uint rowId, uint buddySlot) = SplitPrefixedString(str);
			IItem? item = ItemUtility.GetItemByItemFavoriteProperties(prefix, rowId, buddySlot);

			if (item == null)
				throw new Exception("Error retrieving favorited item from game data.");

			return item;
		}
		else
		{
			// For favorite items stored as just a rowId.
			if (GameDataService.Items == null)
				throw new Exception("No items in game data service");

			return GameDataService.Items.GetRow(uint.Parse(str));
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
		// For items with a non-zero rowId OR are in a non-none Favorite Item Category.
		// Otherwise, write out the value in #, #, # notation.
		if (value.RowId != 0 || !value.FavoriteItemCategory.Equals(ItemFavoriteCategory.None))
		{
			uint favoriteItemCategoryPrefix = (uint)value.FavoriteItemCategory;

			switch(value.FavoriteItemCategory)
			{
				case ItemFavoriteCategory.None:
					writer.WriteStringValue($"{value.RowId}");
					break;
				case ItemFavoriteCategory.BuddyEquipment:
					uint buddyEquipSlotNum = ((GameData.Excel.BuddyEquip.BuddyItem)value).GetBuddyEquipSlotIntAlias();
					writer.WriteStringValue($"{favoriteItemCategoryPrefix}:{value.RowId}:{buddyEquipSlotNum}");
					break;
				default:
					writer.WriteStringValue($"{favoriteItemCategoryPrefix}:{value.RowId}");
					break;
			}
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
