// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Serilog;
using System;
using System.Collections.Generic;
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
	public CharFileDiff CompareTo(CharacterFile target, SaveModes mode = SaveModes.All)
	{
		return CharFileDiff.Create(current: this, target: target, mode: mode);
	}

	internal void WriteToFile(ActorMemory actor, SaveModes mode)
	{
		this.Nickname = actor.Nickname;
		this.ModelType = (uint)actor.ModelType;
		this.ObjectKind = actor.ObjectKind;

		var drawData = actor.DrawData;
		if (drawData.Customize == null || drawData.Equipment == null)
			return; // Malformed actor object. Don't attempt to write.

		this.SaveMode = mode;

		if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
		{
			if (drawData.MainHand != null)
				this.MainHand = new WeaponSave(drawData.MainHand);
			////this.MainHand.Color = actor.GetValue(Offsets.Main.MainHandColor);
			////this.MainHand.Scale = actor.GetValue(Offsets.Main.MainHandScale);

			if (drawData.OffHand != null)
				this.OffHand = new WeaponSave(drawData.OffHand);
			////this.OffHand.Color = actor.GetValue(Offsets.Main.OffhandColor);
			////this.OffHand.Scale = actor.GetValue(Offsets.Main.OffhandScale);
		}

		if (this.IncludeSection(SaveModes.EquipmentGear, mode))
		{
			if (drawData.Equipment?.Head != null)
				this.HeadGear = new ItemSave(drawData.Equipment.Head);

			if (drawData.Equipment?.Chest != null)
				this.Body = new ItemSave(drawData.Equipment.Chest);

			if (drawData.Equipment?.Arms != null)
				this.Hands = new ItemSave(drawData.Equipment.Arms);

			if (drawData.Equipment?.Legs != null)
				this.Legs = new ItemSave(drawData.Equipment.Legs);

			if (drawData.Equipment?.Feet != null)
				this.Feet = new ItemSave(drawData.Equipment.Feet);

			// glasses - technically on the left side
			if (drawData.Glasses != null)
				this.Glasses = new GlassesSave(drawData.Glasses);
		}

		if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
		{
			if (drawData.Equipment?.Ear != null)
				this.Ears = new ItemSave(drawData.Equipment.Ear);

			if (drawData.Equipment?.Neck != null)
				this.Neck = new ItemSave(drawData.Equipment.Neck);

			if (drawData.Equipment?.Wrist != null)
				this.Wrists = new ItemSave(drawData.Equipment.Wrist);

			if (drawData.Equipment?.LFinger != null)
				this.LeftRing = new ItemSave(drawData.Equipment.LFinger);

			if (drawData.Equipment?.RFinger != null)
				this.RightRing = new ItemSave(drawData.Equipment.RFinger);
		}

		if (this.IncludeSection(SaveModes.AppearanceHair, mode))
		{
			this.Hair = drawData.Customize?.Hair;
			this.EnableHighlights = drawData.Customize?.EnableHighlights;
			this.HairTone = drawData.Customize?.HairTone;
			this.Highlights = drawData.Customize?.Highlights;
			this.HairColor = actor.ModelObject?.ExtendedAppearance?.HairColor;
			this.HairGloss = actor.ModelObject?.ExtendedAppearance?.HairGloss;
			this.HairHighlight = actor.ModelObject?.ExtendedAppearance?.HairHighlight;
		}

		if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
		{
			this.Race = drawData.Customize?.Race;
			this.Gender = drawData.Customize?.Gender;
			this.Tribe = drawData.Customize?.Tribe;
			this.Age = drawData.Customize?.Age;
		}

		if (this.IncludeSection(SaveModes.AppearanceFace, mode))
		{
			this.Head = drawData.Customize?.Head;
			this.REyeColor = drawData.Customize?.REyeColor;
			this.LimbalEyes = drawData.Customize?.FacialFeatureColor;
			this.FacialFeatures = drawData.Customize?.FacialFeatures;
			this.Eyebrows = drawData.Customize?.Eyebrows;
			this.LEyeColor = drawData.Customize?.LEyeColor;
			this.Eyes = drawData.Customize?.Eyes;
			this.Nose = drawData.Customize?.Nose;
			this.Jaw = drawData.Customize?.Jaw;
			this.Mouth = drawData.Customize?.Mouth;
			this.LipsToneFurPattern = drawData.Customize?.LipsToneFurPattern;
			this.FacePaint = drawData.Customize?.FacePaint;
			this.FacePaintColor = drawData.Customize?.FacePaintColor;
			this.LeftEyeColor = actor.ModelObject?.ExtendedAppearance?.LeftEyeColor;
			this.RightEyeColor = actor.ModelObject?.ExtendedAppearance?.RightEyeColor;
			this.LimbalRingColor = actor.ModelObject?.ExtendedAppearance?.LimbalRingColor;
			this.MouthColor = actor.ModelObject?.ExtendedAppearance?.MouthColor;
		}

		if (this.IncludeSection(SaveModes.AppearanceBody, mode))
		{
			this.Height = drawData.Customize?.Height;
			this.Skintone = drawData.Customize?.Skintone;
			this.EarMuscleTailSize = drawData.Customize?.EarMuscleTailSize;
			this.TailEarsType = drawData.Customize?.TailEarsType;
			this.Bust = drawData.Customize?.Bust;

			this.HeightMultiplier = actor.ModelObject?.Height;
			this.SkinColor = actor.ModelObject?.ExtendedAppearance?.SkinColor;
			this.SkinGloss = actor.ModelObject?.ExtendedAppearance?.SkinGloss;
			this.MuscleTone = actor.ModelObject?.ExtendedAppearance?.MuscleTone;
			this.BustScale = actor.ModelObject?.Bust?.Scale;
			this.Transparency = actor.Transparency;
		}
	}

	internal async Task Apply(ActorMemory actor, SaveModes mode, ItemSlots? slot = null)
	{
		if (this.Tribe == 0)
			this.Tribe = ActorCustomizeMemory.Tribes.Midlander;

		if (this.Race == 0)
			this.Race = ActorCustomizeMemory.Races.Hyur;

		if (this.Tribe != null && !Enum.IsDefined((ActorCustomizeMemory.Tribes)this.Tribe))
			throw new Exception($"Invalid tribe: {this.Tribe} in appearance file");

		if (this.Race != null && !Enum.IsDefined((ActorCustomizeMemory.Races)this.Race))
			throw new Exception($"Invalid race: {this.Race} in appearance file");

		var drawData = actor.DrawData;
		if (drawData.Customize == null)
			return;

		this.Glasses ??= new GlassesSave();
		if (!actor.CanRefresh)
		{
			Log.Warning($"Actor '{actor.Name}' cannot be refreshed. Appearance will not be applied.");
			return;
		}

		Log.Information("Reading appearance from file");
		var existingModelType = actor.ModelType;
		var existingRace = drawData.Customize.Race;
		var existingGender = drawData.Customize.Gender;
		var existingTribe = drawData.Customize.Tribe;
		var existingHead = drawData.Customize.Head;

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
				this.MainHand?.Write(drawData.MainHand, true);
				this.OffHand?.Write(drawData.OffHand, false);
				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				WriteEquipment(this.HeadGear, drawData.Equipment?.Head);
				WriteEquipment(this.Body, drawData.Equipment?.Chest);
				WriteEquipment(this.Hands, drawData.Equipment?.Arms);
				WriteEquipment(this.Legs, drawData.Equipment?.Legs);
				WriteEquipment(this.Feet, drawData.Equipment?.Feet);

				if (this.Glasses != null)
					this.Glasses.Write(drawData.Glasses);
				else if (drawData.Glasses != null)
					drawData.Glasses.GlassesId = 0;

				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				WriteEquipment(this.Ears, drawData.Equipment?.Ear);
				WriteEquipment(this.Neck, drawData.Equipment?.Neck);
				WriteEquipment(this.Wrists, drawData.Equipment?.Wrist);
				WriteEquipment(this.LeftRing, drawData.Equipment?.LFinger);
				WriteEquipment(this.RightRing, drawData.Equipment?.RFinger);

				needsRedraw = true;
			}

			if (mode == SaveModes.EquipmentSlot && slot != null)
			{
				switch (slot)
				{
					case ItemSlots.MainHand:
						this.MainHand?.Write(drawData.MainHand, true);
						break;
					case ItemSlots.OffHand:
						this.OffHand?.Write(drawData.OffHand, false);
						break;
					case ItemSlots.Head:
						this.HeadGear?.Write(drawData.Equipment?.Head);
						break;
					case ItemSlots.Body:
						this.Body?.Write(drawData.Equipment?.Chest);
						break;
					case ItemSlots.Hands:
						this.Hands?.Write(drawData.Equipment?.Arms);
						break;
					case ItemSlots.Legs:
						this.Legs?.Write(drawData.Equipment?.Legs);
						break;
					case ItemSlots.Feet:
						this.Feet?.Write(drawData.Equipment?.Feet);
						break;
					case ItemSlots.Ears:
						this.Ears?.Write(drawData.Equipment?.Ear);
						break;
					case ItemSlots.Neck:
						this.Neck?.Write(drawData.Equipment?.Neck);
						break;
					case ItemSlots.Wrists:
						this.Wrists?.Write(drawData.Equipment?.Wrist);
						break;
					case ItemSlots.LeftRing:
						this.LeftRing?.Write(drawData.Equipment?.LFinger);
						break;
					case ItemSlots.RightRing:
						this.RightRing?.Write(drawData.Equipment?.RFinger);
						break;
					case ItemSlots.Glasses:
						this.Glasses?.Write(drawData.Glasses);
						break;
				}

				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				if (this.Hair != null)
					drawData.Customize.Hair = (byte)this.Hair;

				if (this.EnableHighlights != null)
					drawData.Customize.EnableHighlights = (bool)this.EnableHighlights;

				if (this.HairTone != null)
					drawData.Customize.HairTone = (byte)this.HairTone;

				if (this.Highlights != null)
					drawData.Customize.Highlights = (byte)this.Highlights;

				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Race != null)
					drawData.Customize.Race = (ActorCustomizeMemory.Races)this.Race;

				if (this.Gender != null)
					drawData.Customize.Gender = (ActorCustomizeMemory.Genders)this.Gender;

				if (this.Tribe != null)
					drawData.Customize.Tribe = (ActorCustomizeMemory.Tribes)this.Tribe;

				if (this.Age != null)
					drawData.Customize.Age = (ActorCustomizeMemory.Ages)this.Age;

				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				if (this.Head != null)
					drawData.Customize.Head = (byte)this.Head;

				if (this.REyeColor != null)
					drawData.Customize.REyeColor = (byte)this.REyeColor;

				if (this.FacialFeatures != null)
					drawData.Customize.FacialFeatures = (ActorCustomizeMemory.FacialFeature)this.FacialFeatures;

				if (this.LimbalEyes != null)
					drawData.Customize.FacialFeatureColor = (byte)this.LimbalEyes;

				if (this.Eyebrows != null)
					drawData.Customize.Eyebrows = (byte)this.Eyebrows;

				if (this.LEyeColor != null)
					drawData.Customize.LEyeColor = (byte)this.LEyeColor;

				if (this.Eyes != null)
					drawData.Customize.Eyes = (byte)this.Eyes;

				if (this.Nose != null)
					drawData.Customize.Nose = (byte)this.Nose;

				if (this.Jaw != null)
					drawData.Customize.Jaw = (byte)this.Jaw;

				if (this.Mouth != null)
					drawData.Customize.Mouth = (byte)this.Mouth;

				if (this.LipsToneFurPattern != null)
					drawData.Customize.LipsToneFurPattern = (byte)this.LipsToneFurPattern;

				if (this.FacePaint != null)
					drawData.Customize.FacePaint = (byte)this.FacePaint;

				if (this.FacePaintColor != null)
					drawData.Customize.FacePaintColor = (byte)this.FacePaintColor;

				needsRedraw = true;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Height != null)
					drawData.Customize.Height = (byte)this.Height;

				if (this.Skintone != null)
					drawData.Customize.Skintone = (byte)this.Skintone;

				if (this.EarMuscleTailSize != null)
					drawData.Customize.EarMuscleTailSize = (byte)this.EarMuscleTailSize;

				if (this.TailEarsType != null)
					drawData.Customize.TailEarsType = (byte)this.TailEarsType;

				if (this.Bust != null)
					drawData.Customize.Bust = (byte)this.Bust;

				needsRedraw = true;
			}

			// Determine if the actor needs to be redrawn based on structural changes
			isStructuralChange |=
				drawData.Customize.Race != existingRace ||
				drawData.Customize.Gender != existingGender ||
				drawData.Customize.Tribe != existingTribe ||
				drawData.Customize.Head != existingHead;

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
				await actor.Refresh();

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

	/// <summary>
	/// Represents the set of changes between two <see cref="CharacterFile"/> snapshots.
	/// Used to determine the optimal redraw strategy.
	/// </summary>
	public readonly struct CharFileDiff
	{
		[Flags]
		public enum ChangeType : uint
		{
			/// <summary>
			/// Identical character customization.
			/// </summary>
			None = 0,

			/// <summary>
			/// The base customization options between the character files is different.
			/// </summary>
			Base = 1 << 0,

			/// <summary>
			/// The appearance customization options between the character files is different.
			/// </summary>
			Appearance = 1 << 1,

			/// <summary>
			/// The extended appearance options between the character files is different.
			/// </summary>
			Extended = 1 << 2,

			/// <summary>
			/// The equipment between the character files is different (gear and accessories, excluding weapons).
			/// </summary>
			Equipment = 1 << 3,

			/// <summary>
			/// The weapon customization between the character files is different.
			/// </summary>
			Weapon = 1 << 4,

			/// <summary>
			/// The character's facewear is different between the character files.
			/// </summary>
			Facewear = 1 << 5,

			All = Base | Appearance | Extended | Equipment | Weapon | Facewear,
		}

		/// <summary>
		/// A flag enum that represents all changed sections between two character files.
		/// </summary>
		public ChangeType Changes { get; init; }

		/// <summary>
		/// Gets whether any changes were detected between the two character files.
		/// </summary>
		public bool HasChanges => this.Changes != ChangeType.None;

		/// <summary>
		/// Computes the differences between two character files.
		/// </summary>
		/// <param name="current">The current state (from memory).</param>
		/// <param name="target">The target state (to apply).</param>
		/// <param name="mode">Which sections are being applied.</param>
		public static CharFileDiff Create(
			CharacterFile current,
			CharacterFile target,
			CharacterFile.SaveModes mode = CharacterFile.SaveModes.All)
		{
			ArgumentNullException.ThrowIfNull(current);
			ArgumentNullException.ThrowIfNull(target);

			ChangeType flags = ChangeType.None;

			if (CheckBaseChanges(current, target))
				flags |= ChangeType.Base;

			if (CheckWeaponsCompatibility(current, target, mode))
				flags |= ChangeType.Base;

			if (CheckAppearanceChanges(current, target, mode))
				flags |= ChangeType.Appearance;

			if (CheckExtendedAppearanceChanges(current, target, mode))
				flags |= ChangeType.Extended;

			if (CheckEquipmentChanges(current, target, mode))
				flags |= ChangeType.Equipment;

			if (CheckWeaponChanges(current, target, mode))
				flags |= ChangeType.Weapon;

			if (CheckFacewearChanges(current, target, mode))
				flags |= ChangeType.Facewear;

			return new CharFileDiff { Changes = flags };
		}

		public override string ToString()
		{
			if (!this.HasChanges)
				return "No changes";

			var parts = new List<string>();
			if (this.Changes.HasFlagUnsafe(ChangeType.Base))
				parts.Add("Base");
			if (this.Changes.HasFlagUnsafe(ChangeType.Appearance))
				parts.Add("Appearance");
			if (this.Changes.HasFlagUnsafe(ChangeType.Extended))
				parts.Add("Extended");
			if (this.Changes.HasFlagUnsafe(ChangeType.Equipment))
				parts.Add("Equipment");
			if (this.Changes.HasFlagUnsafe(ChangeType.Weapon))
				parts.Add("Weapon");
			if (this.Changes.HasFlagUnsafe(ChangeType.Facewear))
				parts.Add("Facewear");

			return $"Changed: {string.Join(", ", parts)}";
		}

		private static bool CheckBaseChanges(CharacterFile c, CharacterFile t)
		{
			return c.ModelType != t.ModelType ||
				   c.Race != t.Race ||
				   c.Gender != t.Gender ||
				   c.Tribe != t.Tribe ||
				   c.Head != t.Head;
		}

		private static bool CheckAppearanceChanges(CharacterFile c, CharacterFile t, CharacterFile.SaveModes mode)
		{
			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.AppearanceHair) &&
				(c.Hair != t.Hair ||
				c.EnableHighlights != t.EnableHighlights ||
				c.HairTone != t.HairTone ||
				c.Highlights != t.Highlights))
				return true;

			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.AppearanceFace) &&
				(c.REyeColor != t.REyeColor ||
				c.LEyeColor != t.LEyeColor ||
				c.FacialFeatures != t.FacialFeatures ||
				c.LimbalEyes != t.LimbalEyes ||
				c.Eyebrows != t.Eyebrows ||
				c.Eyes != t.Eyes ||
				c.Nose != t.Nose ||
				c.Jaw != t.Jaw ||
				c.Mouth != t.Mouth ||
				c.LipsToneFurPattern != t.LipsToneFurPattern ||
				c.FacePaint != t.FacePaint ||
				c.FacePaintColor != t.FacePaintColor))
				return true;

			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.AppearanceBody) &&
				(c.Height != t.Height ||
				c.Skintone != t.Skintone ||
				c.EarMuscleTailSize != t.EarMuscleTailSize ||
				c.TailEarsType != t.TailEarsType ||
				c.Bust != t.Bust))
				return true;

			return false;
		}

		private static bool CheckExtendedAppearanceChanges(CharacterFile c, CharacterFile t, CharacterFile.SaveModes mode)
		{
			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.AppearanceHair) &&
				(c.HairColor != t.HairColor ||
				c.HairGloss != t.HairGloss ||
				c.HairHighlight != t.HairHighlight))
				return true;

			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.AppearanceFace) &&
				(c.LeftEyeColor != t.LeftEyeColor ||
				c.RightEyeColor != t.RightEyeColor ||
				c.LimbalRingColor != t.LimbalRingColor ||
				c.MouthColor != t.MouthColor))
				return true;

			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.AppearanceBody) &&
				(c.SkinColor != t.SkinColor ||
				c.SkinGloss != t.SkinGloss ||
				c.MuscleTone != t.MuscleTone ||
				c.HeightMultiplier != t.HeightMultiplier ||
				c.BustScale != t.BustScale ||
				c.Transparency != t.Transparency))
				return true;

			return false;
		}

		private static bool CheckEquipmentChanges(CharacterFile c, CharacterFile t, CharacterFile.SaveModes mode)
		{
			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.EquipmentGear) &&
				(c.HeadGear != t.HeadGear ||
				c.Body != t.Body ||
				c.Hands != t.Hands ||
				c.Legs != t.Legs ||
				c.Feet != t.Feet))
				return true;

			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.EquipmentAccessories) &&
				(c.Ears != t.Ears ||
				c.Neck != t.Neck ||
				c.Wrists != t.Wrists ||
				c.LeftRing != t.LeftRing ||
				c.RightRing != t.RightRing))
				return true;

			return false;
		}

		private static bool CheckWeaponChanges(CharacterFile c, CharacterFile t, CharacterFile.SaveModes mode)
		{
			if (!mode.HasFlagUnsafe(CharacterFile.SaveModes.EquipmentWeapons))
				return false;

			return c.MainHand != t.MainHand || c.OffHand != t.OffHand;
		}

		private static bool CheckWeaponsCompatibility(CharacterFile c, CharacterFile t, CharacterFile.SaveModes mode)
		{
			if (!mode.HasFlagUnsafe(CharacterFile.SaveModes.EquipmentWeapons))
				return false;

			// Treat all crafters and all gatherers as equivalent
			// since they use the Paladin battle/idle animation.
			static bool AreBothCraftersOrGatherers(Classes a, Classes b)
			{
				return ((a & (Classes.Crafters | Classes.Gatherers)) != 0) &&
					   ((b & (Classes.Crafters | Classes.Gatherers)) != 0);
			}

			static bool IsDummyTransition(object curItem, object tgtItem, object dummy)
			{
				return (ReferenceEquals(curItem, dummy) && !ReferenceEquals(tgtItem, dummy)) ||
					   (!ReferenceEquals(curItem, dummy) && ReferenceEquals(tgtItem, dummy));
			}

			static bool NeedsRedrawForSlotChange(object? curItem, object? tgtItem, ItemSlots slot)
			{
				if (curItem is Anamnesis.GameData.Excel.Item curExcel && tgtItem is Anamnesis.GameData.Excel.Item tgtExcel)
				{
					bool curFits = curExcel.FitsInSlot(slot);
					bool tgtFits = tgtExcel.FitsInSlot(slot);
					return curFits != tgtFits;
				}

				return false;
			}

			if (c.MainHand is not null && t.MainHand is not null && !c.MainHand.Equals(t.MainHand))
			{
				var curItem = ItemUtility.GetItem(ItemSlots.MainHand, c.MainHand.ModelSet, c.MainHand.ModelBase, c.MainHand.ModelVariant, false);
				var tgtItem = ItemUtility.GetItem(ItemSlots.MainHand, t.MainHand.ModelSet, t.MainHand.ModelBase, t.MainHand.ModelVariant, false);

				if (IsDummyTransition(curItem, tgtItem, ItemUtility.NoneItem) ||
					IsDummyTransition(curItem, tgtItem, ItemUtility.EmperorsNewFists))
					return true;

				if (NeedsRedrawForSlotChange(curItem, tgtItem, ItemSlots.MainHand))
					return true;

				if (curItem is Anamnesis.GameData.Excel.Item curExcel && tgtItem is Anamnesis.GameData.Excel.Item tgtExcel)
				{
					var curFlags = curExcel.ClassJobCategory.Value.ToFlags();
					var tgtFlags = tgtExcel.ClassJobCategory.Value.ToFlags();

					if (AreBothCraftersOrGatherers(curFlags, tgtFlags))
						return false;

					if ((curFlags & tgtFlags) == 0)
						return true;
				}
			}

			if (c.OffHand is not null && t.OffHand is not null && !c.OffHand.Equals(t.OffHand))
			{
				var curItem = ItemUtility.GetItem(ItemSlots.OffHand, c.OffHand.ModelSet, c.OffHand.ModelBase, c.OffHand.ModelVariant, false);
				var tgtItem = ItemUtility.GetItem(ItemSlots.OffHand, t.OffHand.ModelSet, t.OffHand.ModelBase, t.OffHand.ModelVariant, false);

				if (IsDummyTransition(curItem, tgtItem, ItemUtility.NoneItem) ||
					IsDummyTransition(curItem, tgtItem, ItemUtility.EmperorsNewFists))
					return true;

				if (NeedsRedrawForSlotChange(curItem, tgtItem, ItemSlots.MainHand))
					return true;

				if (curItem is Anamnesis.GameData.Excel.Item curExcel && tgtItem is Anamnesis.GameData.Excel.Item tgtExcel)
				{
					var curFlags = curExcel.ClassJobCategory.Value.ToFlags();
					var tgtFlags = tgtExcel.ClassJobCategory.Value.ToFlags();

					if (AreBothCraftersOrGatherers(curFlags, tgtFlags))
						return false;

					if ((curFlags & tgtFlags) == 0)
						return true;
				}
			}

			return false;
		}

		private static bool CheckFacewearChanges(CharacterFile c, CharacterFile t, CharacterFile.SaveModes mode)
		{
			if (mode.HasFlagUnsafe(CharacterFile.SaveModes.EquipmentGear) && c.Glasses != t.Glasses)
				return true;

			return false;
		}
	}

	[Serializable]
	public record WeaponSave
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

		public virtual bool Equals(WeaponSave? other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return this.ModelSet == other.ModelSet &&
				   this.ModelBase == other.ModelBase &&
				   this.ModelVariant == other.ModelVariant &&
				   this.DyeId == other.DyeId &&
				   this.DyeId2 == other.DyeId2;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(this.ModelSet, this.ModelBase, this.ModelVariant, this.DyeId, this.DyeId2);
		}
	}

	[Serializable]
	public record ItemSave
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
	public record GlassesSave
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
}
