// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Memory;
using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using XivToolsWpf.Math3D.Extensions;

[Serializable]
public class CharacterFile : JsonFileBase
{
	[Flags]
	public enum SaveModes
	{
		None = 0,

		EquipmentGear = 1,
		EquipmentAccessories = 2,
		EquipmentWeapons = 4,
		AppearanceHair = 8,
		AppearanceFace = 16,
		AppearanceBody = 32,
		AppearanceExtended = 64,
		EquipmentSlot = 128,

		Equipment = EquipmentGear | EquipmentAccessories,
		Appearance = AppearanceHair | AppearanceFace | AppearanceBody | AppearanceExtended,

		All = EquipmentGear | EquipmentAccessories | EquipmentWeapons | AppearanceHair | AppearanceFace | AppearanceBody | AppearanceExtended,
	}

	public override string FileExtension => ".chara";
	public override string TypeName => "Anamnesis Character File";

	public SaveModes SaveMode { get; set; } = SaveModes.All;

	public string? Nickname { get; set; } = null;
	public uint ModelType { get; set; } = 0;
	public ActorTypes ObjectKind { get; set; } = ActorTypes.None;

	// appearance
	public ActorCustomizeMemory.Races? Race { get; set; }
	public ActorCustomizeMemory.Genders? Gender { get; set; }
	public ActorCustomizeMemory.Ages? Age { get; set; }
	public byte? Height { get; set; }
	public ActorCustomizeMemory.Tribes? Tribe { get; set; }
	public byte? Head { get; set; }
	public byte? Hair { get; set; }
	public bool? EnableHighlights { get; set; }
	public byte? Skintone { get; set; }
	public byte? REyeColor { get; set; }
	public byte? HairTone { get; set; }
	public byte? Highlights { get; set; }
	public ActorCustomizeMemory.FacialFeature? FacialFeatures { get; set; }
	public byte? LimbalEyes { get; set; } // facial feature color
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

	// weapons
	public WeaponSave? MainHand { get; set; }
	public WeaponSave? OffHand { get; set; }

	// equipment
	public ItemSave? HeadGear { get; set; }
	public ItemSave? Body { get; set; }
	public ItemSave? Hands { get; set; }
	public ItemSave? Legs { get; set; }
	public ItemSave? Feet { get; set; }
	public ItemSave? Ears { get; set; }
	public ItemSave? Neck { get; set; }
	public ItemSave? Wrists { get; set; }
	public ItemSave? LeftRing { get; set; }
	public ItemSave? RightRing { get; set; }

	// glasses
	public GlassesSave? Glasses { get; set; }

	// extended appearance
	// NOTE: extended weapon values are stored in the WeaponSave
	public Color? SkinColor { get; set; }
	public Color? SkinGloss { get; set; }
	public Color? LeftEyeColor { get; set; }
	public Color? RightEyeColor { get; set; }
	public Color? LimbalRingColor { get; set; }
	public Color? HairColor { get; set; }
	public Color? HairGloss { get; set; }
	public Color? HairHighlight { get; set; }
	public Color4? MouthColor { get; set; }
	public Vector3? BustScale { get; set; }
	public float? Transparency { get; set; }
	public float? MuscleTone { get; set; }
	public float? HeightMultiplier { get; set; }

