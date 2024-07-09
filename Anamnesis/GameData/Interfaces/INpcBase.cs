// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.TexTools;

public interface INpcBase : IRow
{
	uint ModelCharaRow { get; }

	ImageReference? Icon { get; }
	Mod? Mod { get; }
	bool IsFavorite { get; }
	bool CanFavorite { get; }
	bool HasName { get; }
	string TypeName { get; }

	INpcAppearance? GetAppearance();
}

public interface INpcAppearance
{
	uint ModelCharaRow { get; }

	int FacePaintColor { get; }
	int FacePaint { get; }
	int ExtraFeature2OrBust { get; }
	int ExtraFeature1 { get; }
	Race? Race { get; }
	int Gender { get; }
	int BodyType { get; }
	int Height { get; }
	Tribe? Tribe { get; }
	int Face { get; }
	int HairStyle { get; }
	bool EnableHairHighlight { get; }
	int SkinColor { get; }
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

	IItem? MainHand { get; }
	IDye? DyeMainHand { get; }
	IDye? Dye2MainHand { get; }
	IItem? OffHand { get; }
	IDye? DyeOffHand { get; }
	IDye? Dye2OffHand { get; }
	IItem? Head { get; }
	IDye? DyeHead { get; }
	IDye? Dye2Head { get; }
	IItem? Body { get; }
	IDye? DyeBody { get; }
	IDye? Dye2Body { get; }
	IItem? Legs { get; }
	IDye? DyeLegs { get; }
	IDye? Dye2Legs { get; }
	IItem? Feet { get; }
	IDye? DyeFeet { get; }
	IDye? Dye2Feet { get; }
	IItem? Hands { get; }
	IDye? DyeHands { get; }
	IDye? Dye2Hands { get; }
	IItem? Wrists { get; }
	IDye? DyeWrists { get; }
	IDye? Dye2Wrists { get; }
	IItem? Neck { get; }
	IDye? DyeNeck { get; }
	IDye? Dye2Neck { get; }
	IItem? Ears { get; }
	IDye? DyeEars { get; }
	IDye? Dye2Ears { get; }
	IItem? LeftRing { get; }
	IDye? DyeLeftRing { get; }
	IDye? Dye2LeftRing { get; }
	IItem? RightRing { get; }
	IDye? DyeRightRing { get; }
	IDye? Dye2RightRing { get; }
}
