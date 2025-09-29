// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.GameData;

using Anamnesis.Files;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Interface representing the base properties of an NPC.
/// </summary>
public interface INpcBase : IRow
{
	/// <summary>Gets the model character's row ID.</summary>
	uint ModelCharaRow { get; }

	/// <summary>Gets the icon associated with the NPC (if any).</summary>
	ImgRef? Icon { get; }

	/// <summary>Gets the TexTools mod associated with the NPC (if any).</summary>
	Mod? Mod { get; }

	/// <summary>Gets a value indicating whether the NPC is marked as a favorite.</summary>
	bool IsFavorite { get; }

	/// <summary>Gets a value indicating whether the NPC can be marked as a favorite.</summary>
	bool CanFavorite { get; }

	/// <summary>Gets a value indicating whether the NPC has a name.</summary>
	bool HasName { get; }

	/// <summary>Gets the type name of the NPC.</summary>
	string TypeName { get; }

	/// <summary>Gets the color of the face paint.</summary>
	byte FacePaintColor { get; }

	/// <summary>Gets the face paint option of the NPC.</summary>
	byte FacePaint { get; }

	/// <summary>Gets extra feature 2 or bust size.</summary>
	byte ExtraFeature2OrBust { get; }

	/// <summary>Gets extra feature 1.</summary>
	byte ExtraFeature1 { get; }

	/// <summary>Gets the race of the NPC.</summary>
	RowRef<Race> Race { get; }

	/// <summary>Gets the gender of the NPC.</summary>
	byte Gender { get; }

	/// <summary>Gets the body type of the NPC.</summary>
	byte BodyType { get; }

	/// <summary>Gets the height of the NPC.</summary>
	byte Height { get; }

	/// <summary>Gets the tribe of the NPC.</summary>
	RowRef<Tribe> Tribe { get; }

	/// <summary>Gets the face of the NPC.</summary>
	byte Face { get; }

	/// <summary>Gets the hairstyle of the NPC.</summary>
	byte HairStyle { get; }

	/// <summary>Gets a value indicating whether hair highlights are enabled.</summary>
	bool EnableHairHighlight { get; }

	/// <summary>Gets the skin color of the NPC.</summary>
	byte SkinColor { get; }

	/// <summary>Gets the eye heterochromia mode of the NPC.</summary>
	byte EyeHeterochromia { get; }

	/// <summary>Gets the hair highlight color of the NPC.</summary>
	byte HairHighlightColor { get; }

	/// <summary>Gets the facial feature of the NPC.</summary>
	byte FacialFeature { get; }

	/// <summary>Gets the facial feature color of the NPC.</summary>
	byte FacialFeatureColor { get; }

	/// <summary>Gets the eyebrows of the NPC.</summary>
	byte Eyebrows { get; }

	/// <summary>Gets the eye color of the NPC.</summary>
	byte EyeColor { get; }

	/// <summary>Gets the eye shape of the NPC.</summary>
	byte EyeShape { get; }

	/// <summary>Gets the nose of the NPC.</summary>
	byte Nose { get; }

	/// <summary>Gets the jaw of the NPC.</summary>
	byte Jaw { get; }

	/// <summary>Gets the mouth of the NPC.</summary>
	byte Mouth { get; }

	/// <summary>Gets the lip color of the NPC.</summary>
	byte LipColor { get; }

	/// <summary>Gets the bust or tone 1 of the NPC.</summary>
	byte BustOrTone1 { get; }

	/// <summary>Gets the hair color of the NPC.</summary>
	byte HairColor { get; }

	/// <summary>Gets the main hand item of the NPC.</summary>
	IItem MainHand { get; }

	/// <summary>Gets the primary dye for the main hand item.</summary>
	RowRef<Stain> DyeMainHand { get; }

	/// <summary>Gets the secondary dye for the main hand item.</summary>
	RowRef<Stain> Dye2MainHand { get; }

	/// <summary>Gets the off hand item of the NPC.</summary>
	IItem OffHand { get; }

