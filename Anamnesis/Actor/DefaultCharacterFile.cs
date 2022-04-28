// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using Anamnesis.Files;
using Anamnesis.Memory;

public static class DefaultCharacterFile
{
	public static CharacterFile Default = new CharacterFile()
	{
		SaveMode = CharacterFile.SaveModes.All,

		Race = ActorCustomizeMemory.Races.Hyur,
		Gender = ActorCustomizeMemory.Genders.Feminine,
		Age = ActorCustomizeMemory.Ages.Normal,
		Height = 50,
		Tribe = ActorCustomizeMemory.Tribes.Midlander,
		Head = 1,
		Hair = 1,
		EnableHighlights = false,
		Skintone = 0,
		REyeColor = 79,
		HairTone = 51,
		Highlights = 5,
		FacialFeatures = ActorCustomizeMemory.FacialFeature.None,
		LimbalEyes = 0,
		Eyebrows = 0,
		LEyeColor = 79,
		Eyes = 0,
		Nose = 0,
		Jaw = 0,
		Mouth = 0,
		LipsToneFurPattern = 170,
		EarMuscleTailSize = 50,
		TailEarsType = 0,
		Bust = 50,
		FacePaint = 0,
		FacePaintColor = 0,
		MainHand = new CharacterFile.WeaponSave()
		{
			Color = Color.Black,
			Scale = Vector.One,
			ModelSet = 301,
			ModelBase = 31,
			ModelVariant = 1,
			DyeId = 0,
		},
		OffHand = new CharacterFile.WeaponSave()
		{
			Color = Color.Black,
			Scale = Vector.One,
			ModelSet = 301,
			ModelBase = 31,
			ModelVariant = 1,
			DyeId = 0,
		},
		HeadGear = new CharacterFile.ItemSave()
		{
			ModelBase = 0,
			ModelVariant = 0,
			DyeId = 0,
		},
		Body = new CharacterFile.ItemSave()
		{
			ModelBase = 85,
			ModelVariant = 2,
			DyeId = 0,
		},
		Hands = new CharacterFile.ItemSave()
		{
			ModelBase = 85,
			ModelVariant = 2,
			DyeId = 0,
		},
		Legs = new CharacterFile.ItemSave()
		{
			ModelBase = 85,
			ModelVariant = 2,
			DyeId = 0,
		},
		Feet = new CharacterFile.ItemSave()
		{
			ModelBase = 85,
			ModelVariant = 2,
			DyeId = 0,
		},
		Ears = new CharacterFile.ItemSave()
		{
			ModelBase = 0,
			ModelVariant = 0,
			DyeId = 0,
		},
		Neck = new CharacterFile.ItemSave()
		{
			ModelBase = 0,
			ModelVariant = 0,
			DyeId = 0,
		},
		Wrists = new CharacterFile.ItemSave()
		{
			ModelBase = 0,
			ModelVariant = 0,
			DyeId = 0,
		},
		LeftRing = new CharacterFile.ItemSave()
		{
			ModelBase = 0,
			ModelVariant = 0,
			DyeId = 0,
		},
		RightRing = new CharacterFile.ItemSave()
		{
			ModelBase = 0,
			ModelVariant = 0,
			DyeId = 0,
		},
	};
}
