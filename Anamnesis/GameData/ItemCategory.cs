// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;

	public class ItemCategory : JsonDictionarySheet<ItemCategories, ItemCategory>.Entry
	{
		public static ItemCategories GetCategory(Item item)
		{
			if (!GameDataService.ItemCategories.Contains(item.RowId))
				return ItemCategories.None;

			ItemCategories category = GameDataService.ItemCategories.Get(item.RowId).Value;

			if (FavoritesService.IsFavorite(item))
				category = category.SetFlag(ItemCategories.Favorites, true);

			if (FavoritesService.IsOwned(item))
				category = category.SetFlag(ItemCategories.Owned, true);

			return category;
		}

		public override void SetValue(ItemCategories value)
		{
			base.SetValue(value);
		}
	}

#pragma warning disable

	[Flags]
	public enum ItemCategories
	{
		None = 0,

		Standard = 1 << 0,
		Premium = 1 << 1,
		Limited = 1 << 2,
		Deprecated = 1 << 3,
		Props = 1 << 4,
		Performance = 1 << 5,
		Modded = 1 << 6,
		Favorites = 1 << 7,
		Owned = 1 << 8,

		All = Standard | Premium | Limited | Deprecated | Props | Performance | Modded | Favorites | Owned,
	}

	public static class ItemCategoriesExtensions
	{
		public static ItemCategories SetFlag(this ItemCategories a, ItemCategories b, bool enabled)
		{
			if (enabled)
			{
				return a | b;
			}
			else
			{
				return a & ~b;
			}
		}
	}
}