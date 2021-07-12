// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.Memory;

	public static class INpcBaseExtensions
	{
		public static CharacterFile ToFile(this INpcBase npc)
		{
			CharacterFile file = new CharacterFile();
			file.SaveMode = CharacterFile.SaveModes.All;
			file.ModelType = npc.ModelType;
			file.Race = npc.Race.Race;
			file.Tribe = npc.Tribe.Tribe;
			file.Gender = (Customize.Genders)npc.Gender;
			file.Age = (Customize.Ages)npc.BodyType;
			file.Height = (byte)npc.Height;
			file.Head = (byte)npc.Face;
			file.Hair = (byte)npc.HairStyle;
			file.EnableHighlights = npc.HairHighlightColor > 1;
			file.Skintone = (byte)npc.SkinColor;
			file.REyeColor = (byte)npc.EyeColor;
			file.LEyeColor = (byte)npc.EyeHeterochromia;
			file.HairTone = (byte)npc.HairColor;
			file.Highlights = (byte)npc.HairHighlightColor;
			file.FacialFeatures = (Customize.FacialFeature)npc.FacialFeature;
			file.LimbalEyes = 0; // TODO: Can npc's have limbal rings?
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
