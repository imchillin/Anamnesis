// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Memory;
	using Anamnesis.Services;

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
			else if (type == typeof(ModelListEntry))
			{
				t = 'F';
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
					'F' => GameDataService.ModelList.Get(key),
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

		private static CharacterFile ToFile(this INpcAppearance appearance)
		{
			if (appearance == null)
				throw new ArgumentNullException(nameof(appearance));

			ActorCustomizeMemory.Races? race = appearance.Race?.CustomizeRace;
			ActorCustomizeMemory.Tribes? tribe = appearance.Tribe?.CustomizeTribe;

			if (race == null)
				race = ActorCustomizeMemory.Races.Hyur;

			if (tribe == null)
				tribe = ActorCustomizeMemory.Tribes.Midlander;

			CharacterFile file = new CharacterFile();
			file.SaveMode = CharacterFile.SaveModes.All;
			file.ModelType = appearance.ModelCharaRow;
			file.Race = race;
			file.Tribe = tribe;
			file.Gender = (ActorCustomizeMemory.Genders)appearance.Gender;
			file.Age = (ActorCustomizeMemory.Ages)appearance.BodyType;
			file.Height = (byte)Math.Min(appearance.Height, 100);
			file.Head = (byte)appearance.Face;
			file.Hair = (byte)appearance.HairStyle;
			file.EnableHighlights = appearance.EnableHairHighlight;
			file.Skintone = (byte)appearance.SkinColor;

			// not sure anyone has -1 as an eye value, but juuust in case.
			if (appearance.EyeHeterochromia == -1)
			{
				file.REyeColor = (byte)appearance.EyeColor;
				file.LEyeColor = (byte)appearance.EyeColor;
			}
			else
			{
				file.REyeColor = (byte)appearance.EyeHeterochromia;
				file.LEyeColor = (byte)appearance.EyeColor;
			}

			file.HairTone = (byte)appearance.HairColor;
			file.Highlights = (byte)appearance.HairHighlightColor;
			file.FacialFeatures = (ActorCustomizeMemory.FacialFeature)appearance.FacialFeature;
			file.LimbalEyes = (byte)appearance.FacialFeatureColor;
			file.Eyebrows = (byte)appearance.Eyebrows;
			file.Eyes = (byte)appearance.EyeShape;
			file.Nose = (byte)appearance.Nose;
			file.Jaw = (byte)appearance.Jaw;
			file.Mouth = (byte)appearance.Mouth;
			file.LipsToneFurPattern = (byte)appearance.LipColor;

			if (appearance.Race?.CustomizeRace == ActorCustomizeMemory.Races.Miqote ||
				appearance.Race?.CustomizeRace == ActorCustomizeMemory.Races.AuRa ||
				appearance.Race?.CustomizeRace == ActorCustomizeMemory.Races.Viera ||
				appearance.Race?.CustomizeRace == ActorCustomizeMemory.Races.Hrothgar)
			{
				file.EarMuscleTailSize = (byte)appearance.ExtraFeature1;
				file.TailEarsType = (byte)appearance.ExtraFeature2OrBust;
				file.Bust = (byte)appearance.BustOrTone1;
			}
			else
			{
				file.Bust = (byte)appearance.ExtraFeature1;
				file.EarMuscleTailSize = (byte)appearance.BustOrTone1;
			}

			file.FacePaint = (byte)appearance.FacePaint;
			file.FacePaintColor = (byte)appearance.FacePaintColor;

			file.MainHand = WeaponFromItem(appearance.MainHand, appearance.DyeMainHand);
			file.OffHand = WeaponFromItem(appearance.OffHand, appearance.DyeOffHand);

			file.HeadGear = GearFromItem(appearance.Head, appearance.DyeHead);
			file.Body = GearFromItem(appearance.Body, appearance.DyeBody);
			file.Hands = GearFromItem(appearance.Hands, appearance.DyeHands);
			file.Legs = GearFromItem(appearance.Legs, appearance.DyeLegs);
			file.Feet = GearFromItem(appearance.Feet, appearance.DyeFeet);
			file.Ears = GearFromItem(appearance.Ears, appearance.DyeEars);
			file.Neck = GearFromItem(appearance.Neck, appearance.DyeNeck);
			file.Wrists = GearFromItem(appearance.Wrists, appearance.DyeWrists);
			file.LeftRing = GearFromItem(appearance.LeftRing, appearance.DyeLeftRing);
			file.RightRing = GearFromItem(appearance.RightRing, appearance.DyeRightRing);

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