	public void WriteToFile(ActorMemory actor, SaveModes mode)
	{
		this.Nickname = actor.Nickname;
		this.ModelType = (uint)actor.ModelType;
		this.ObjectKind = actor.ObjectKind;

		if (actor.Customize == null)
			return;

		this.SaveMode = mode;

		if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
		{
			if (actor.MainHand != null)
				this.MainHand = new WeaponSave(actor.MainHand);
			////this.MainHand.Color = actor.GetValue(Offsets.Main.MainHandColor);
			////this.MainHand.Scale = actor.GetValue(Offsets.Main.MainHandScale);

			if (actor.OffHand != null)
				this.OffHand = new WeaponSave(actor.OffHand);
			////this.OffHand.Color = actor.GetValue(Offsets.Main.OffhandColor);
			////this.OffHand.Scale = actor.GetValue(Offsets.Main.OffhandScale);
		}

		if (this.IncludeSection(SaveModes.EquipmentGear, mode))
		{
			if (actor.Equipment?.Head != null)
				this.HeadGear = new ItemSave(actor.Equipment.Head);

			if (actor.Equipment?.Chest != null)
				this.Body = new ItemSave(actor.Equipment.Chest);

			if (actor.Equipment?.Arms != null)
				this.Hands = new ItemSave(actor.Equipment.Arms);

			if (actor.Equipment?.Legs != null)
				this.Legs = new ItemSave(actor.Equipment.Legs);

			if (actor.Equipment?.Feet != null)
				this.Feet = new ItemSave(actor.Equipment.Feet);

			// glasses - technically on the left side
			if (actor.Glasses != null)
				this.Glasses = new GlassesSave(actor.Glasses);
		}

		if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
		{
			if (actor.Equipment?.Ear != null)
				this.Ears = new ItemSave(actor.Equipment.Ear);

			if (actor.Equipment?.Neck != null)
				this.Neck = new ItemSave(actor.Equipment.Neck);

			if (actor.Equipment?.Wrist != null)
				this.Wrists = new ItemSave(actor.Equipment.Wrist);

			if (actor.Equipment?.LFinger != null)
				this.LeftRing = new ItemSave(actor.Equipment.LFinger);

			if (actor.Equipment?.RFinger != null)
				this.RightRing = new ItemSave(actor.Equipment.RFinger);
		}

		if (this.IncludeSection(SaveModes.AppearanceHair, mode))
		{
			this.Hair = actor.Customize?.Hair;
			this.EnableHighlights = actor.Customize?.EnableHighlights;
			this.HairTone = actor.Customize?.HairTone;
			this.Highlights = actor.Customize?.Highlights;
			this.HairColor = actor.ModelObject?.ExtendedAppearance?.HairColor;
			this.HairGloss = actor.ModelObject?.ExtendedAppearance?.HairGloss;
			this.HairHighlight = actor.ModelObject?.ExtendedAppearance?.HairHighlight;
		}

		if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
		{
			this.Race = actor.Customize?.Race;
			this.Gender = actor.Customize?.Gender;
			this.Tribe = actor.Customize?.Tribe;
			this.Age = actor.Customize?.Age;
		}

		if (this.IncludeSection(SaveModes.AppearanceFace, mode))
		{
			this.Head = actor.Customize?.Head;
			this.REyeColor = actor.Customize?.REyeColor;
			this.LimbalEyes = actor.Customize?.FacialFeatureColor;
			this.FacialFeatures = actor.Customize?.FacialFeatures;
			this.Eyebrows = actor.Customize?.Eyebrows;
			this.LEyeColor = actor.Customize?.LEyeColor;
			this.Eyes = actor.Customize?.Eyes;
			this.Nose = actor.Customize?.Nose;
			this.Jaw = actor.Customize?.Jaw;
			this.Mouth = actor.Customize?.Mouth;
			this.LipsToneFurPattern = actor.Customize?.LipsToneFurPattern;
			this.FacePaint = actor.Customize?.FacePaint;
			this.FacePaintColor = actor.Customize?.FacePaintColor;
			this.LeftEyeColor = actor.ModelObject?.ExtendedAppearance?.LeftEyeColor;
			this.RightEyeColor = actor.ModelObject?.ExtendedAppearance?.RightEyeColor;
			this.LimbalRingColor = actor.ModelObject?.ExtendedAppearance?.LimbalRingColor;
			this.MouthColor = actor.ModelObject?.ExtendedAppearance?.MouthColor;
		}

		if (this.IncludeSection(SaveModes.AppearanceBody, mode))
		{
			this.Height = actor.Customize?.Height;
			this.Skintone = actor.Customize?.Skintone;
			this.EarMuscleTailSize = actor.Customize?.EarMuscleTailSize;
			this.TailEarsType = actor.Customize?.TailEarsType;
			this.Bust = actor.Customize?.Bust;

			this.HeightMultiplier = actor.ModelObject?.Height;
			this.SkinColor = actor.ModelObject?.ExtendedAppearance?.SkinColor;
			this.SkinGloss = actor.ModelObject?.ExtendedAppearance?.SkinGloss;
			this.MuscleTone = actor.ModelObject?.ExtendedAppearance?.MuscleTone;
			this.BustScale = actor.ModelObject?.Bust?.Scale;
			this.Transparency = actor.Transparency;
		}
	}

