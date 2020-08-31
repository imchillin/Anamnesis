// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Appearance
	{
		public Races Race;
		public Genders Gender;
		public Ages Age;
		public byte Height;
		public Tribes Tribe;
		public byte Head;
		public byte Hair;
		public bool EnableHighlights;
		public byte Skintone;
		public byte REyeColor;
		public byte HairTone;
		public byte Highlights;
		public FacialFeature FacialFeatures;
		public byte LimbalEyes;
		public byte Eyebrows;
		public byte LEyeColor;
		public byte Eyes;
		public byte Nose;
		public byte Jaw;
		public byte Mouth;
		public byte LipsToneFurPattern;
		public byte EarMuscleTailSize;
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
			Viera = 8,
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

		public enum Ages : byte
		{
			Normal = 1,
			Old = 3,
			Young = 4,
		}

		[Flags]
		public enum FacialFeature : byte
		{
			None = 0,
			First = 1,
			Second = 2,
			Third = 4,
			Fourth = 8,
			Fifth = 16,
			Sixth = 32,
			Seventh = 64,
			LegacyTattoo = 128,
		}

		public static Appearance Default()
		{
			Appearance ap = default;

			ap.Race = Races.Hyur;
			ap.Gender = Genders.Feminine;
			ap.Age = Ages.Normal;
			ap.Tribe = Tribes.Midlander;

			return ap;
		}
	}

	#pragma warning disable SA1402
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
				case Appearance.Races.Viera: return new[] { Appearance.Tribes.Rava, Appearance.Tribes.Veena };
			}

			throw new Exception("Unrecognized race: " + race);
		}
	}

	public class AppearanceViewModel : MemoryViewModelBase<Appearance>
	{
		public AppearanceViewModel(IntPtr pointer)
			: base(pointer)
		{
		}

		public AppearanceViewModel(ActorViewModel parent)
			: base(parent, nameof(ActorViewModel.Customize))
		{
		}

		public Appearance.Races Race { get; set; }
		public Appearance.Genders Gender { get; set; }
		public Appearance.Ages Age { get; set; }
		public byte Height { get; set; }
		public Appearance.Tribes Tribe { get; set; }
		public byte Head { get; set; }
		public byte Hair { get; set; }
		public bool EnableHighlights { get; set; }
		public byte Skintone { get; set; }
		public byte REyeColor { get; set; }
		public byte HairTone { get; set; }
		public byte Highlights { get; set; }
		public Appearance.FacialFeature FacialFeatures { get; set; }
		public byte LimbalEyes { get; set; }
		public byte Eyebrows { get; set; }
		public byte LEyeColor { get; set; }
		public byte Eyes { get; set; }
		public byte Nose { get; set; }
		public byte Jaw { get; set; }
		public byte Mouth { get; set; }
		public byte LipsToneFurPattern { get; set; }
		public byte EarMuscleTailSize { get; set; }
		public byte TailEarsType { get; set; }
		public byte Bust { get; set; }
		public byte FacePaint { get; set; }
		public byte FacePaintColor { get; set; }
	}
}
