// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using System;
	using ConceptMatrix;
	using ConceptMatrix.AppearanceModule.ViewModels;

	[Serializable]
	public class AppearanceSetFile : FileBase
	{
		public static readonly FileType FileType = new FileType("cm3ap", "Appearance Set", typeof(AppearanceSetFile));

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
		public byte Lips { get; set; }
		public byte LipsToneFurPattern { get; set; }
		public byte EarMuscleTailSize { get; set; }
		public byte TailEarsType { get; set; }
		public byte Bust { get; set; }
		public byte FacePaint { get; set; }
		public byte FacePaintColor { get; set; }

		public override FileType GetFileType()
		{
			return FileType;
		}

		public void Read(AppearanceViewModel vm)
		{
			this.Race = vm.Race;
			this.Gender = vm.Gender;
			this.Age = vm.Age;
			this.Height = vm.Height;
			this.Tribe = vm.Tribe;
			this.Head = vm.Head;
			this.Hair = vm.Hair;
			this.EnableHighlights = vm.EnableHighlights;
			this.Skintone = vm.Skintone;
			this.REyeColor = vm.REyeColor;
			this.HairTone = vm.HairTone;
			this.Highlights = vm.Highlights;
			this.FacialFeatures = vm.FacialFeatures;
			this.LimbalEyes = vm.LimbalEyes;
			this.Eyebrows = vm.Eyebrows;
			this.LEyeColor = vm.LEyeColor;
			this.Eyes = vm.Eyes;
			this.Nose = vm.Nose;
			this.Jaw = vm.Jaw;
			this.Lips = vm.Mouth;
			this.LipsToneFurPattern = vm.LipsToneFurPattern;
			this.EarMuscleTailSize = vm.EarMuscleTailSize;
			this.TailEarsType = vm.TailEarsType;
			this.Bust = vm.Bust;
			this.FacePaint = vm.FacePaint;
			this.FacePaintColor = vm.FacePaintColor;
		}

		public void Write(AppearanceViewModel vm)
		{
			vm.Race = this.Race;
			vm.Gender = this.Gender;
			vm.Age = this.Age;
			vm.Height = this.Height;
			vm.Tribe = this.Tribe;
			vm.Head = this.Head;
			vm.Hair = this.Hair;
			vm.EnableHighlights = this.EnableHighlights;
			vm.Skintone = this.Skintone;
			vm.REyeColor = this.REyeColor;
			vm.HairTone = this.HairTone;
			vm.Highlights = this.Highlights;
			vm.FacialFeatures = this.FacialFeatures;
			vm.LimbalEyes = this.LimbalEyes;
			vm.Eyebrows = this.Eyebrows;
			vm.LEyeColor = this.LEyeColor;
			vm.Eyes = this.Eyes;
			vm.Nose = this.Nose;
			vm.Jaw = this.Jaw;
			vm.Mouth = this.Lips;
			vm.LipsToneFurPattern = this.LipsToneFurPattern;
			vm.EarMuscleTailSize = this.EarMuscleTailSize;
			vm.TailEarsType = this.TailEarsType;
			vm.Bust = this.Bust;
			vm.FacePaint = this.FacePaint;
			vm.FacePaintColor = this.FacePaintColor;
		}
	}
}