	/// <summary>Gets the primary dye for the off hand item.</summary>
	RowRef<Stain> DyeOffHand { get; }

	/// <summary>Gets the secondary dye for the off hand item.</summary>
	RowRef<Stain> Dye2OffHand { get; }

	/// <summary>Gets the head item of the NPC.</summary>
	IItem Head { get; }

	/// <summary>Gets the primary dye for the head item.</summary>
	RowRef<Stain> DyeHead { get; }

	/// <summary>Gets the secondary dye for the head item.</summary>
	RowRef<Stain> Dye2Head { get; }

	/// <summary>Gets the body item of the NPC.</summary>
	IItem Body { get; }

	/// <summary>Gets the primary dye for the body item.</summary>
	RowRef<Stain> DyeBody { get; }

	/// <summary>Gets the secondary dye for the body item.</summary>
	RowRef<Stain> Dye2Body { get; }

	/// <summary>Gets the legs item of the NPC.</summary>
	IItem Legs { get; }

	/// <summary>Gets the primary dye for the legs item.</summary>
	RowRef<Stain> DyeLegs { get; }

	/// <summary> Gets the secondary dye for the legs item.</summary>
	RowRef<Stain> Dye2Legs { get; }

	/// <summary>Gets the feet item of the NPC.</summary>
	IItem Feet { get; }

	/// <summary>Gets the primary dye for the feet item.</summary>
	RowRef<Stain> DyeFeet { get; }

	/// <summary>Gets the secondary dye for the feet item.</summary>
	RowRef<Stain> Dye2Feet { get; }

	/// <summary>Gets the hands item of the NPC.</summary>
	IItem Hands { get; }

	/// <summary>Gets the primary dye for the hands item.</summary>
	RowRef<Stain> DyeHands { get; }

	/// <summary>Gets the secondary dye for the hands item.</summary>
	RowRef<Stain> Dye2Hands { get; }

	/// <summary>Gets the wrists item of the NPC.</summary>
	IItem Wrists { get; }

	/// <summary>Gets the primary dye for the wrists item.</summary>
	RowRef<Stain> DyeWrists { get; }

	/// <summary>Gets the secondary dye for the wrists item.</summary>
	RowRef<Stain> Dye2Wrists { get; }

	/// <summary>Gets the neck item of the NPC.</summary>
	IItem Neck { get; }

	/// <summary>Gets the primary dye for the neck item.</summary>
	RowRef<Stain> DyeNeck { get; }

	/// <summary>Gets the secondary dye for the neck item.</summary>
	RowRef<Stain> Dye2Neck { get; }

	/// <summary>Gets the ears item of the NPC.</summary>
	IItem Ears { get; }

	/// <summary>Gets the primary dye for the ears item.</summary>
	RowRef<Stain> DyeEars { get; }

	/// <summary>Gets the secondary dye for the ears item.</summary>
	RowRef<Stain> Dye2Ears { get; }

	/// <summary>Gets the left ring item of the NPC.</summary>
	IItem LeftRing { get; }

	/// <summary>Gets the primary dye for the left ring item.</summary>
	RowRef<Stain> DyeLeftRing { get; }

	/// <summary>Gets the secondary dye for the left ring item.</summary>
	RowRef<Stain> Dye2LeftRing { get; }

	/// <summary>Gets the right ring item of the NPC.</summary>
	IItem RightRing { get; }

	/// <summary>Gets the primary dye for the right ring item.</summary>
	RowRef<Stain> DyeRightRing { get; }

	/// <summary>Gets the secondary dye for the right ring item.</summary>
	RowRef<Stain> Dye2RightRing { get; }
}

public static class INpcBaseExtensions
{
	/// <summary>
	/// Maps NPC types to a letter for use in string keys.
	/// </summary>
	private static readonly Dictionary<Type, char> s_typeToCharMap = new()
	{
		{ typeof(ResidentNpc), 'R' },
		{ typeof(BattleNpc), 'B' },
		{ typeof(EventNpc), 'E' },
		{ typeof(Companion), 'C' },
		{ typeof(Mount), 'M' },
		{ typeof(Ornament), 'O' },
	};