	public async Task Apply(ActorMemory actor, SaveModes mode, ItemSlots? slot = null)
	{
		if (this.Tribe == 0)
			this.Tribe = ActorCustomizeMemory.Tribes.Midlander;

		if (this.Race == 0)
			this.Race = ActorCustomizeMemory.Races.Hyur;

		if (this.Tribe != null && !Enum.IsDefined((ActorCustomizeMemory.Tribes)this.Tribe))
			throw new Exception($"Invalid tribe: {this.Tribe} in appearance file");

		if (this.Race != null && !Enum.IsDefined((ActorCustomizeMemory.Races)this.Race))
			throw new Exception($"Invalid race: {this.Race} in appearance file");

		if (actor.Customize == null)
			return;

		if (this.Glasses == null)
			this.Glasses = new GlassesSave();

		Log.Information("Reading appearance from file");

		actor.AutomaticRefreshEnabled = false;

		if (actor.CanRefresh)
		{
			actor.EnableReading = false;

			if (!string.IsNullOrEmpty(this.Nickname))
				actor.Nickname = this.Nickname;

			actor.ModelType = (int)this.ModelType;
			////actor.ObjectKind = this.ObjectKind;

			if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
			{
				this.MainHand?.Write(actor.MainHand, true);
				this.OffHand?.Write(actor.OffHand, false);
				actor.IsWeaponDirty = true;
			}

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				this.HeadGear?.Write(actor.Equipment?.Head);
				this.Body?.Write(actor.Equipment?.Chest);
				this.Hands?.Write(actor.Equipment?.Arms);
				this.Legs?.Write(actor.Equipment?.Legs);
				this.Feet?.Write(actor.Equipment?.Feet);
				this.Glasses?.Write(actor.Glasses);
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				this.Ears?.Write(actor.Equipment?.Ear);
				this.Neck?.Write(actor.Equipment?.Neck);
				this.Wrists?.Write(actor.Equipment?.Wrist);
				this.RightRing?.Write(actor.Equipment?.RFinger);
				this.LeftRing?.Write(actor.Equipment?.LFinger);
			}

			if (mode == SaveModes.EquipmentSlot && slot != null)
			{
				switch (slot)
				{
					case ItemSlots.MainHand:
						this.MainHand?.Write(actor.MainHand, true);
						actor.IsWeaponDirty = true;
						break;
					case ItemSlots.OffHand:
						this.OffHand?.Write(actor.OffHand, false);
						actor.IsWeaponDirty = true;
						break;
					case ItemSlots.Head:
						this.HeadGear?.Write(actor.Equipment?.Head);
						break;
					case ItemSlots.Body:
						this.Body?.Write(actor.Equipment?.Chest);
						break;
					case ItemSlots.Hands:
						this.Hands?.Write(actor.Equipment?.Arms);
						break;
					case ItemSlots.Legs:
						this.Legs?.Write(actor.Equipment?.Legs);
						break;
					case ItemSlots.Feet:
						this.Feet?.Write(actor.Equipment?.Feet);
						break;
					case ItemSlots.Ears:
						this.Ears?.Write(actor.Equipment?.Ear);
						break;
					case ItemSlots.Neck:
						this.Neck?.Write(actor.Equipment?.Neck);
						break;
					case ItemSlots.Wrists:
						this.Wrists?.Write(actor.Equipment?.Wrist);
						break;
					case ItemSlots.LeftRing:
						this.LeftRing?.Write(actor.Equipment?.LFinger);
						break;
					case ItemSlots.RightRing:
						this.RightRing?.Write(actor.Equipment?.RFinger);
						break;
					case ItemSlots.Glasses:
						this.Glasses?.Write(actor.Glasses);
						break;
				}
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				if (this.Hair != null)
					actor.Customize.Hair = (byte)this.Hair;

				if (this.EnableHighlights != null)
					actor.Customize.EnableHighlights = (bool)this.EnableHighlights;

				if (this.HairTone != null)
					actor.Customize.HairTone = (byte)this.HairTone;

				if (this.Highlights != null)
					actor.Customize.Highlights = (byte)this.Highlights;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Race != null)
					actor.Customize.Race = (ActorCustomizeMemory.Races)this.Race;

				if (this.Gender != null)
					actor.Customize.Gender = (ActorCustomizeMemory.Genders)this.Gender;

				if (this.Tribe != null)
					actor.Customize.Tribe = (ActorCustomizeMemory.Tribes)this.Tribe;

				if (this.Age != null)
					actor.Customize.Age = (ActorCustomizeMemory.Ages)this.Age;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				if (this.Head != null)
					actor.Customize.Head = (byte)this.Head;

				if (this.REyeColor != null)
					actor.Customize.REyeColor = (byte)this.REyeColor;

				if (this.FacialFeatures != null)
					actor.Customize.FacialFeatures = (ActorCustomizeMemory.FacialFeature)this.FacialFeatures;

				if (this.LimbalEyes != null)
					actor.Customize.FacialFeatureColor = (byte)this.LimbalEyes;

				if (this.Eyebrows != null)
					actor.Customize.Eyebrows = (byte)this.Eyebrows;

				if (this.LEyeColor != null)
					actor.Customize.LEyeColor = (byte)this.LEyeColor;

				if (this.Eyes != null)
					actor.Customize.Eyes = (byte)this.Eyes;

				if (this.Nose != null)
					actor.Customize.Nose = (byte)this.Nose;

				if (this.Jaw != null)
					actor.Customize.Jaw = (byte)this.Jaw;

				if (this.Mouth != null)
					actor.Customize.Mouth = (byte)this.Mouth;

				if (this.LipsToneFurPattern != null)
					actor.Customize.LipsToneFurPattern = (byte)this.LipsToneFurPattern;

				if (this.FacePaint != null)
					actor.Customize.FacePaint = (byte)this.FacePaint;

				if (this.FacePaintColor != null)
					actor.Customize.FacePaintColor = (byte)this.FacePaintColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Height != null)
					actor.Customize.Height = (byte)this.Height;

