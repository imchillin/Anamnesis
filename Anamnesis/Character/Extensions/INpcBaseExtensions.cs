// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
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
			else if (type == typeof(EventNpc) || type == typeof(EventNpcBase))
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

			return $"{t}:{npc.RowId}";
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
			INpcAppearance? appearance = npc.GetAppearance();

			if (appearance == null)
				throw new Exception($"No NPc appearance for npc: {npc}");

			return appearance.ToFile();
		}

		public static CharacterFile ToFile(this INpcAppearance npc)
		{
			if (npc.Race == null || npc.Tribe == null)
				throw new Exception("NPC missing race or tribe");

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

			file.MainHand = WeaponFromItem(npc.MainHand, npc.DyeMainHand);
			file.OffHand = WeaponFromItem(npc.OffHand, npc.DyeOffHand);

			file.HeadGear = GearFromItem(npc.Head, npc.DyeHead);
			file.Body = GearFromItem(npc.Body, npc.DyeBody);
			file.Hands = GearFromItem(npc.Hands, npc.DyeHands);
			file.Legs = GearFromItem(npc.Legs, npc.DyeLegs);
			file.Feet = GearFromItem(npc.Feet, npc.DyeFeet);
			file.Ears = GearFromItem(npc.Ears, npc.DyeEars);
			file.Neck = GearFromItem(npc.Neck, npc.DyeNeck);
			file.Wrists = GearFromItem(npc.Wrists, npc.DyeWrists);
			file.LeftRing = GearFromItem(npc.LeftRing, npc.DyeLeftRing);
			file.RightRing = GearFromItem(npc.RightRing, npc.DyeRightRing);

			return file;
		}

		private static CharacterFile.WeaponSave? WeaponFromItem(IItem? item, IDye? dye)
		{
			if (item == null)
				return null;

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

		private static CharacterFile.ItemSave? GearFromItem(IItem? item, IDye? dye)
		{
			if (item == null)
				return null;

			CharacterFile.ItemSave save = new CharacterFile.ItemSave();

			save.ModelBase = item.ModelBase;
			save.ModelVariant = (byte)item.ModelVariant;

			if (dye != null)
				save.DyeId = dye.Id;

			return save;
		}
	}
}