	/// <summary>
	/// Maps a letter to a function that retrieves an NPC from the game data service.
	/// </summary>
	private static readonly Dictionary<char, Func<uint, INpcBase>> s_charToServiceMap = new()
	{
		{ 'R', key => GameDataService.ResidentNPCs.GetRow(key) },
		{ 'B', key => GameDataService.BattleNPCs.GetRow(key) },
		{ 'E', key => GameDataService.EventNPCs.GetRow(key) },
		{ 'C', key => GameDataService.Companions.GetRow(key) },
		{ 'M', key => GameDataService.Mounts.GetRow(key) },
		{ 'O', key => GameDataService.Ornaments.GetRow(key) },
	};

	/// <summary>
	/// Converts an NPC to a string key.
	/// </summary>
	/// <param name="npc">The NPC to convert.</param>
	/// <returns>The string key.</returns>
	/// <exception cref="Exception">Thrown if the NPC type is unknown.</exception>
	public static string ToStringKey(this INpcBase npc)
	{
		if (!s_typeToCharMap.TryGetValue(npc.GetType(), out char t))
			throw new Exception($"Unknown Npc Type: {npc.GetType()}");

		return $"{t}:{npc.RowId:D7}";
	}

	/// <summary>
	/// Converts a string key to an NPC.
	/// </summary>
	/// <param name="stringKey">The string key to convert.</param>
	/// <returns>The NPC.</returns>
	/// <exception cref="Exception">Thrown if the NPC key is unrecognized.</exception>
	public static INpcBase FromStringKey(string stringKey)
	{
		string[] parts = stringKey.Split(':');
		if (parts.Length == 1)
		{
			uint key = uint.Parse(stringKey);
			return GameDataService.ResidentNPCs.GetRow(key);
		}
		else if (parts.Length == 2 && s_charToServiceMap.TryGetValue(parts[0][0], out var getService))
		{
			uint key = uint.Parse(parts[1]);
			return getService(key);
		}
		else
		{
			throw new Exception($"Unrecognized NPC key: {stringKey}");
		}
	}

	/// <summary>
	/// Converts an NPC to a character file.
	/// </summary>
	/// <param name="npc">The NPC to convert.</param>
	/// <returns>A character file.</returns>
	public static CharacterFile ToFile(this INpcBase npc)
	{
		ArgumentNullException.ThrowIfNull(npc);

		ActorCustomizeMemory.Races race = npc.Race.IsValid
			? npc.Race.Value.CustomizeRace
			: ActorCustomizeMemory.Races.Hyur;
		ActorCustomizeMemory.Tribes tribe = npc.Race.IsValid
			? npc.Tribe.Value.CustomizeTribe
			: ActorCustomizeMemory.Tribes.Midlander;

		CharacterFile file = new()
		{
			SaveMode = CharacterFile.SaveModes.All,
			ModelType = npc.ModelCharaRow,
			Race = race,
			Tribe = tribe,
			Gender = (ActorCustomizeMemory.Genders)npc.Gender,
			Age = (ActorCustomizeMemory.Ages)npc.BodyType,
			Height = Math.Min(npc.Height, (byte)100),
			Head = npc.Face,
			Hair = npc.HairStyle,
			EnableHighlights = npc.EnableHairHighlight,
			Skintone = npc.SkinColor,
			REyeColor = npc.EyeHeterochromia,
			LEyeColor = npc.EyeColor,
			HairTone = npc.HairColor,
			Highlights = npc.HairHighlightColor,
			FacialFeatures = (ActorCustomizeMemory.FacialFeature)npc.FacialFeature,
			LimbalEyes = npc.FacialFeatureColor,
			Eyebrows = npc.Eyebrows,
			Eyes = npc.EyeShape,
			Nose = npc.Nose,
			Jaw = npc.Jaw,
			Mouth = npc.Mouth,
			LipsToneFurPattern = npc.LipColor,
		};

		// Hyurs and Roegadyn get muscle sliders, while everyone else
		// Gets custom tails or ears.
		if (race is ActorCustomizeMemory.Races.Hyur or ActorCustomizeMemory.Races.Roegadyn)
		{
			file.Bust = npc.ExtraFeature1;
			file.EarMuscleTailSize = npc.BustOrTone1;
		}
		else
		{
			file.EarMuscleTailSize = npc.ExtraFeature2OrBust;
			file.TailEarsType = npc.ExtraFeature1;
			file.Bust = npc.BustOrTone1;
		}

		file.FacePaint = npc.FacePaint;
		file.FacePaintColor = npc.FacePaintColor;

		file.MainHand = WeaponFromItem(npc.MainHand, npc.DyeMainHand, npc.Dye2MainHand);
		file.OffHand = WeaponFromItem(npc.OffHand, npc.DyeOffHand, npc.Dye2OffHand, true);

		file.HeadGear = GearFromItem(npc.Head, npc.DyeHead, npc.Dye2Head);
		file.Body = GearFromItem(npc.Body, npc.DyeBody, npc.Dye2Body);
		file.Hands = GearFromItem(npc.Hands, npc.DyeHands, npc.Dye2Hands);
		file.Legs = GearFromItem(npc.Legs, npc.DyeLegs, npc.Dye2Legs);
		file.Feet = GearFromItem(npc.Feet, npc.DyeFeet, npc.Dye2Feet);
		file.Ears = GearFromItem(npc.Ears, npc.DyeEars, npc.Dye2Ears);
		file.Neck = GearFromItem(npc.Neck, npc.DyeNeck, npc.Dye2Neck);
		file.Wrists = GearFromItem(npc.Wrists, npc.DyeWrists, npc.Dye2Wrists);
		file.LeftRing = GearFromItem(npc.LeftRing, npc.DyeLeftRing, npc.Dye2LeftRing);
		file.RightRing = GearFromItem(npc.RightRing, npc.DyeRightRing, npc.Dye2RightRing);

		return file;
	}