				if (this.Skintone != null)
					actor.Customize.Skintone = (byte)this.Skintone;

				if (this.EarMuscleTailSize != null)
					actor.Customize.EarMuscleTailSize = (byte)this.EarMuscleTailSize;

				if (this.TailEarsType != null)
					actor.Customize.TailEarsType = (byte)this.TailEarsType;

				if (this.Bust != null)
					actor.Customize.Bust = (byte)this.Bust;
			}

			await actor.RefreshAsync();

			// Setting customize values will reset the extended appearance, which me must read.
			actor.EnableReading = true;
			actor.Synchronize();
			actor.EnableReading = false;
		}

		Log.Verbose("Begin reading Extended Appearance from file");

		if (actor.ModelObject?.ExtendedAppearance != null)
		{
			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				actor.ModelObject.ExtendedAppearance.HairColor = this.HairColor ?? actor.ModelObject.ExtendedAppearance.HairColor;
				actor.ModelObject.ExtendedAppearance.HairGloss = this.HairGloss ?? actor.ModelObject.ExtendedAppearance.HairGloss;
				actor.ModelObject.ExtendedAppearance.HairHighlight = this.HairHighlight ?? actor.ModelObject.ExtendedAppearance.HairHighlight;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				actor.ModelObject.ExtendedAppearance.LeftEyeColor = this.LeftEyeColor ?? actor.ModelObject.ExtendedAppearance.LeftEyeColor;
				actor.ModelObject.ExtendedAppearance.RightEyeColor = this.RightEyeColor ?? actor.ModelObject.ExtendedAppearance.RightEyeColor;
				actor.ModelObject.ExtendedAppearance.LimbalRingColor = this.LimbalRingColor ?? actor.ModelObject.ExtendedAppearance.LimbalRingColor;
				actor.ModelObject.ExtendedAppearance.MouthColor = this.MouthColor ?? actor.ModelObject.ExtendedAppearance.MouthColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				actor.ModelObject.ExtendedAppearance.SkinColor = this.SkinColor ?? actor.ModelObject.ExtendedAppearance.SkinColor;
				actor.ModelObject.ExtendedAppearance.SkinGloss = this.SkinGloss ?? actor.ModelObject.ExtendedAppearance.SkinGloss;
				actor.ModelObject.ExtendedAppearance.MuscleTone = this.MuscleTone ?? actor.ModelObject.ExtendedAppearance.MuscleTone;
				actor.Transparency = this.Transparency ?? actor.Transparency;

				if (Float.IsValid(this.HeightMultiplier))
					actor.ModelObject.Height = this.HeightMultiplier ?? actor.ModelObject.Height;

				if (actor.ModelObject.Bust?.Scale != null && this.BustScale.IsValid())
				{
					actor.ModelObject.Bust.Scale = this.BustScale ?? actor.ModelObject.Bust.Scale;
				}
			}
		}

		actor.AutomaticRefreshEnabled = true;
		actor.EnableReading = true;

		Log.Information("Finished reading appearance from file");
	}

	private bool IncludeSection(SaveModes section, SaveModes mode)
	{
		return this.SaveMode.HasFlag(section) && mode.HasFlag(section);
	}

	[Serializable]
	public class WeaponSave
	{
		public WeaponSave()
		{
		}

		public WeaponSave(WeaponMemory from)
		{
			this.ModelSet = from.Set;
			this.ModelBase = from.Base;
			this.ModelVariant = from.Variant;
			this.DyeId = from.Dye;
			this.DyeId2 = from.Dye2;
		}

		public Color Color { get; set; }
		public Vector3 Scale { get; set; }
		public ushort ModelSet { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public byte DyeId { get; set; }
		public byte DyeId2 { get; set; }

		public void Write(WeaponMemory? vm, bool isMainHand)
		{
			if (vm == null)
				return;

			vm.Set = this.ModelSet;

			// sanity check values
			if (vm.Set != 0)
			{
				vm.Base = this.ModelBase;
				vm.Variant = this.ModelVariant;
				vm.Dye = this.DyeId;
				vm.Dye2 = this.DyeId2;
			}
			else
			{
				if (isMainHand)
				{
					vm.Set = ItemUtility.EmperorsNewFists.ModelSet;
					vm.Base = ItemUtility.EmperorsNewFists.ModelBase;
					vm.Variant = ItemUtility.EmperorsNewFists.ModelVariant;
				}
				else
				{
					vm.Set = 0;
					vm.Base = 0;
					vm.Variant = 0;
				}

				vm.Dye = ItemUtility.NoneDye.Id;
				vm.Dye2 = ItemUtility.NoneDye.Id;
			}
		}
	}

	[Serializable]
	public class ItemSave
	{
		public ItemSave()
		{
		}

		public ItemSave(ItemMemory from)
		{
			this.ModelBase = from.Base;
			this.ModelVariant = from.Variant;
			this.DyeId = from.Dye;
			this.DyeId2 = from.Dye2;
		}

		public ushort ModelBase { get; set; }
		public byte ModelVariant { get; set; }
		public byte DyeId { get; set; }
		public byte DyeId2 { get; set; }

		public void Write(ItemMemory? vm)
		{
			if (vm == null)
				return;

			vm.Base = this.ModelBase;
			vm.Variant = this.ModelVariant;
			vm.Dye = this.DyeId;
			vm.Dye2 = this.DyeId2;
		}
	}

	[Serializable]
	public class GlassesSave
	{
		public GlassesSave()
		{
			this.GlassesId = 0;
		}

		public GlassesSave(GlassesMemory from)
		{
			this.GlassesId = from.GlassesId;
		}

		public GlassesSave(ushort glassesId)
		{
			this.GlassesId = glassesId;
		}

		public ushort GlassesId { get; set; }

		public void Write(GlassesMemory? glasses)
		{
			if (glasses == null)
				return;
			glasses.GlassesId = this.GlassesId;
		}
	}
}
