// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System;

/// <summary>
/// Represents the categories that an item can belong to.
/// </summary>
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

public static class ItemCategoriesExtensions
{
	/// <summary>
	/// Modifies the flag of the given item category.
	/// </summary>
	/// <param name="a">The primary item category.</param>
	/// <param name="b">The item category to apply.</param>
	/// <param name="enabled">True to enable the flag(s), false to disable them.</param>
	/// <returns>A new item category with the modified flag(s).</returns>
	public static ItemCategories SetFlag(this ItemCategories a, ItemCategories b, bool enabled) => enabled ? a | b : a & ~b;
}
