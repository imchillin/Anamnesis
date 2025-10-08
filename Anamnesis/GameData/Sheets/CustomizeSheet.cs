// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class CustomizeSheet
{
	/// <summary>
	/// The excel sheet table length for all hair options.
	/// </summary>
	private const uint HAIR_OPTIONS_LENGTH = 130;
	private const uint FACE_PAINT_OPTIONS_LENGTH = 50;

	private static readonly Dictionary<(Features, ActorCustomizeMemory.Tribes, ActorCustomizeMemory.Genders), Dictionary<byte, CharaMakeCustomize>> s_featureCache = [];

	/// <summary>
	/// A lookup table for the start indices for all supported character customization features.
	/// </summary>
	private static readonly Dictionary<(Features, ActorCustomizeMemory.Tribes, ActorCustomizeMemory.Genders), uint> s_featureStartIndexMap = new()
	{
		// Hyur (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Masculine), 0 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Feminine), 130 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Masculine), 260 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Feminine), 390 },

		// Elezen (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Masculine), 520 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Feminine), 650 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Masculine), 520 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Feminine), 650 },

		// Lalafell (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Masculine), 780 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Feminine), 910 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Masculine), 780 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Feminine), 910 },

		// Miqo'te (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine), 1040 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine), 1170 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine), 1040 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine), 1170 },

		// Roegadyn (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Masculine), 1300 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Feminine), 1430 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Masculine), 1300 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Feminine), 1430 },

		// Au Ra (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Masculine), 1560 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Feminine), 1690 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Masculine), 1560 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Feminine), 1690 },

		// Hrothgar (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Masculine), 1820 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Feminine), 1950 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Masculine), 1820 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Feminine), 1950 },

		// Viera (Hair)
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine), 2080 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine), 2210 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine), 2080 },
		{ (Features.Hair, ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine), 2210 },

		// Hyur (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Masculine), 2400 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Feminine), 2450 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Masculine), 2500 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Feminine), 2550 },

		// Elezen (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Masculine), 2600 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Feminine), 2650 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Masculine), 2700 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Feminine), 2750 },

		// Lalafell (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Masculine), 2800 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Feminine), 2850 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Masculine), 2900 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Feminine), 2950 },

		// Miqo'te (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine), 3000 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine), 3050 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine), 3100 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine), 3150 },

		// Roegadyn (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Masculine), 3200 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Feminine), 3250 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Masculine), 3300 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Feminine), 3350 },

		// Au Ra (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Masculine), 3400 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Feminine), 3450 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Masculine), 3500 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Feminine), 3550 },

		// Hrothgar (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Masculine), 3600 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Feminine), 3650 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Masculine), 3700 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Feminine), 3750 },

		// Viera (Face Paint)
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine), 3800 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine), 3850 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine), 3900 },
		{ (Features.FacePaint, ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine), 3950 },
	};

	/// <summary>
	/// Represents the supported character customization features.
	/// </summary>
	public enum Features
	{
		/// <summary>Represents the hair feature.</summary>
		Hair,

		/// <summary>Represents the face paint feature.</summary>
		FacePaint,
	}

	/// <summary>
	/// Retrieves all feature options for a provided feature type, tribe, and gender.
	/// </summary>
	/// <param name="self">The Excel sheet.</param>
	/// <param name="featureType">The feature type.</param>
	/// <param name="tribe">The tribe.</param>
	/// <param name="gender">The gender.</param>
	/// <returns>The list of feature options.</returns>
	/// <exception cref="Exception">Thrown if the feature type, tribe, and gender combination is unrecognized.</exception>
	public static List<CharaMakeCustomize> GetFeatureOptions(
		this ExcelSheet<CharaMakeCustomize> self,
		Features featureType,
		ActorCustomizeMemory.Tribes tribe,
		ActorCustomizeMemory.Genders gender)
	{
		var lookup = GetOrBuildFeatureLookup(self, featureType, tribe, gender);
		return [.. lookup.Values];
	}

	/// <summary>
	/// Finds a feature option by its feature ID, feature type, tribe, and gender.
	/// </summary>
	/// <param name="self">The Excel sheet.</param>
	/// <param name="featureType">The feature type.</param>
	/// <param name="tribe">The tribe.</param>
	/// <param name="gender">The gender.</param>
	/// <param name="featureId">The feature ID.</param>
	/// <returns>The feature option if found; otherwise, <c>null</c>.</returns>
	/// <exception cref="Exception">Thrown if the feature type, tribe, and gender combination is unrecognized.</exception>
	public static CharaMakeCustomize? GetFeature(
		this ExcelSheet<CharaMakeCustomize> self,
		Features featureType,
		ActorCustomizeMemory.Tribes tribe,
		ActorCustomizeMemory.Genders gender,
		byte featureId)
	{
		var lookup = GetOrBuildFeatureLookup(self, featureType, tribe, gender);
		return lookup.TryGetValue(featureId, out var feature) ? feature : null;
	}

	/// <summary>
	/// Retrieves the row length of a feature type.
	/// </summary>
	/// <param name="featureType">The feature type.</param>
	/// <returns>The row length of the feature type.</returns>
	/// <exception cref="Exception">Thrown if the feature type is unrecognized.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint GetFeatureLength(Features featureType)
	{
		return featureType switch
		{
			Features.Hair => HAIR_OPTIONS_LENGTH,
			Features.FacePaint => FACE_PAINT_OPTIONS_LENGTH,
			_ => throw new Exception("Unrecognized feature: " + featureType),
		};
	}

	/// <summary>
	/// Gets or builds a lookup dictionary for character customization features based on the specified feature type.
	/// </summary>
	/// <param name="self">The <see cref="ExcelSheet{T}"/> containing the character customization data.</param>
	/// <param name="featureType">The type of feature to look up, such as hair or face paint.</param>
	/// <param name="tribe">The tribe of the character.</param>
	/// <param name="gender">The gender of the character.</param>
	/// <returns>
	/// A dictionary where the keys are feature IDs and the values are <see cref="CharaMakeCustomize"/> objects
	/// representing the corresponding customization features.
	/// </returns>
	/// <exception cref="Exception">Thrown if the feature type, tribe, and gender combination is unrecognized.</exception>
	private static Dictionary<byte, CharaMakeCustomize> GetOrBuildFeatureLookup(
		ExcelSheet<CharaMakeCustomize> self,
		Features featureType,
		ActorCustomizeMemory.Tribes tribe,
		ActorCustomizeMemory.Genders gender)
	{
		var key = (featureType, tribe, gender);
		if (!s_featureCache.TryGetValue(key, out var lookup))
		{
			if (!s_featureStartIndexMap.TryGetValue(key, out uint startIndex))
				throw new Exception("Unrecognized feature type, tribe, and gender combination");

			uint count = GetFeatureLength(featureType);
			lookup = BuildFeatureLookup(self, startIndex, count, featureType);
			s_featureCache[key] = lookup;
		}

		return lookup;
	}

	/// <summary>
	/// Builds a lookup dictionary of character customization features based on the specified range and feature type.
	/// </summary>
	/// <param name="self">The <see cref="ExcelSheet{T}"/> containing the character customization data.</param>
	/// <param name="startIndex">The starting index of the range to process.</param>
	/// <param name="count">The number of rows to process starting from <paramref name="startIndex"/>.</param>
	/// <param name="featureType">The type of feature to include in the lookup, such as hair or face paint.</param>
	/// <returns>
	/// A dictionary where the keys are feature IDs and the values are <see cref="CharaMakeCustomize"/> objects
	/// representing the corresponding customization features.
	/// </returns>
	private static Dictionary<byte, CharaMakeCustomize> BuildFeatureLookup(
		ExcelSheet<CharaMakeCustomize> self, uint startIndex, uint count, Features featureType)
	{
		var dict = new Dictionary<byte, CharaMakeCustomize>();
		for (uint i = startIndex; i < startIndex + count; i++)
		{
			var feature = self.GetRow(i);
			if (feature.Equals(default(CharaMakeCustomize)))
				continue;

			// Include a "no feature" option for face paint but not for hair.
			if (featureType != Features.FacePaint && feature.FeatureId == 0)
				continue;

			dict[feature.FeatureId] = feature;
		}

		return dict;
	}
}
