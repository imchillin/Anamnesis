// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Utilities
{
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;

	public class NpcNoneAppearance : INpcAppearance
	{
		public static NpcNoneAppearance NoneAppearance = new NpcNoneAppearance();

		public int FacePaintColor => 0;
		public int FacePaint => 0;
		public int ExtraFeature2OrBust => 0;
		public int ExtraFeature1 => 0;
		public Race? Race => GameDataService.Races.Get(1);
		public int Gender => 0;
		public int BodyType => 0;
		public int Height => 0;
		public Tribe? Tribe => GameDataService.Tribes.Get(1);
		public int Face => 0;
		public int HairStyle => 0;
		public bool EnableHairHighlight => false;
		public int SkinColor => 0;
		public int EyeHeterochromia => 0;
		public int HairHighlightColor => 0;
		public int FacialFeature => 0;
		public int FacialFeatureColor => 0;
		public int Eyebrows => 0;
		public int EyeColor => 0;
		public int EyeShape => 0;
		public int Nose => 0;
		public int Jaw => 0;
		public int Mouth => 0;
		public int LipColor => 0;
		public int BustOrTone1 => 0;
		public int HairColor => 0;

		public IItem MainHand => ItemUtility.NoneItem;
		public IDye DyeMainHand => DyeUtility.NoneDye;
		public IItem OffHand => ItemUtility.NoneItem;
		public IDye DyeOffHand => DyeUtility.NoneDye;
		public IItem Head => ItemUtility.NoneItem;
		public IDye DyeHead => DyeUtility.NoneDye;
		public IItem Body => ItemUtility.NoneItem;
		public IDye DyeBody => DyeUtility.NoneDye;
		public IItem Legs => ItemUtility.NoneItem;
		public IDye DyeLegs => DyeUtility.NoneDye;
		public IItem Feet => ItemUtility.NoneItem;
		public IDye DyeFeet => DyeUtility.NoneDye;
		public IItem Hands => ItemUtility.NoneItem;
		public IDye DyeHands => DyeUtility.NoneDye;
		public IItem Wrists => ItemUtility.NoneItem;
		public IDye DyeWrists => DyeUtility.NoneDye;
		public IItem Neck => ItemUtility.NoneItem;
		public IDye DyeNeck => DyeUtility.NoneDye;
		public IItem Ears => ItemUtility.NoneItem;
		public IDye DyeEars => DyeUtility.NoneDye;
		public IItem LeftRing => ItemUtility.NoneItem;
		public IDye DyeLeftRing => DyeUtility.NoneDye;
		public IItem RightRing => ItemUtility.NoneItem;
		public IDye DyeRightRing => DyeUtility.NoneDye;
	}
}
