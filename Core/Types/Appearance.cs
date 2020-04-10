// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;

	public struct Appearance
	{
		public Races Race;
		public Genders Gender;
		public byte Body;
		public byte Height;
		public Tribes Tribe;
		public byte Head;
		public byte Hair;
		public byte Highlights;
		public byte Skintone;
		public byte REyeColor;
		public byte HairTone;
		public byte HighlightTone;
		public byte FacialFeatures;
		public byte LimbalEyes;
		public byte EyeBrows;
		public byte LEyeColor;
		public byte Eyes;
		public byte Nose;
		public byte Jaw;
		public byte Lips;
		public byte LipsTone;
		public byte Muscle;
		public byte TailEarsType;
		public byte Bust;
		public byte FacePaint;
		public byte FacePaintColor;

		public enum Genders : byte
		{
			Masculine = 0,
			Feminine = 1,
		}

		public enum Races : byte
		{
			Hyur = 1,
			Elezen = 2,
			Lalafel = 3,
			Miqote = 4,
			Roegadyn = 5,
			AuRa = 6,
			Hrothgar = 7,
			Vierra = 8,
		}

		public enum Tribes : byte
		{
			Midlander = 1,
			Highlander = 2,
			Wildwood = 3,
			Duskwight = 4,
			Plainsfolk = 5,
			Dunesfolk = 6,
			SeekerOfTheSun = 7,
			KeeperOfTheMoon = 8,
			SeaWolf = 9,
			Hellsguard = 10,
			Raen = 11,
			Xaela = 12,
			Helions = 13,
			TheLost = 14,
			Rava = 15,
			Veena = 16,
		}
	}

	public static class RacesExtensions
	{
		public static Appearance.Tribes[] GetTribes(this Appearance.Races race)
		{
			switch (race)
			{
				case Appearance.Races.Hyur: return new[] { Appearance.Tribes.Midlander, Appearance.Tribes.Highlander };
				case Appearance.Races.Elezen: return new[] { Appearance.Tribes.Wildwood, Appearance.Tribes.Duskwight };
				case Appearance.Races.Lalafel: return new[] { Appearance.Tribes.Plainsfolk, Appearance.Tribes.Dunesfolk };
				case Appearance.Races.Miqote: return new[] { Appearance.Tribes.SeekerOfTheSun, Appearance.Tribes.KeeperOfTheMoon };
				case Appearance.Races.Roegadyn: return new[] { Appearance.Tribes.SeaWolf, Appearance.Tribes.Hellsguard };
				case Appearance.Races.AuRa: return new[] { Appearance.Tribes.Raen, Appearance.Tribes.Xaela };
				case Appearance.Races.Hrothgar: return new[] { Appearance.Tribes.Helions, Appearance.Tribes.TheLost };
				case Appearance.Races.Vierra: return new[] { Appearance.Tribes.Rava, Appearance.Tribes.Veena };
			}

			throw new Exception("Unrecognized race: " + race);
		}
	}
}
