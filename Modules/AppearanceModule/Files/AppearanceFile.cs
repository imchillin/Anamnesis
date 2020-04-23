// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using System;
	using ConceptMatrix;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.AppearanceModule.Views;

	[Serializable]
	public class AppearanceFile : FileBase
	{
		public static readonly FileType FileType = new FileType("cm3a", "Appearance", typeof(AppearanceFile));

		[Flags]
		public enum SaveModes
		{
			None = 0,

			EquipmentGear = 1,
			EquipmentAccessories = 2,
			AppearanceHair = 8,
			AppearanceFace = 16,
			AppearanceBody = 32,

			All = EquipmentGear | EquipmentAccessories | AppearanceHair | AppearanceFace | AppearanceBody,
		}

		public Appearance.Races? Race { get; set; }
		public Appearance.Genders? Gender { get; set; }
		public Appearance.Ages? Age { get; set; }
		public byte? Height { get; set; }
		public Appearance.Tribes? Tribe { get; set; }
		public byte? Head { get; set; }
		public byte? Hair { get; set; }
		public bool? EnableHighlights { get; set; }
		public byte? Skintone { get; set; }
		public byte? REyeColor { get; set; }
		public byte? HairTone { get; set; }
		public byte? Highlights { get; set; }
		public Appearance.FacialFeature? FacialFeatures { get; set; }
		public byte? LimbalEyes { get; set; }
		public byte? Eyebrows { get; set; }
		public byte? LEyeColor { get; set; }
		public byte? Eyes { get; set; }
		public byte? Nose { get; set; }
		public byte? Jaw { get; set; }
		public byte? Mouth { get; set; }
		public byte? LipsToneFurPattern { get; set; }
		public byte? EarMuscleTailSize { get; set; }
		public byte? TailEarsType { get; set; }
		public byte? Bust { get; set; }
		public byte? FacePaint { get; set; }
		public byte? FacePaintColor { get; set; }

		public Color? SkinTint { get; set; }
		public Color? SkinGlow { get; set; }
		public Color? LeftEyeColor { get; set; }
		public Color? RightEyeColor { get; set; }
		public Color? LimbalRingColor { get; set; }
		public Color? HairTint { get; set; }
		public Color? HairGlow { get; set; }
		public Color? HighlightTint { get; set; }
		public Color4? LipTint { get; set; }

		public Weapon MainHand { get; set; }
		public Vector? MainHandScale { get; set; } = Vector.One;
		public Color? MainHandColor { get; set; } = Color.White;
		public Weapon OffHand { get; set; }
		public Vector? OffHandScale { get; set; } = Vector.One;
		public Color? OffHandColor { get; set; } = Color.White;

		public Item HeadGear { get; set; }
		public Item Body { get; set; }
		public Item Hands { get; set; }
		public Item Legs { get; set; }
		public Item Feet { get; set; }
		public Item Ears { get; set; }
		public Item Neck { get; set; }
		public Item Wrists { get; set; }
		public Item LeftRing { get; set; }
		public Item RightRing { get; set; }

		public override FileType GetFileType()
		{
			return FileType;
		}

		public void Read(AppearanceEditor ed, EquipmentEditor eq, SaveModes mode)
		{
			if (mode.HasFlag(SaveModes.EquipmentGear))
			{
				this.MainHand = new Weapon(eq.MainHand);
				this.MainHandScale = eq.MainHand.Scale;
				this.MainHandColor = eq.MainHand.Color;

				this.OffHand = new Weapon(eq.OffHand);
				this.OffHandScale = eq.OffHand.Scale;
				this.OffHandColor = eq.OffHand.Color;

				this.HeadGear = new Item(eq.Head);
				this.Body = new Item(eq.Body);
				this.Hands = new Item(eq.Hands);
				this.Legs = new Item(eq.Legs);
				this.Feet = new Item(eq.Feet);
			}

			if (mode.HasFlag(SaveModes.EquipmentAccessories))
			{
				this.Ears = new Item(eq.Ears);
				this.Neck = new Item(eq.Neck);
				this.Wrists = new Item(eq.Wrists);
				this.LeftRing = new Item(eq.LeftRing);
				this.RightRing = new Item(eq.RightRing);
			}

			if (mode.HasFlag(SaveModes.AppearanceHair))
			{
				this.Hair = ed.Appearance.Hair;
				this.EnableHighlights = ed.Appearance.EnableHighlights;
				this.HairTone = ed.Appearance.HairTone;
				this.Highlights = ed.Appearance.Highlights;
				this.HairTint = ed.HairTint;
				this.HairGlow = ed.HairGlow;
				this.HighlightTint = ed.HighlightTint;
			}

			if (mode.HasFlag(SaveModes.AppearanceFace) || mode.HasFlag(SaveModes.AppearanceBody))
			{
				this.Race = ed.Appearance.Race;
				this.Gender = ed.Appearance.Gender;
				this.Tribe = ed.Appearance.Tribe;
				this.Age = ed.Appearance.Age;
			}

			if (mode.HasFlag(SaveModes.AppearanceFace))
			{
				this.Head = ed.Appearance.Head;
				this.REyeColor = ed.Appearance.REyeColor;
				this.LimbalEyes = ed.Appearance.LimbalEyes;
				this.FacialFeatures = ed.Appearance.FacialFeatures;
				this.Eyebrows = ed.Appearance.Eyebrows;
				this.LEyeColor = ed.Appearance.LEyeColor;
				this.Eyes = ed.Appearance.Eyes;
				this.Nose = ed.Appearance.Nose;
				this.Jaw = ed.Appearance.Jaw;
				this.Mouth = ed.Appearance.Mouth;
				this.LipsToneFurPattern = ed.Appearance.LipsToneFurPattern;
				this.FacePaint = ed.Appearance.FacePaint;
				this.FacePaintColor = ed.Appearance.FacePaintColor;
				this.LeftEyeColor = ed.LeftEyeColor;
				this.RightEyeColor = ed.RightEyeColor;
				this.LimbalRingColor = ed.LimbalRingColor;
				this.LipTint = ed.LipTint;
			}

			if (mode.HasFlag(SaveModes.AppearanceBody))
			{
				this.Height = ed.Appearance.Height;
				this.Skintone = ed.Appearance.Skintone;
				this.EarMuscleTailSize = ed.Appearance.EarMuscleTailSize;
				this.TailEarsType = ed.Appearance.TailEarsType;
				this.Bust = ed.Appearance.Bust;
				this.SkinTint = ed.SkinTint;
				this.SkinGlow = ed.SkinGlow;
			}
		}

		public void Write(AppearanceEditor ap, EquipmentEditor eq, SaveModes mode)
		{
			if (mode.HasFlag(SaveModes.EquipmentGear))
			{
				if (this.MainHand?.ModelSet != 0)
					this.MainHand?.Write(eq.MainHand);

				this.OffHand?.Write(eq.OffHand);
				this.HeadGear?.Write(eq.Head);
				this.Body?.Write(eq.Body);
				this.Hands?.Write(eq.Hands);
				this.Legs?.Write(eq.Legs);
				this.Feet?.Write(eq.Feet);

				Write(this.MainHandScale, (v) => eq.MainHand.Scale = v);
				Write(this.MainHandColor, (v) => eq.MainHand.Color = v);
				Write(this.OffHandScale, (v) => eq.OffHand.Scale = v);
				Write(this.OffHandColor, (v) => eq.OffHand.Color = v);
			}

			if (mode.HasFlag(SaveModes.EquipmentAccessories))
			{
				this.Ears?.Write(eq.Ears);
				this.Neck?.Write(eq.Neck);
				this.Wrists?.Write(eq.Wrists);
				this.LeftRing?.Write(eq.LeftRing);
				this.RightRing?.Write(eq.RightRing);
			}

			if (mode.HasFlag(SaveModes.AppearanceHair))
			{
				Write(this.Hair, (v) => ap.Appearance.Hair = v);
				Write(this.EnableHighlights, (v) => ap.Appearance.EnableHighlights = v);
				Write(this.HairTone, (v) => ap.Appearance.HairTone = v);
				Write(this.Highlights, (v) => ap.Appearance.Highlights = v);

				ap.HairTint = this.HairTint;
				ap.HairGlow = this.HairGlow;
				ap.HighlightTint = this.HighlightTint;
			}

			if (mode.HasFlag(SaveModes.AppearanceFace) || mode.HasFlag(SaveModes.AppearanceBody))
			{
				Write(this.Race, (v) => ap.Appearance.Race = v);
				Write(this.Gender, (v) => ap.Appearance.Gender = v);
				Write(this.Tribe, (v) => ap.Appearance.Tribe = v);
				Write(this.Age, (v) => ap.Appearance.Age = v);
			}

			if (mode.HasFlag(SaveModes.AppearanceFace))
			{
				Write(this.Head, (v) => ap.Appearance.Head = v);
				Write(this.REyeColor, (v) => ap.Appearance.REyeColor = v);
				Write(this.FacialFeatures, (v) => ap.Appearance.FacialFeatures = v);
				Write(this.LimbalEyes, (v) => ap.Appearance.LimbalEyes = v);
				Write(this.Eyebrows, (v) => ap.Appearance.Eyebrows = v);
				Write(this.LEyeColor, (v) => ap.Appearance.LEyeColor = v);
				Write(this.Eyes, (v) => ap.Appearance.Eyes = v);
				Write(this.Nose, (v) => ap.Appearance.Nose = v);
				Write(this.Jaw, (v) => ap.Appearance.Jaw = v);
				Write(this.Mouth, (v) => ap.Appearance.Mouth = v);
				Write(this.LipsToneFurPattern, (v) => ap.Appearance.LipsToneFurPattern = v);
				Write(this.FacePaint, (v) => ap.Appearance.FacePaint = v);
				Write(this.FacePaintColor, (v) => ap.Appearance.FacePaintColor = v);

				ap.LeftEyeColor = this.LeftEyeColor;
				ap.RightEyeColor = this.RightEyeColor;
				ap.LimbalRingColor = this.LimbalRingColor;
				ap.LipTint = this.LipTint;
			}

			if (mode.HasFlag(SaveModes.AppearanceBody))
			{
				Write(this.Height, (v) => ap.Appearance.Height = v);
				Write(this.Skintone, (v) => ap.Appearance.Skintone = v);
				Write(this.EarMuscleTailSize, (v) => ap.Appearance.EarMuscleTailSize = v);
				Write(this.TailEarsType, (v) => ap.Appearance.TailEarsType = v);
				Write(this.Bust, (v) => ap.Appearance.Bust = v);

				ap.SkinTint = this.SkinTint;
				ap.SkinGlow = this.SkinGlow;
			}
		}

		private static void Write<T>(T? val, Action<T> set)
			where T : struct
		{
			if (val == null)
			{
				Log.Write(new Exception("Missing appearance value where one was required"), "Appearance File", Log.Severity.Warning);
				return;
			}

			set.Invoke((T)val);
		}

		[Serializable]
		public class Weapon : Item
		{
			public Weapon()
			{
			}

			public Weapon(EquipmentBaseViewModel from)
				: base(from)
			{
				this.Color = from.Color;
				this.Scale = from.Scale;
				this.ModelSet = from.ModelSet;
			}

			public Color Color { get; set; }
			public Vector Scale { get; set; }
			public ushort ModelSet { get; set; }

			public override void Write(EquipmentBaseViewModel to)
			{
				to.Color = this.Color;
				to.Scale = this.Scale;
				to.ModelSet = this.ModelSet;

				base.Write(to);
			}
		}

		[Serializable]
		public class Item
		{
			public Item()
			{
			}

			public Item(EquipmentBaseViewModel from)
			{
				this.ModelBase = from.ModelBase;
				this.ModelVariant = from.ModelVariant;
				this.DyeId = from.DyeId;
			}

			public ushort ModelBase { get; set; }
			public ushort ModelVariant { get; set; }
			public byte DyeId { get; set; }

			public virtual void Write(EquipmentBaseViewModel to)
			{
				to.ModelBase = this.ModelBase;
				to.ModelVariant = this.ModelVariant;
				to.DyeId = this.DyeId;
			}
		}
	}
}
