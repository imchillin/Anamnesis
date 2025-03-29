// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Lumina.Excel;
using System;
using System.Numerics;

public static class INpcBaseExtensions
{
	public static string ToStringKey(this INpcBase npc)
	{
		Type type = npc.GetType();
		char t;

		if (type == typeof(ResidentNpc))
		{
			t = 'R';
		}
		else if (type == typeof(BattleNpc))
		{
			t = 'B';
		}
		else if (type == typeof(EventNpc))
		{
			t = 'E';
		}
		else if (type == typeof(Companion))
		{
			t = 'C';
		}
		else if (type == typeof(Mount))
		{
			t = 'M';
		}
		else if (type == typeof(Ornament))
		{
			t = 'O';
		}
		else
		{
			throw new Exception($"Unknown Npc Type: {type}");
		}

		return $"{t}:{npc.RowId.ToString("D7")}";
	}

	public static INpcBase FromStringKey(string stringKey)
	{
		string[] parts = stringKey.Split(':');
		if (parts.Length <= 1)
		{
			uint key = uint.Parse(stringKey);
			return GameDataService.ResidentNPCs.GetRow(key);
		}
		else if (parts.Length == 2)
		{
			char t = parts[0][0];
			uint key = uint.Parse(parts[1]);

			return t switch
			{
				'R' => GameDataService.ResidentNPCs.GetRow(key),
				'B' => GameDataService.BattleNPCs.GetRow(key),
				'E' => GameDataService.EventNPCs.GetRow(key),
				'C' => GameDataService.Companions.GetRow(key),
				'M' => GameDataService.Mounts.GetRow(key),
				'O' => GameDataService.Ornaments.GetRow(key),
				_ => throw new Exception($"Unrecognized Npc type key: {t}"),
			};
		}
		else
		{
			throw new Exception($"Unrecognized NPC key: {stringKey}");
		}
	}

	public static CharacterFile ToFile(this INpcBase npc)
	{
		ArgumentNullException.ThrowIfNull(npc);

		ActorCustomizeMemory.Races? race = npc.Race.Value.CustomizeRace;
		ActorCustomizeMemory.Tribes? tribe = npc.Tribe.Value.CustomizeTribe;

		race ??= ActorCustomizeMemory.Races.Hyur;
		tribe ??= ActorCustomizeMemory.Tribes.Midlander;

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
		if (npc.Race.Value.CustomizeRace == ActorCustomizeMemory.Races.Hyur ||
			npc.Race.Value.CustomizeRace == ActorCustomizeMemory.Races.Roegadyn)
		{
			file.Bust = npc.ExtraFeature1;
			file.EarMuscleTailSize = npc.BustOrTone1;
		}
		else
		{
			file.EarMuscleTailSize = npc.ExtraFeature1;
			file.TailEarsType = npc.ExtraFeature2OrBust;
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
			DyeId = (byte)dye.Value.RowId,
			DyeId2 = (byte)dye2.Value.RowId,
		};

		if (isOffHand && item.HasSubModel)
		{
			save.ModelSet = item.SubModelSet;
			save.ModelBase = item.SubModelBase;
			save.ModelVariant = item.SubModelBase;
		}

		return save;
	}

	private static CharacterFile.ItemSave? GearFromItem(IItem? item, RowRef<Stain> dye, RowRef<Stain> dye2)
	{
		if (item == null)
			return null;

		CharacterFile.ItemSave save = new()
		{
			ModelBase = item.ModelBase,
			ModelVariant = (byte)item.ModelVariant,
			DyeId = (byte)dye.Value.RowId,
			DyeId2 = (byte)dye2.Value.RowId,
		};

		return save;
	}
}
