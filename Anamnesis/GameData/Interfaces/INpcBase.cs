// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	public interface INpcBase : IRow
	{
		int FacePaintColor { get; }
		int FacePaint { get; }
		int ExtraFeature2OrBust { get; }
		int ExtraFeature1 { get; }
		int ModelType { get; }
		IRace Race { get; }
		int Gender { get; }
		int BodyType { get; }
		int Height { get; }
		ITribe Tribe { get; }
		int Face { get; }
		int HairStyle { get; }
		int HairHighlight { get; }
		int SkinColor { get; }
		INpcEquip NpcEquip { get; }
		int EyeHeterochromia { get; }
		int HairHighlightColor { get; }
		int FacialFeature { get; }
		int FacialFeatureColor { get; }
		int Eyebrows { get; }
		int EyeColor { get; }
		int EyeShape { get; }
		int Nose { get; }
		int Jaw { get; }
		int Mouth { get; }
		int LipColor { get; }
		int BustOrTone1 { get; }
		int HairColor { get; }
	}

	public interface INpcEquip
	{
		IItem MainHand { get; }
		IDye DyeMainHand { get; }

		IItem OffHand { get; }
		IDye DyeOffHand { get; }

		IItem Head { get; }
		IDye DyeHead { get; }

		IItem Body { get; }
		IDye DyeBody { get; }

		IItem Legs { get; }
		IDye DyeLegs { get; }

		IItem Feet { get; }
		IDye DyeFeet { get; }

		IItem Hands { get; }
		IDye DyeHands { get; }

		IItem Wrists { get; }
		IDye DyeWrists { get; }

		IItem Neck { get; }
		IDye DyeNeck { get; }

		IItem Ears { get; }
		IDye DyeEars { get; }

		IItem LeftRing { get; }
		IDye DyeLeftRing { get; }

		IItem RightRing { get; }
		IDye DyeRightRing { get; }
	}
}
