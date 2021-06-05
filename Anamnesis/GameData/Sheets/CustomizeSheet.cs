// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections.Generic;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Memory;
	using Lumina;
	using Lumina.Excel.GeneratedSheets;

	public class CustomizeSheet : LuminaSheet<ICharaMakeCustomize, CharaMakeCustomize, CharaMakeCustomizeViewModel>, ICharaMakeCustomizeData
	{
		public CustomizeSheet(GameData lumina)
			: base(lumina)
		{
		}

		public List<ICharaMakeCustomize> GetFeatureOptions(Features featureType, Appearance.Tribes tribe, Appearance.Genders gender)
		{
			List<ICharaMakeCustomize> results = new List<ICharaMakeCustomize>();
			uint fromIndex = (uint)this.GetFeatureStartIndex(featureType, tribe, gender);
			int count = this.GetFeatureLength(featureType);

			for (int i = 1; i < 200; i++)
			{
				ICharaMakeCustomize? feature = this.FindFeatureById(fromIndex, count, (byte)i);

				if (feature == null)
					continue;

				results.Add(feature);
			}

			return results;
		}

		public ICharaMakeCustomize? GetFeature(Features featureType, Appearance.Tribes tribe, Appearance.Genders gender, byte featureId)
		{
			List<ICharaMakeCustomize> hairs = this.GetFeatureOptions(featureType, tribe, gender);
			foreach (ICharaMakeCustomize hair in hairs)
			{
				if (hair.FeatureId == featureId)
				{
					return hair;
				}
			}

			return null;
		}

		private int GetFeatureStartIndex(Features featureType, Appearance.Tribes tribe, Appearance.Genders gender)
		{
			bool isMale = gender == Appearance.Genders.Masculine;

			if (featureType == Features.Hair)
			{
				switch (tribe)
				{
					case Appearance.Tribes.Midlander: return isMale ? 0 : 100;
					case Appearance.Tribes.Highlander: return isMale ? 200 : 300;
					case Appearance.Tribes.Wildwood: return isMale ? 400 : 500;
					case Appearance.Tribes.Duskwight: return isMale ? 400 : 500;
					case Appearance.Tribes.Plainsfolk: return isMale ? 600 : 700;
					case Appearance.Tribes.Dunesfolk: return isMale ? 600 : 700;
					case Appearance.Tribes.SeekerOfTheSun: return isMale ? 800 : 900;
					case Appearance.Tribes.KeeperOfTheMoon: return isMale ? 800 : 900;
					case Appearance.Tribes.SeaWolf: return isMale ? 1000 : 1100;
					case Appearance.Tribes.Hellsguard: return isMale ? 1000 : 1100;
					case Appearance.Tribes.Raen: return isMale ? 1200 : 1300;
					case Appearance.Tribes.Xaela: return isMale ? 1200 : 1300;
					case Appearance.Tribes.Helions: return 1400;
					case Appearance.Tribes.TheLost: return 1400;
					case Appearance.Tribes.Rava: return 1500;
					case Appearance.Tribes.Veena: return 1500;
				}
			}
			else if (featureType == Features.FacePaint)
			{
				switch (tribe)
				{
					case Appearance.Tribes.Midlander: return isMale ? 1600 : 1650;
					case Appearance.Tribes.Highlander: return isMale ? 1700 : 1750;
					case Appearance.Tribes.Wildwood: return isMale ? 1800 : 1850;
					case Appearance.Tribes.Duskwight: return isMale ? 1900 : 1950;
					case Appearance.Tribes.Plainsfolk: return isMale ? 2000 : 2050;
					case Appearance.Tribes.Dunesfolk: return isMale ? 2100 : 2150;
					case Appearance.Tribes.SeekerOfTheSun: return isMale ? 2200 : 2250;
					case Appearance.Tribes.KeeperOfTheMoon: return isMale ? 2300 : 2350;
					case Appearance.Tribes.SeaWolf: return isMale ? 2400 : 2450;
					case Appearance.Tribes.Hellsguard: return isMale ? 2500 : 2550;
					case Appearance.Tribes.Raen: return isMale ? 2600 : 2650;
					case Appearance.Tribes.Xaela: return isMale ? 2700 : 2750;
					case Appearance.Tribes.Helions: return 2800;
					case Appearance.Tribes.TheLost: return 2850;
					case Appearance.Tribes.Rava: return 2900;
					case Appearance.Tribes.Veena: return 2950;
				}
			}

			throw new Exception("Unrecognized tribe: " + tribe);
		}

		private int GetFeatureLength(Features featureType)
		{
			switch (featureType)
			{
				case Features.Hair: return 100;
				case Features.FacePaint: return 50;
			}

			throw new Exception("Unrecognized feature: " + featureType);
		}

		private ICharaMakeCustomize? FindFeatureById(uint from, int length, byte value)
		{
			for (uint i = from; i < from + length; i++)
			{
				ICharaMakeCustomize feature = this.Get(i);

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
}
