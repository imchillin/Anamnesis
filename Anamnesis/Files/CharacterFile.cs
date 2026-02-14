// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
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
	public ObjectTypes ObjectKind { get; set; } = ObjectTypes.None;

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

	public void WriteToFile(ObjectHandle<ActorMemory> handle, SaveModes mode)
	{
		handle.Do(actor => this.WriteToFile(actor, mode));
	}

	public void Apply(ObjectHandle<ActorMemory> handle, SaveModes mode, ItemSlots? slot = null)
	{
		handle.Do(async actor => await this.Apply(actor, mode, slot));
	}

	/// <summary>
	/// Computes the changes between this file and another.
	/// </summary>
	/// <param name="target">The target state to compare against.</param>
	/// <param name="mode">Which sections to compare.</param>
	public CharChangeSet CompareTo(CharacterFile target, SaveModes mode = SaveModes.All)
	{
		return CharChangeSet.Create(current: this, target: target, mode: mode);
	}

	private static void WriteEquipment(ItemSave? itemSave, ItemMemory? itemMemory)
	{
		if (itemSave != null)
		{
			itemSave.Write(itemMemory);
		}
		else if (itemMemory != null)
		{
			itemMemory.Base = ItemUtility.NoneItem.ModelBase;
			itemMemory.Variant = (byte)ItemUtility.NoneItem.ModelVariant;
			itemMemory.Dye = (byte)ItemUtility.NoneDye.RowId;
			itemMemory.Dye2 = (byte)ItemUtility.NoneDye.RowId;
		}
	}

	private bool IncludeSection(SaveModes section, SaveModes mode)
	{
		return this.SaveMode.HasFlagUnsafe(section) && mode.HasFlagUnsafe(section);
	}

	private void WriteToFile(ActorMemory actor, SaveModes mode)
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

	private async Task Apply(ActorMemory actor, SaveModes mode, ItemSlots? slot = null)
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

		this.Glasses ??= new GlassesSave();
		if (!actor.CanRefresh)
		{
			Log.Warning($"Actor '{actor.Name}' cannot be refreshed. Appearance will not be applied.");
			return;
		}

		Log.Information("Reading appearance from file");
		var existingModelType = actor.ModelType;
		var existingRace = actor.Customize.Race;
		var existingGender = actor.Customize.Gender;
		var existingTribe = actor.Customize.Tribe;
		var existingHead = actor.Customize.Head;

		bool needsRedraw = false;
		bool isStructuralChange = false;
		bool prevAutoRefresh = actor.AutomaticRefreshEnabled;

		try
		{
			// Disable automatic per-property refresh so we can batch changes
			// into a single controlled redraw at the end.
			actor.AutomaticRefreshEnabled = false;
			actor.PauseSynchronization = true;

			if (!string.IsNullOrEmpty(this.Nickname))
				actor.Nickname = this.Nickname;

			// Model type change always requires a full redraw
			actor.ModelType = (int)this.ModelType;
			if (actor.ModelType != existingModelType)
			{
				needsRedraw = true;
				isStructuralChange = true;
			}

			if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
			{
				this.MainHand?.Write(actor.MainHand, true);
				this.OffHand?.Write(actor.OffHand, false);
				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				WriteEquipment(this.HeadGear, actor.Equipment?.Head);
				WriteEquipment(this.Body, actor.Equipment?.Chest);
				WriteEquipment(this.Hands, actor.Equipment?.Arms);
				WriteEquipment(this.Legs, actor.Equipment?.Legs);
				WriteEquipment(this.Feet, actor.Equipment?.Feet);

				if (this.Glasses != null)
					this.Glasses.Write(actor.Glasses);
				else if (actor.Glasses != null)
					actor.Glasses.GlassesId = 0;

				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				WriteEquipment(this.Ears, actor.Equipment?.Ear);
				WriteEquipment(this.Neck, actor.Equipment?.Neck);
				WriteEquipment(this.Wrists, actor.Equipment?.Wrist);
				WriteEquipment(this.LeftRing, actor.Equipment?.LFinger);
				WriteEquipment(this.RightRing, actor.Equipment?.RFinger);

				needsRedraw = true;
			}

			if (mode == SaveModes.EquipmentSlot && slot != null)
			{
				switch (slot)
				{
					case ItemSlots.MainHand:
						this.MainHand?.Write(actor.MainHand, true);
						break;
					case ItemSlots.OffHand:
						this.OffHand?.Write(actor.OffHand, false);
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

				needsRedraw = true;
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

				needsRedraw = true;
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

				needsRedraw = true;
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

				needsRedraw = true;
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

				needsRedraw = true;
			}

			// Determine if the actor needs to be redrawn based on structural changes
			isStructuralChange |=
				actor.Customize.Race != existingRace ||
				actor.Customize.Gender != existingGender ||
				actor.Customize.Tribe != existingTribe ||
				actor.Customize.Head != existingHead;

			// Resume synchronization and read back from game memory.
			// This is required so that the ConstantBufferMemory (extended appearance)
			// pointer is resolved before we attempt to write extended appearance values.
			actor.PauseSynchronization = false;
			try
			{
				actor.Synchronize(exclGroups: TargetService.ExcludeSkeletonGroup);
			}
			catch (Exception)
			{
				Log.Warning("Failed to synchronize actor after applying appearance values. Extended appearance may not be applied correctly.");
			}

			if (needsRedraw)
			{
				if (isStructuralChange || !actor.IsHuman)
				{
					Log.Information($"Performing full redraw (Structural change: {isStructuralChange}; IsHuman: {actor.IsHuman})");
					await actor.Refresh();
				}
				else
				{
					var drawData = actor.BuildDrawData();
					bool updated = actor.UpdateDrawData(in drawData, skipEquipment: false);

					if (!updated)
					{
						Log.Information("Optimized UpdateDrawData failed, falling back to full redraw");
						await actor.Refresh();
					}
					else
					{
						Log.Information("Applied changes via optimized UpdateDrawData");
					}
				}

				// Re-synchronize after redraw to pick up the new draw object state
				try
				{
					actor.Synchronize(exclGroups: TargetService.ExcludeSkeletonGroup);
				}
				catch (Exception)
				{
					Log.Warning("Failed to synchronize actor after redraw.");
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to apply character file");
		}
		finally
		{
			actor.AutomaticRefreshEnabled = prevAutoRefresh;
			actor.PauseSynchronization = false;
		}

		Log.Verbose("Begin applying extended appearance from file");

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

		Log.Information("Finished applying appearance from file");
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

		public GlassesSave(uint glassesId)
		{
			if (glassesId > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(glassesId), $"Glasses ID exceeds the maximum support value of {ushort.MaxValue}.");

			this.GlassesId = (ushort)glassesId;
		}

		public ushort GlassesId { get; set; }

		public void Write(GlassesMemory? glasses)
		{
			if (glasses == null)
				return;
			glasses.GlassesId = this.GlassesId;
		}
	}

	/// <summary>
	/// Represents the set of changes between two <see cref="CharacterFile"/> snapshots.
	/// Used to determine the optimal redraw strategy.
	/// </summary>
	public sealed class CharChangeSet
	{
		private CharChangeSet()
		{
		}

		/// <summary>Creates an empty changeset (no changes detected).</summary>
		public static CharChangeSet None => new();

		/// <summary>Creates a changeset indicating a full redraw is required.</summary>
		public static CharChangeSet FullRedraw => new() { HasStructuralChanges = true };

		/// <summary>
		/// Gets whether structural changes occurred that require a full redraw
		/// (race, gender, tribe, head, or model type changes).
		/// </summary>
		public bool HasStructuralChanges { get; private init; }

		/// <summary>Gets whether equipment models changed.</summary>
		public bool HasEquipmentChanges { get; private init; }

		/// <summary>Gets whether weapon models changed.</summary>
		public bool HasWeaponChanges { get; private init; }

		/// <summary>Gets whether appearance/customize data changed.</summary>
		public bool HasAppearanceChanges { get; private init; }

		/// <summary>Gets whether extended appearance data changed.</summary>
		public bool HasExtendedAppearanceChanges { get; private init; }

		/// <summary>Gets whether any changes were detected.</summary>
		public bool HasChanges =>
			this.HasStructuralChanges ||
			this.HasEquipmentChanges ||
			this.HasWeaponChanges ||
			this.HasAppearanceChanges ||
			this.HasExtendedAppearanceChanges;

		/// <summary>Gets whether an optimized redraw (UpdateDrawData) can be used.</summary>
		public bool CanUseOptimizedRedraw => !this.HasStructuralChanges && !this.HasWeaponChanges;

		/// <summary>
		/// Computes the differences between two character files.
		/// </summary>
		/// <param name="current">The current state (from memory).</param>
		/// <param name="target">The target state (to apply).</param>
		/// <param name="mode">Which sections are being applied.</param>
		public static CharChangeSet Create(
			CharacterFile current,
			CharacterFile target,
			CharacterFile.SaveModes mode = CharacterFile.SaveModes.All)
		{
			ArgumentNullException.ThrowIfNull(current);
			ArgumentNullException.ThrowIfNull(target);

			bool hasStructural = false;
			bool hasAppearance = false;
			bool hasEquipment = false;
			bool hasWeapon = false;
			bool hasExtended = false;

			// Structural changes (always checked - these require full redraw)
			hasStructural =
				current.ModelType != target.ModelType ||
				current.Race != target.Race ||
				current.Gender != target.Gender ||
				current.Tribe != target.Tribe ||
				current.Head != target.Head;

			// Appearance changes
			if (mode.HasFlag(CharacterFile.SaveModes.AppearanceHair))
			{
				hasAppearance |=
					current.Hair != target.Hair ||
					current.EnableHighlights != target.EnableHighlights ||
					current.HairTone != target.HairTone ||
					current.Highlights != target.Highlights;

				hasExtended |=
					current.HairColor != target.HairColor ||
					current.HairGloss != target.HairGloss ||
					current.HairHighlight != target.HairHighlight;
			}

			if (mode.HasFlag(CharacterFile.SaveModes.AppearanceFace))
			{
				hasAppearance |=
					current.REyeColor != target.REyeColor ||
					current.LEyeColor != target.LEyeColor ||
					current.FacialFeatures != target.FacialFeatures ||
					current.LimbalEyes != target.LimbalEyes ||
					current.Eyebrows != target.Eyebrows ||
					current.Eyes != target.Eyes ||
					current.Nose != target.Nose ||
					current.Jaw != target.Jaw ||
					current.Mouth != target.Mouth ||
					current.LipsToneFurPattern != target.LipsToneFurPattern ||
					current.FacePaint != target.FacePaint ||
					current.FacePaintColor != target.FacePaintColor;

				hasExtended |=
					current.LeftEyeColor != target.LeftEyeColor ||
					current.RightEyeColor != target.RightEyeColor ||
					current.LimbalRingColor != target.LimbalRingColor ||
					current.MouthColor != target.MouthColor;
			}

			if (mode.HasFlag(CharacterFile.SaveModes.AppearanceBody))
			{
				hasAppearance |=
					current.Height != target.Height ||
					current.Skintone != target.Skintone ||
					current.EarMuscleTailSize != target.EarMuscleTailSize ||
					current.TailEarsType != target.TailEarsType ||
					current.Bust != target.Bust;

				hasExtended |=
					current.SkinColor != target.SkinColor ||
					current.SkinGloss != target.SkinGloss ||
					current.MuscleTone != target.MuscleTone ||
					current.HeightMultiplier != target.HeightMultiplier ||
					current.BustScale != target.BustScale ||
					current.Transparency != target.Transparency;
			}

			// Equipment changes
			if (mode.HasFlag(CharacterFile.SaveModes.EquipmentGear))
			{
				hasEquipment |=
					!ItemEquals(current.HeadGear, target.HeadGear) ||
					!ItemEquals(current.Body, target.Body) ||
					!ItemEquals(current.Hands, target.Hands) ||
					!ItemEquals(current.Legs, target.Legs) ||
					!ItemEquals(current.Feet, target.Feet) ||
					!GlassesEquals(current.Glasses, target.Glasses);
			}

			if (mode.HasFlag(CharacterFile.SaveModes.EquipmentAccessories))
			{
				hasEquipment |=
					!ItemEquals(current.Ears, target.Ears) ||
					!ItemEquals(current.Neck, target.Neck) ||
					!ItemEquals(current.Wrists, target.Wrists) ||
					!ItemEquals(current.LeftRing, target.LeftRing) ||
					!ItemEquals(current.RightRing, target.RightRing);
			}

			// Weapon changes
			if (mode.HasFlag(CharacterFile.SaveModes.EquipmentWeapons))
			{
				hasWeapon |=
					!WeaponEquals(current.MainHand, target.MainHand) ||
					!WeaponEquals(current.OffHand, target.OffHand);
			}

			return new CharChangeSet
			{
				HasStructuralChanges = hasStructural,
				HasAppearanceChanges = hasAppearance,
				HasEquipmentChanges = hasEquipment,
				HasWeaponChanges = hasWeapon,
				HasExtendedAppearanceChanges = hasExtended,
			};
		}

		public override string ToString()
		{
			if (!this.HasChanges)
				return "No changes";

			return $"Structural={this.HasStructuralChanges}, Equipment={this.HasEquipmentChanges}, " +
				   $"Weapons={this.HasWeaponChanges}, Appearance={this.HasAppearanceChanges}, " +
				   $"Extended={this.HasExtendedAppearanceChanges}";
		}

		private static bool ItemEquals(CharacterFile.ItemSave? a, CharacterFile.ItemSave? b)
		{
			if (a == null && b == null)
				return true;

			if (a == null || b == null)
				return false;

			return a.ModelBase == b.ModelBase &&
				   a.ModelVariant == b.ModelVariant &&
				   a.DyeId == b.DyeId &&
				   a.DyeId2 == b.DyeId2;
		}

		private static bool WeaponEquals(CharacterFile.WeaponSave? a, CharacterFile.WeaponSave? b)
		{
			if (a == null && b == null)
				return true;

			if (a == null || b == null)
				return false;

			return a.ModelSet == b.ModelSet &&
				   a.ModelBase == b.ModelBase &&
				   a.ModelVariant == b.ModelVariant &&
				   a.DyeId == b.DyeId &&
				   a.DyeId2 == b.DyeId2;
		}

		private static bool GlassesEquals(CharacterFile.GlassesSave? a, CharacterFile.GlassesSave? b)
		{
			if (a == null && b == null)
				return true;
			if (a == null || b == null)
				return false;

			return a.GlassesId == b.GlassesId;
		}
	}
}
