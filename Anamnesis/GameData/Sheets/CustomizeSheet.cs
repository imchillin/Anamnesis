// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using System;
using System.Collections.Generic;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;

public static class CustomizeSheet
{
	public enum Features
	{
		Hair,
		FacePaint,
	}

	public static List<CharaMakeCustomize> GetFeatureOptions(this ExcelSheet<CharaMakeCustomize> self, Features featureType, ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender)
	{
		List<CharaMakeCustomize> results = new List<CharaMakeCustomize>();
		uint fromIndex = (uint)GetFeatureStartIndex(featureType, tribe, gender);
		int count = GetFeatureLength(featureType);

		for (int i = 1; i < 200; i++)
		{
			CharaMakeCustomize? feature = self.FindFeatureById(fromIndex, count, (byte)i);

			if (feature == null)
				continue;

			results.Add(feature);
		}

		return results;
	}

	public static CharaMakeCustomize? GetFeature(this ExcelSheet<CharaMakeCustomize> self, Features featureType, ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender, byte featureId)
	{
		List<CharaMakeCustomize> hairs = self.GetFeatureOptions(featureType, tribe, gender);
		foreach (CharaMakeCustomize hair in hairs)
		{
			if (hair.FeatureId == featureId)
			{
				return hair;
			}
		}

		return null;
	}

	private static int GetFeatureStartIndex(Features featureType, ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender)
	{
		bool isMasc = gender == ActorCustomizeMemory.Genders.Masculine;

		if (featureType == Features.Hair)
		{
			switch (tribe)
			{
				case ActorCustomizeMemory.Tribes.Midlander: return isMasc ? 0 : 130;
				case ActorCustomizeMemory.Tribes.Highlander: return isMasc ? 260 : 390;
				case ActorCustomizeMemory.Tribes.Wildwood: return isMasc ? 520 : 650;
				case ActorCustomizeMemory.Tribes.Duskwight: return isMasc ? 520 : 650;
				case ActorCustomizeMemory.Tribes.Plainsfolk: return isMasc ? 780 : 910;
				case ActorCustomizeMemory.Tribes.Dunesfolk: return isMasc ? 780 : 910;
				case ActorCustomizeMemory.Tribes.SeekerOfTheSun: return isMasc ? 1040 : 1170;
				case ActorCustomizeMemory.Tribes.KeeperOfTheMoon: return isMasc ? 1040 : 1170;
				case ActorCustomizeMemory.Tribes.SeaWolf: return isMasc ? 1300 : 1430;
				case ActorCustomizeMemory.Tribes.Hellsguard: return isMasc ? 1300 : 1430;
				case ActorCustomizeMemory.Tribes.Raen: return isMasc ? 1560 : 1690;
				case ActorCustomizeMemory.Tribes.Xaela: return isMasc ? 1560 : 1690;
				case ActorCustomizeMemory.Tribes.Helions: return isMasc ? 1820 : 1950;
				case ActorCustomizeMemory.Tribes.TheLost: return isMasc ? 1820 : 1950;
				case ActorCustomizeMemory.Tribes.Rava: return isMasc ? 2080 : 2210;
				case ActorCustomizeMemory.Tribes.Veena: return isMasc ? 2080 : 2210;
			}
		}
		else if (featureType == Features.FacePaint)
		{
			switch (tribe)
			{
				case ActorCustomizeMemory.Tribes.Midlander: return isMasc ? 2400 : 2450;
				case ActorCustomizeMemory.Tribes.Highlander: return isMasc ? 2500 : 2550;
				case ActorCustomizeMemory.Tribes.Wildwood: return isMasc ? 2600 : 2650;
				case ActorCustomizeMemory.Tribes.Duskwight: return isMasc ? 2700 : 2750;
				case ActorCustomizeMemory.Tribes.Plainsfolk: return isMasc ? 2800 : 2850;
				case ActorCustomizeMemory.Tribes.Dunesfolk: return isMasc ? 2900 : 2950;
				case ActorCustomizeMemory.Tribes.SeekerOfTheSun: return isMasc ? 3000 : 3050;
				case ActorCustomizeMemory.Tribes.KeeperOfTheMoon: return isMasc ? 3100 : 3150;
				case ActorCustomizeMemory.Tribes.SeaWolf: return isMasc ? 3200 : 3250;
				case ActorCustomizeMemory.Tribes.Hellsguard: return isMasc ? 3300 : 3350;
				case ActorCustomizeMemory.Tribes.Raen: return isMasc ? 3400 : 3450;
				case ActorCustomizeMemory.Tribes.Xaela: return isMasc ? 3500 : 3550;
				case ActorCustomizeMemory.Tribes.Helions: return isMasc ? 3600 : 3650;
				case ActorCustomizeMemory.Tribes.TheLost: return isMasc ? 3700 : 3750;
				case ActorCustomizeMemory.Tribes.Rava: return isMasc ? 3800 : 3850;
				case ActorCustomizeMemory.Tribes.Veena: return isMasc ? 3900 : 3950;
			}
		}

		throw new Exception("Unrecognized tribe: " + tribe);
	}

	private static int GetFeatureLength(Features featureType)
	{
		switch (featureType)
		{
			case Features.Hair: return 130;
			case Features.FacePaint: return 50;
		}

		throw new Exception("Unrecognized feature: " + featureType);
	}

	private static CharaMakeCustomize? FindFeatureById(this ExcelSheet<CharaMakeCustomize> self, uint from, int length, byte value)
	{
		for (uint i = from; i < from + length; i++)
		{
			CharaMakeCustomize feature = self.Get(i);

			if (feature == null)
				continue;

			if (feature.FeatureId == value)
			{
				return feature;
			}
		}

		return null;
	}
}
