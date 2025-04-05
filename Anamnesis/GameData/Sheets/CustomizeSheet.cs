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
	private const uint HairOptionsLength = 130;
	private const uint FacePaintOptionsLength = 50;

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
	/// A lookup table for the start indices for all supported character customization features.
	/// </summary>
	private static readonly Dictionary<(Features, ActorCustomizeMemory.Tribes, ActorCustomizeMemory.Genders), uint> FeatureStartIndexMap = new()
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
	/// Retrieves all feature options for a provided feature type, tribe, and gender.
	/// </summary>
	/// <param name="self">The Excel sheet.</param>
	/// <param name="featureType">The feature type.</param>
	/// <param name="tribe">The tribe.</param>
	/// <param name="gender">The gender.</param>
	/// <returns>The list of feature options.</returns>
	/// <exception cref="Exception">Thrown if the feature type, tribe, and gender combination is unrecognized.</exception>
	public static List<CharaMakeCustomize> GetFeatureOptions(this ExcelSheet<CharaMakeCustomize> self, Features featureType, ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender)
	{
		List<CharaMakeCustomize> results = [];

		if (!FeatureStartIndexMap.TryGetValue((featureType, tribe, gender), out uint startIndex))
			throw new Exception("Unrecognized feature type, tribe, and gender combination");

		uint count = GetFeatureLength(featureType);
		for (byte i = (featureType != Features.FacePaint) ? (byte)1 : (byte)0; i < byte.MaxValue; i++)
		{
			CharaMakeCustomize? feature = self.FindFeatureById(startIndex, count, i);

			if (!feature.HasValue)
				continue;

			results.Add(feature.Value);
		}

		return results;
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
	public static CharaMakeCustomize? GetFeature(this ExcelSheet<CharaMakeCustomize> self, Features featureType, ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender, byte featureId)
	{
		List<CharaMakeCustomize> featureOptions = self.GetFeatureOptions(featureType, tribe, gender);
		foreach (CharaMakeCustomize featureOption in featureOptions)
		{
			if (featureOption.FeatureId == featureId)
				return featureOption;
		}

		return null;
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
			Features.Hair => HairOptionsLength,
			Features.FacePaint => FacePaintOptionsLength,
			_ => throw new Exception("Unrecognized feature: " + featureType),
		};
	}

	/// <summary>
	/// Finds a feature option by its feature ID within a range of rows.
	/// </summary>
	/// <param name="self">The Excel sheet.</param>
	/// <param name="from">The starting row index.</param>
	/// <param name="length">The row length.</param>
	/// <param name="featureId">The sub.</param>
	/// <returns>The feature option if found; otherwise, <c>null</c>.</returns>
	private static CharaMakeCustomize? FindFeatureById(this ExcelSheet<CharaMakeCustomize> self, uint from, uint length, byte featureId)
	{
		for (uint i = from; i < from + length; i++)
		{
			CharaMakeCustomize feature = self.GetRow(i);

			if (feature.Equals(default(CharaMakeCustomize)))
				continue;

			if (feature.FeatureId == featureId)
				return feature;
		}

		return null;
	}
}
