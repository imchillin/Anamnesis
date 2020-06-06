// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using Anamnesis;
	using ConceptMatrix;
	using ConceptMatrix.GameData;

	internal class CustomizeTable : Table<ICharaMakeCustomize>, ICharaMakeCustomizeData
	{
		public List<ICharaMakeCustomize> GetHair(Appearance.Tribes tribe, Appearance.Genders gender)
		{
			List<ICharaMakeCustomize> hairs = new List<ICharaMakeCustomize>();
			int fromIndex = this.GetStartIndex(gender, tribe);
			for (int i = 1; i < 200; i++)
			{
				ICharaMakeCustomize feature = this.FindHair(fromIndex, 100, (byte)i);

				if (feature == null)
					continue;

				hairs.Add(feature);
			}

			return hairs;
		}

		public ICharaMakeCustomize GetHair(Appearance.Tribes tribe, Appearance.Genders gender, byte featureId)
		{
			List<ICharaMakeCustomize> hairs = this.GetHair(tribe, gender);
			foreach (ICharaMakeCustomize hair in hairs)
			{
				if (hair.FeatureId == featureId)
				{
					return hair;
				}
			}

			return null;
		}

		private int GetStartIndex(Appearance.Genders gender, Appearance.Tribes tribe)
		{
			bool isMale = gender == Appearance.Genders.Masculine;

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

			throw new Exception("Unrecognized tribe: " + tribe);
		}

		private ICharaMakeCustomize FindHair(int from, int length, byte value)
		{
			for (int i = from; i < from + length; i++)
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
