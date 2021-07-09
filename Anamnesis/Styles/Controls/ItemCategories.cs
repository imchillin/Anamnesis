// © Anamnesis.
// Licensed under the MIT license.

#pragma warning disable SA1649
namespace Anamnesis.Styles.Controls
{
	using System;

	[Flags]
	public enum ItemCategories
	{
		None = 0,

		Items = 1 << 0,
		Props = 1 << 1,
		Performance = 1 << 2,
		Modded = 1 << 3,
		Favorites = 1 << 4,

		All = Items | Props | Performance | Modded | Favorites,
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
