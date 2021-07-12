// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Customize
	{
		public Races Race;
		public Genders Gender;
		public Ages Age;
		public byte Height;
		public Tribes Tribe;
		public byte Head;
		public byte Hair;
		public byte HighlightType;
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

		public static Customize Default()
		{
			Customize ap = default;

			ap.Race = Races.Hyur;
			ap.Gender = Genders.Feminine;
			ap.Age = Ages.Normal;
			ap.Tribe = Tribes.Midlander;

			return ap;
		}
	}

	#pragma warning disable SA1402
	public class CustomizeViewModel : StructViewModelBase<Customize>
	{
		public CustomizeViewModel(ActorViewModel parent)
			: base(parent, nameof(ActorViewModel.Customize))
		{
		}

		public CustomizeViewModel(ActorViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public Customize.Races Race { get; set; }
		[ModelField] public Customize.Genders Gender { get; set; }
		[ModelField] public Customize.Ages Age { get; set; }
		[ModelField] public byte Height { get; set; }
		[ModelField] public Customize.Tribes Tribe { get; set; }
		[ModelField] public byte Head { get; set; }
		[ModelField] public byte Hair { get; set; }
		[ModelField] public byte HighlightType { get; set; }
		[ModelField] public byte Skintone { get; set; }
		[ModelField] public byte REyeColor { get; set; }
		[ModelField] public byte HairTone { get; set; }
		[ModelField] public byte Highlights { get; set; }
		[ModelField] public Customize.FacialFeature FacialFeatures { get; set; }
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

		public bool EnableHighlights
		{
			get => this.HighlightType != 0;
			set => this.HighlightType = value ? (byte)128 : (byte)0;
		}

		public byte Lips
		{
			get => (byte)(this.EnableLipColor ? this.Mouth - 128 : this.Mouth);
			set => this.Mouth = (byte)(this.EnableLipColor ? value - 128 : value);
		}

		public bool EnableLipColor
		{
			get => this.Mouth >= 128;
			set => this.Mouth = (byte)(this.Lips + (value ? 128 : 0));
		}
	}
}
