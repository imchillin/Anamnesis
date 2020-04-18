// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using System;
	using ConceptMatrix;
	using ConceptMatrix.AppearanceModule.Views;

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

		public Color SkinTint { get; set; }
		public Color SkinGlow { get; set; }
		public Color LeftEyeColor { get; set; }
		public Color RightEyeColor { get; set; }
		public Color LimbalRingColor { get; set; }
		public Color HairTint { get; set; }
		public Color HairGlow { get; set; }
		public Color HighlightTint { get; set; }
		public Color4 LipTint { get; set; }

		public override FileType GetFileType()
		{
			return FileType;
		}

		public void Read(AppearanceEditor ed)
		{
			this.Race = ed.Appearance.Race;
			this.Gender = ed.Appearance.Gender;
			this.Age = ed.Appearance.Age;
			this.Height = ed.Appearance.Height;
			this.Tribe = ed.Appearance.Tribe;
			this.Head = ed.Appearance.Head;
			this.Hair = ed.Appearance.Hair;
			this.EnableHighlights = ed.Appearance.EnableHighlights;
			this.Skintone = ed.Appearance.Skintone;
			this.REyeColor = ed.Appearance.REyeColor;
			this.HairTone = ed.Appearance.HairTone;
			this.Highlights = ed.Appearance.Highlights;
			this.FacialFeatures = ed.Appearance.FacialFeatures;
			this.LimbalEyes = ed.Appearance.LimbalEyes;
			this.Eyebrows = ed.Appearance.Eyebrows;
			this.LEyeColor = ed.Appearance.LEyeColor;
			this.Eyes = ed.Appearance.Eyes;
			this.Nose = ed.Appearance.Nose;
			this.Jaw = ed.Appearance.Jaw;
			this.Lips = ed.Appearance.Mouth;
			this.LipsToneFurPattern = ed.Appearance.LipsToneFurPattern;
			this.EarMuscleTailSize = ed.Appearance.EarMuscleTailSize;
			this.TailEarsType = ed.Appearance.TailEarsType;
			this.Bust = ed.Appearance.Bust;
			this.FacePaint = ed.Appearance.FacePaint;
			this.FacePaintColor = ed.Appearance.FacePaintColor;

			this.SkinTint = ed.SkinTint;
			this.SkinGlow = ed.SkinGlow;
			this.LeftEyeColor = ed.LeftEyeColor;
			this.RightEyeColor = ed.RightEyeColor;
			this.LimbalRingColor = ed.LimbalRingColor;
			this.HairTint = ed.HairTint;
			this.HairGlow = ed.HairGlow;
			this.HighlightTint = ed.HighlightTint;
			this.LipTint = ed.LipTint;
		}

		public void WritePreRefresh(AppearanceEditor ed)
		{
			ed.Appearance.Race = this.Race;
			ed.Appearance.Gender = this.Gender;
			ed.Appearance.Age = this.Age;
			ed.Appearance.Height = this.Height;
			ed.Appearance.Tribe = this.Tribe;
			ed.Appearance.Head = this.Head;
			ed.Appearance.Hair = this.Hair;
			ed.Appearance.EnableHighlights = this.EnableHighlights;
			ed.Appearance.Skintone = this.Skintone;
			ed.Appearance.REyeColor = this.REyeColor;
			ed.Appearance.HairTone = this.HairTone;
			ed.Appearance.Highlights = this.Highlights;
			ed.Appearance.FacialFeatures = this.FacialFeatures;
			ed.Appearance.LimbalEyes = this.LimbalEyes;
			ed.Appearance.Eyebrows = this.Eyebrows;
			ed.Appearance.LEyeColor = this.LEyeColor;
			ed.Appearance.Eyes = this.Eyes;
			ed.Appearance.Nose = this.Nose;
			ed.Appearance.Jaw = this.Jaw;
			ed.Appearance.Mouth = this.Lips;
			ed.Appearance.LipsToneFurPattern = this.LipsToneFurPattern;
			ed.Appearance.EarMuscleTailSize = this.EarMuscleTailSize;
			ed.Appearance.TailEarsType = this.TailEarsType;
			ed.Appearance.Bust = this.Bust;
			ed.Appearance.FacePaint = this.FacePaint;
			ed.Appearance.FacePaintColor = this.FacePaintColor;
		}

		public void WritePostRefresh(AppearanceEditor ed)
		{
			ed.SkinTint = this.SkinTint;
			ed.SkinGlow = this.SkinGlow;
			ed.LeftEyeColor = this.LeftEyeColor;
			ed.RightEyeColor = this.RightEyeColor;
			ed.LimbalRingColor = this.LimbalRingColor;
			ed.HairTint = this.HairTint;
			ed.HairGlow = this.HairGlow;
			ed.HighlightTint = this.HighlightTint;
			ed.LipTint = this.LipTint;
		}
	}
}