	/// <summary>
	/// Converts an item to a weapon chracter file save.
	/// </summary>
	/// <param name="item">The item to convert.</param>
	/// <param name="dye">The primary dye to use.</param>
	/// <param name="dye2">The secondary dye to use.</param>
	/// <param name="isOffHand">A value indicating whether the item is an off-hand item.</param>
	/// <returns>A weapon save for a character file.</returns>
	private static CharacterFile.WeaponSave? WeaponFromItem(IItem? item, RowRef<Stain> dye, RowRef<Stain> dye2, bool isOffHand = false)
	{
		if (item == null)
			return null;

		CharacterFile.WeaponSave save = new()
		{
			Color = Color.White,
			Scale = Vector3.One,
			ModelSet = item.ModelSet,
			ModelBase = item.ModelBase,
			ModelVariant = item.ModelVariant,
			DyeId = dye.IsValid ? (byte)dye.Value.RowId : (byte)0,
			DyeId2 = dye2.IsValid ? (byte)dye2.Value.RowId : (byte)0,
		};

		if (isOffHand && item.HasSubModel)
		{
			save.ModelSet = item.SubModelSet;
			save.ModelBase = item.SubModelBase;
			save.ModelVariant = item.SubModelBase;
		}

		return save;
	}

	/// <summary>
	/// Converts an item to a gear character file save.
	/// </summary>
	/// <param name="item">The item to convert.</param>
	/// <param name="dye">The primary dye to use.</param>
	/// <param name="dye2">The secondary dye to use.</param>
	/// <returns>A gear save for a character file.</returns>
	private static CharacterFile.ItemSave? GearFromItem(IItem? item, RowRef<Stain> dye, RowRef<Stain> dye2)
	{
		if (item == null)
			return null;

		CharacterFile.ItemSave save = new()
		{
			ModelBase = item.ModelBase,
			ModelVariant = (byte)item.ModelVariant,
			DyeId = dye.IsValid ? (byte)dye.Value.RowId : (byte)0,
			DyeId2 = dye2.IsValid ? (byte)dye2.Value.RowId : (byte)0,
		};

		return save;
	}
}
