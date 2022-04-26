// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System;

[Flags]
public enum ItemCategories
{
	None = 0,

	Standard = 1 << 0,
	Premium = 1 << 1,
	Limited = 1 << 2,
	Deprecated = 1 << 3,
	CustomEquipment = 1 << 4,
	Performance = 1 << 5,
	Modded = 1 << 6,
	Favorites = 1 << 7,
	Owned = 1 << 8,

	All = Standard | Premium | Limited | Deprecated | CustomEquipment | Performance | Modded | Favorites | Owned,
}

#pragma warning disable SA1649
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
