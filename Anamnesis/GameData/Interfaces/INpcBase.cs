// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.GameData;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.TexTools;
using Lumina.Excel;

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

	/// <summary>Gets the face paint.</summary>
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

	/// <summary>Gets a value indicating whether hair highlight is enabled.</summary>
	bool EnableHairHighlight { get; }

	/// <summary>Gets the skin color of the NPC.</summary>
	byte SkinColor { get; }

	/// <summary>Gets the eye heterochromia of the NPC.</summary>
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
