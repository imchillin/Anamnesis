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

		[MarshalAs(UnmanagedType.I1)]
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

		public AppearanceViewModel(ActorViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public Appearance.Races Race { get; set; }
		[ModelField] public Appearance.Genders Gender { get; set; }
		[ModelField] public Appearance.Ages Age { get; set; }
		[ModelField] public byte Height { get; set; }
		[ModelField] public Appearance.Tribes Tribe { get; set; }
		[ModelField] public byte Head { get; set; }
		[ModelField] public byte Hair { get; set; }
		[ModelField] public bool EnableHighlights { get; set; }
		[ModelField] public byte Skintone { get; set; }
		[ModelField] public byte REyeColor { get; set; }
		[ModelField] public byte HairTone { get; set; }
		[ModelField] public byte Highlights { get; set; }
		[ModelField] public Appearance.FacialFeature FacialFeatures { get; set; }
		[ModelField] public byte LimbalEyes { get; set; }
		[ModelField] public byte Eyebrows { get; set; }
		[ModelField] public byte LEyeColor { get; set; }
		[ModelField] public byte Eyes { get; set; }
		[ModelField] public byte Nose { get; set; }
		[ModelField] public byte Jaw { get; set; }
		[ModelField] public byte Mouth { get; set; }
		[ModelField] public byte LipsToneFurPattern { get; set; }
		[ModelField] public byte EarMuscleTailSize { get; set; }
		[ModelField] public byte TailEarsType { get; set; }
		[ModelField] public byte Bust { get; set; }
		[ModelField] public byte FacePaint { get; set; }
		[ModelField] public byte FacePaintColor { get; set; }
	}
}
