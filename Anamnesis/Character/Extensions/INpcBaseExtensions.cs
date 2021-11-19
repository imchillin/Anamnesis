// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	public static class INpcBaseExtensions
	{
		public static string ToStringKey(this INpcBase npc)
		{
			Type type = npc.GetType();
			char t;

			if (type == typeof(NpcResidentViewModel))
			{
				t = 'R';
			}
			else if (type == typeof(BNpcBaseViewModel))
			{
				t = 'B';
			}
			else if (type == typeof(ENpcBaseViewModel))
			{
				t = 'E';
			}
			else if (type == typeof(CompanionViewModel))
			{
				t = 'C';
			}
			else if (type == typeof(MountViewModel))
			{
				t = 'M';
			}
			else
			{
				throw new Exception($"Unknown Npc Type: {type}");
			}

			return $"{t}:{npc.Key}";
		}

		public static INpcBase FromStringKey(string stringKey)
		{
			string[] parts = stringKey.Split(':');
			if (parts.Length <= 1)
			{
				uint key = uint.Parse(stringKey);
				return GameDataService.ResidentNPCs.Get(key);
			}
			else if (parts.Length == 2)
			{
				char t = parts[0][0];
				uint key = uint.Parse(parts[1]);

				return t switch
				{
					'R' => GameDataService.ResidentNPCs.Get(key),
					'B' => GameDataService.BattleNPCs.Get(key),
					'E' => GameDataService.EventNPCs.Get(key),
					'C' => GameDataService.Companions.Get(key),
					'M' => GameDataService.Mounts.Get(key),
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
			CharacterFile file = new CharacterFile();
			file.SaveMode = CharacterFile.SaveModes.All;
			file.ModelType = npc.ModelCharaRow;
			file.Race = npc.Race.CustomizeRace;
			file.Tribe = npc.Tribe.CustomizeTribe;
			file.Gender = (ActorCustomizeMemory.Genders)npc.Gender;
			file.Age = (ActorCustomizeMemory.Ages)npc.BodyType;
			file.Height = (byte)Math.Min(npc.Height, 100);
			file.Head = (byte)npc.Face;
			file.Hair = (byte)npc.HairStyle;
			file.EnableHighlights = npc.EnableHairHighlight;
			file.Skintone = (byte)npc.SkinColor;
			file.REyeColor = (byte)npc.EyeColor;
			file.LEyeColor = (byte)npc.EyeHeterochromia;
			file.HairTone = (byte)npc.HairColor;
			file.Highlights = (byte)npc.HairHighlightColor;
			file.FacialFeatures = (ActorCustomizeMemory.FacialFeature)npc.FacialFeature;
			file.LimbalEyes = (byte)npc.FacialFeatureColor;
			file.Eyebrows = (byte)npc.Eyebrows;
			file.Eyes = (byte)npc.EyeShape;
			file.Nose = (byte)npc.Nose;
			file.Jaw = (byte)npc.Jaw;
			file.Mouth = (byte)npc.Mouth;
			file.LipsToneFurPattern = (byte)npc.LipColor;
			file.EarMuscleTailSize = (byte)npc.ExtraFeature1;
			file.TailEarsType = (byte)npc.ExtraFeature2OrBust;
			file.Bust = (byte)npc.ExtraFeature2OrBust;
			file.FacePaint = (byte)npc.FacePaint;
			file.FacePaintColor = (byte)npc.FacePaintColor;

			file.MainHand = WeaponFromItem(npc.NpcEquip.MainHand, npc.NpcEquip.DyeMainHand);
			file.OffHand = WeaponFromItem(npc.NpcEquip.OffHand, npc.NpcEquip.DyeOffHand);

			file.HeadGear = GearFromItem(npc.NpcEquip.Head, npc.NpcEquip.DyeHead);
			file.Body = GearFromItem(npc.NpcEquip.Body, npc.NpcEquip.DyeBody);
			file.Hands = GearFromItem(npc.NpcEquip.Hands, npc.NpcEquip.DyeHands);
			file.Legs = GearFromItem(npc.NpcEquip.Legs, npc.NpcEquip.DyeLegs);
			file.Feet = GearFromItem(npc.NpcEquip.Feet, npc.NpcEquip.DyeFeet);
			file.Ears = GearFromItem(npc.NpcEquip.Ears, npc.NpcEquip.DyeEars);
			file.Neck = GearFromItem(npc.NpcEquip.Neck, npc.NpcEquip.DyeNeck);
			file.Wrists = GearFromItem(npc.NpcEquip.Wrists, npc.NpcEquip.DyeWrists);
			file.LeftRing = GearFromItem(npc.NpcEquip.LeftRing, npc.NpcEquip.DyeLeftRing);
			file.RightRing = GearFromItem(npc.NpcEquip.RightRing, npc.NpcEquip.DyeRightRing);

			return file;
		}

		private static CharacterFile.WeaponSave WeaponFromItem(IItem item, IDye dye)
		{
			CharacterFile.WeaponSave save = new CharacterFile.WeaponSave();

			save.Color = Color.White;
			save.Scale = Vector.One;
			save.ModelSet = item.ModelSet;
			save.ModelBase = item.ModelBase;
			save.ModelVariant = item.ModelVariant;

			if (dye != null)
				save.DyeId = dye.Id;

			return save;
		}

		private static CharacterFile.ItemSave GearFromItem(IItem item, IDye dye)
		{
			CharacterFile.ItemSave save = new CharacterFile.ItemSave();

			save.ModelBase = item.ModelBase;
			save.ModelVariant = (byte)item.ModelVariant;

			if (dye != null)
				save.DyeId = dye.Id;

			return save;
		}
	}
}
