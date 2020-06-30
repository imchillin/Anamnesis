// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using Anamnesis;
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

		public SaveModes SaveMode { get; set; } = SaveModes.All;

		public int ModelType { get; set; } = 0;

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

		public WeaponSave MainHand { get; set; }
		public WeaponSave OffHand { get; set; }

		public ItemSave HeadGear { get; set; }
		public ItemSave Body { get; set; }
		public ItemSave Hands { get; set; }
		public ItemSave Legs { get; set; }
		public ItemSave Feet { get; set; }
		public ItemSave Ears { get; set; }
		public ItemSave Neck { get; set; }
		public ItemSave Wrists { get; set; }
		public ItemSave LeftRing { get; set; }
		public ItemSave RightRing { get; set; }

		public override FileType GetFileType()
		{
			return FileType;
		}

		public void Read(Actor actor, SaveModes mode)
		{
			Log.Write("Writing appearance to file", "AppearanceFile");

			using IMemory<int> modelTypeMem = actor.GetMemory(Offsets.Main.ModelType);
			this.ModelType = modelTypeMem.Value;

			if (!actor.IsCustomizable())
				return;

			using IMemory<Appearance> appearanceMem = actor.GetMemory(Offsets.Main.ActorAppearance);
			using IMemory<Equipment> equipmentMem = actor.GetMemory(Offsets.Main.ActorEquipment);
			using IMemory<Weapon> mainHandMem = actor.GetMemory(Offsets.Main.MainHand);
			using IMemory<Weapon> offHandMem = actor.GetMemory(Offsets.Main.OffHand);

			Appearance appearance = appearanceMem.Value;
			Equipment equipment = equipmentMem.Value;
			Weapon mainHand = mainHandMem.Value;
			Weapon offHand = offHandMem.Value;

			this.SaveMode = mode;

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				using IMemory<Color> mainHandTintMem = actor.GetMemory(Offsets.Main.MainHandColor);
				using IMemory<Vector> mainHandScaleMem = actor.GetMemory(Offsets.Main.MainHandScale);
				using IMemory<Color> offHandTintMem = actor.GetMemory(Offsets.Main.OffhandColor);
				using IMemory<Vector> offHandScaleMem = actor.GetMemory(Offsets.Main.OffhandScale);

				this.MainHand = new WeaponSave();
				this.MainHand.DyeId = mainHand.Dye;
				this.MainHand.ModelBase = mainHand.Base;
				this.MainHand.ModelSet = mainHand.Set;
				this.MainHand.ModelVariant = mainHand.Variant;
				this.MainHand.Color = mainHandTintMem.Value;
				this.MainHand.Scale = mainHandScaleMem.Value;

				this.OffHand = new WeaponSave();
				this.OffHand.DyeId = offHand.Dye;
				this.OffHand.ModelBase = offHand.Base;
				this.OffHand.ModelSet = offHand.Set;
				this.OffHand.ModelVariant = offHand.Variant;
				this.OffHand.Color = offHandTintMem.Value;
				this.OffHand.Scale = offHandScaleMem.Value;

				this.HeadGear = new ItemSave(equipment.Head);
				this.Body = new ItemSave(equipment.Chest);
				this.Hands = new ItemSave(equipment.Arms);
				this.Legs = new ItemSave(equipment.Legs);
				this.Feet = new ItemSave(equipment.Feet);
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				this.Ears = new ItemSave(equipment.Ear);
				this.Neck = new ItemSave(equipment.Neck);
				this.Wrists = new ItemSave(equipment.Wrist);
				this.LeftRing = new ItemSave(equipment.LFinger);
				this.RightRing = new ItemSave(equipment.RFinger);
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				using IMemory<Color> hairTintColorMem = actor.GetMemory(Offsets.Main.HairColor);
				using IMemory<Color> hairGlowColorMem = actor.GetMemory(Offsets.Main.HairGloss);
				using IMemory<Color> highlightTintColorMem = actor.GetMemory(Offsets.Main.HairHiglight);

				this.Hair = appearance.Hair;
				this.EnableHighlights = appearance.EnableHighlights;
				this.HairTone = appearance.HairTone;
				this.Highlights = appearance.Highlights;
				this.HairTint = hairTintColorMem.Value;
				this.HairGlow = hairGlowColorMem.Value;
				this.HighlightTint = highlightTintColorMem.Value;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				this.Race = appearance.Race;
				this.Gender = appearance.Gender;
				this.Tribe = appearance.Tribe;
				this.Age = appearance.Age;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				using IMemory<Color> leftEyeColorMem = actor.GetMemory(Offsets.Main.LeftEyeColor);
				using IMemory<Color> rightEyeColorMem = actor.GetMemory(Offsets.Main.RightEyeColor);
				using IMemory<Color> limbalRingColorMem = actor.GetMemory(Offsets.Main.LimbalColor);
				using IMemory<Color> lipTintMem = actor.GetMemory(Offsets.Main.MouthColor);
				using IMemory<float> lipGlossMem = actor.GetMemory(Offsets.Main.MouthGloss);

				this.Head = appearance.Head;
				this.REyeColor = appearance.REyeColor;
				this.LimbalEyes = appearance.LimbalEyes;
				this.FacialFeatures = appearance.FacialFeatures;
				this.Eyebrows = appearance.Eyebrows;
				this.LEyeColor = appearance.LEyeColor;
				this.Eyes = appearance.Eyes;
				this.Nose = appearance.Nose;
				this.Jaw = appearance.Jaw;
				this.Mouth = appearance.Mouth;
				this.LipsToneFurPattern = appearance.LipsToneFurPattern;
				this.FacePaint = appearance.FacePaint;
				this.FacePaintColor = appearance.FacePaintColor;
				this.LeftEyeColor = leftEyeColorMem.Value;
				this.RightEyeColor = rightEyeColorMem.Value;
				this.LimbalRingColor = limbalRingColorMem.Value;

				this.LipTint = new Color4(lipTintMem.Value, lipGlossMem.Value);
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				using IMemory<Color> skinColorMem = actor.GetMemory(Offsets.Main.SkinColor);
				using IMemory<Color> skinGlowMem = actor.GetMemory(Offsets.Main.SkinGloss);

				this.Height = appearance.Height;
				this.Skintone = appearance.Skintone;
				this.EarMuscleTailSize = appearance.EarMuscleTailSize;
				this.TailEarsType = appearance.TailEarsType;
				this.Bust = appearance.Bust;
				this.SkinTint = skinColorMem.Value;
				this.SkinGlow = skinGlowMem.Value;
			}
		}

		public async Task Apply(Actor actor, SaveModes mode)
		{
			Log.Write("Reading appearance from file", "AppearanceFile");

			using IMemory<int> modelTypeMem = actor.GetMemory(Offsets.Main.ModelType);

			if (modelTypeMem.Value != this.ModelType)
			{
				modelTypeMem.Value = (int)this.ModelType;
				await actor.ActorRefreshAsync();
			}

			if (!actor.IsCustomizable())
				return;

			using IMemory<Appearance> appearanceMem = actor.GetMemory(Offsets.Main.ActorAppearance);
			using IMemory<Equipment> equipmentMem = actor.GetMemory(Offsets.Main.ActorEquipment);
			using IMemory<Weapon> mainHandMem = actor.GetMemory(Offsets.Main.MainHand);
			using IMemory<Weapon> offHandMem = actor.GetMemory(Offsets.Main.OffHand);

			Appearance appearance = appearanceMem.Value;
			Equipment equipment = equipmentMem.Value;
			Weapon mainHand = mainHandMem.Value;
			Weapon offHand = offHandMem.Value;

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				mainHand.Base = this.MainHand.ModelBase;
				mainHand.Dye = this.MainHand.DyeId;
				mainHand.Set = this.MainHand.ModelSet;
				mainHand.Variant = this.MainHand.ModelVariant;

				if (this.OffHand != null)
				{
					offHand.Base = this.OffHand.ModelBase;
					offHand.Dye = this.OffHand.DyeId;
					offHand.Set = this.OffHand.ModelSet;
					offHand.Variant = this.OffHand.ModelVariant;
				}

				equipment.Head = this.HeadGear;
				equipment.Chest = this.Body;
				equipment.Arms = this.Hands;
				equipment.Legs = this.Legs;
				equipment.Feet = this.Feet;
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				equipment.Ear = this.Ears;
				equipment.Neck = this.Neck;
				equipment.Wrist = this.Wrists;
				equipment.RFinger = this.RightRing;
				equipment.LFinger = this.LeftRing;
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				appearance.Hair = (byte)this.Hair;
				appearance.EnableHighlights = (bool)this.EnableHighlights;
				appearance.HairTone = (byte)this.HairTone;
				appearance.Highlights = (byte)this.Highlights;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				appearance.Race = (Appearance.Races)this.Race;
				appearance.Gender = (Appearance.Genders)this.Gender;
				appearance.Tribe = (Appearance.Tribes)this.Tribe;
				appearance.Age = (Appearance.Ages)this.Age;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				appearance.Head = (byte)this.Head;
				appearance.REyeColor = (byte)this.REyeColor;
				appearance.FacialFeatures = (Appearance.FacialFeature)this.FacialFeatures;
				appearance.LimbalEyes = (byte)this.LimbalEyes;
				appearance.Eyebrows = (byte)this.Eyebrows;
				appearance.LEyeColor = (byte)this.LEyeColor;
				appearance.Eyes = (byte)this.Eyes;
				appearance.Nose = (byte)this.Nose;
				appearance.Jaw = (byte)this.Jaw;
				appearance.Mouth = (byte)this.Mouth;
				appearance.LipsToneFurPattern = (byte)this.LipsToneFurPattern;
				appearance.FacePaint = (byte)this.FacePaint;
				appearance.FacePaintColor = (byte)this.FacePaintColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				appearance.Height = (byte)this.Height;
				appearance.Skintone = (byte)this.Skintone;
				appearance.EarMuscleTailSize = (byte)this.EarMuscleTailSize;
				appearance.TailEarsType = (byte)this.TailEarsType;
				appearance.Bust = (byte)this.Bust;
			}

			await Task.Delay(100);

			appearanceMem.Value = appearance;
			equipmentMem.Value = equipment;
			mainHandMem.Value = mainHand;
			offHandMem.Value = offHand;

			await actor.ActorRefreshAsync();
			await Task.Delay(1000);

			// write everything that is reset by actor refreshes
			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				using IMemory<Color> mainHandTintMem = actor.GetMemory(Offsets.Main.MainHandColor);
				using IMemory<Vector> mainHandScaleMem = actor.GetMemory(Offsets.Main.MainHandScale);

				mainHandScaleMem.Value = this.MainHand.Scale;
				mainHandTintMem.Value = this.MainHand.Color;

				if (this.OffHand != null)
				{
					using IMemory<Color> offHandTintMem = actor.GetMemory(Offsets.Main.OffhandColor);
					using IMemory<Vector> offHandScaleMem = actor.GetMemory(Offsets.Main.OffhandScale);

					offHandScaleMem.Value = this.OffHand.Scale;
					offHandTintMem.Value = this.OffHand.Color;
				}
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				if (this.HairTint != null)
				{
					using IMemory<Color> hairTintColorMem = actor.GetMemory(Offsets.Main.HairColor);
					hairTintColorMem.Value = (Color)this.HairTint;
				}

				if (this.HairGlow != null)
				{
					using IMemory<Color> hairGlowColorMem = actor.GetMemory(Offsets.Main.HairGloss);
					hairGlowColorMem.Value = (Color)this.HairGlow;
				}

				if (this.HighlightTint != null)
				{
					using IMemory<Color> highlightTintColorMem = actor.GetMemory(Offsets.Main.HairHiglight);
					highlightTintColorMem.Value = (Color)this.HighlightTint;
				}
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				if (this.LeftEyeColor != null)
				{
					using IMemory<Color> leftEyeColorMem = actor.GetMemory(Offsets.Main.LeftEyeColor);
					leftEyeColorMem.Value = (Color)this.LeftEyeColor;
				}

				if (this.RightEyeColor != null)
				{
					using IMemory<Color> rightEyeColorMem = actor.GetMemory(Offsets.Main.RightEyeColor);
					rightEyeColorMem.Value = (Color)this.RightEyeColor;
				}

				if (this.LimbalRingColor != null)
				{
					using IMemory<Color> limbalRingColorMem = actor.GetMemory(Offsets.Main.LimbalColor);
					limbalRingColorMem.Value = (Color)this.LimbalRingColor;
				}

				if (this.LipTint != null)
				{
					using IMemory<Color> lipTintMem = actor.GetMemory(Offsets.Main.MouthColor);
					using IMemory<float> lipGlossMem = actor.GetMemory(Offsets.Main.MouthGloss);

					Color4 c = (Color4)this.LipTint;
					lipTintMem.Value = c.Color;
					lipGlossMem.Value = c.A;
				}
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.SkinTint != null)
				{
					using IMemory<Color> skinTintMem = actor.GetMemory(Offsets.Main.SkinColor);
					skinTintMem.Value = (Color)this.SkinTint;
				}

				if (this.SkinGlow != null)
				{
					using IMemory<Color> skinGlowMem = actor.GetMemory(Offsets.Main.SkinGloss);
					skinGlowMem.Value = (Color)this.SkinGlow;
				}
			}
		}

		private bool IncludeSection(SaveModes section, SaveModes mode)
		{
			return this.SaveMode.HasFlag(section) && mode.HasFlag(section);
		}

		[Serializable]
		public class WeaponSave
		{
			public Color Color { get; set; }
			public Vector Scale { get; set; }
			public ushort ModelSet { get; set; }
			public ushort ModelBase { get; set; }
			public ushort ModelVariant { get; set; }
			public byte DyeId { get; set; }
		}

		[Serializable]
		public class ItemSave
		{
			public ItemSave()
			{
			}

			public ItemSave(Equipment.Item from)
			{
				this.ModelBase = from.Base;
				this.ModelVariant = from.Variant;
				this.DyeId = from.Dye;
			}

			public ushort ModelBase { get; set; }
			public byte ModelVariant { get; set; }
			public byte DyeId { get; set; }

			public static implicit operator Equipment.Item(ItemSave item)
			{
				Equipment.Item eqItem = new Equipment.Item();

				if (item == null)
					return eqItem;

				eqItem.Base = item.ModelBase;
				eqItem.Variant = item.ModelVariant;
				eqItem.Dye = item.DyeId;

				return eqItem;
			}
		}
	}
}
