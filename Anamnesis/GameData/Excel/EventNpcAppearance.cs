// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ENpcBase", 0x464052cd)]
public class EventNpcAppearance : ExcelRow, INpcAppearance
{
	public uint ModelCharaRow { get; private set; }

	public int FacePaintColor { get; private set; }
	public int FacePaint { get; private set; }
	public int ExtraFeature2OrBust { get; private set; }
	public int ExtraFeature1 { get; private set; }
	public Race? Race { get; private set; }
	public int Gender { get; private set; }
	public int BodyType { get; private set; }
	public int Height { get; private set; }
	public Tribe? Tribe { get; private set; }
	public int Face { get; private set; }
	public int HairStyle { get; private set; }
	public bool EnableHairHighlight { get; private set; }
	public int SkinColor { get; private set; }
	public int EyeHeterochromia { get; private set; }
	public int HairHighlightColor { get; private set; }
	public int FacialFeature { get; private set; }
	public int FacialFeatureColor { get; private set; }
	public int Eyebrows { get; private set; }
	public int EyeColor { get; private set; }
	public int EyeShape { get; private set; }
	public int Nose { get; private set; }
	public int Jaw { get; private set; }
	public int Mouth { get; private set; }
	public int LipColor { get; private set; }
	public int BustOrTone1 { get; private set; }
	public int HairColor { get; private set; }

	public IItem? MainHand { get; private set; }
	public IDye? DyeMainHand { get; private set; }
	public IDye? Dye2MainHand { get; private set; }
	public IItem? OffHand { get; private set; }
	public IDye? DyeOffHand { get; private set; }
	public IDye? Dye2OffHand { get; private set; }
	public IItem? Head { get; private set; }
	public IDye? DyeHead { get; private set; }
	public IDye? Dye2Head { get; private set; }
	public IItem? Body { get; private set; }
	public IDye? DyeBody { get; private set; }
	public IDye? Dye2Body { get; private set; }
	public IItem? Hands { get; private set; }
	public IDye? DyeHands { get; private set; }
	public IDye? Dye2Hands { get; private set; }
	public IItem? Legs { get; private set; }
	public IDye? DyeLegs { get; private set; }
	public IDye? Dye2Legs { get; private set; }
	public IItem? Feet { get; private set; }
	public IDye? DyeFeet { get; private set; }
	public IDye? Dye2Feet { get; private set; }
	public IItem? Ears { get; private set; }
	public IDye? DyeEars { get; private set; }
	public IDye? Dye2Ears { get; private set; }
	public IItem? Neck { get; private set; }
	public IDye? DyeNeck { get; private set; }
	public IDye? Dye2Neck { get; private set; }
	public IItem? Wrists { get; private set; }
	public IDye? DyeWrists { get; private set; }
	public IDye? Dye2Wrists { get; private set; }
	public IItem? LeftRing { get; private set; }
	public IDye? DyeLeftRing { get; private set; }
	public IDye? Dye2LeftRing { get; private set; }
	public IItem? RightRing { get; private set; }
	public IDye? DyeRightRing { get; private set; }
	public IDye? Dye2RightRing { get; private set; }

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.ModelCharaRow = (uint)parser.ReadColumn<ushort>(35);
		this.Race = parser.ReadRowReference<byte, Race>(36, 1);
		this.Gender = parser.ReadColumn<byte>(37);
		this.BodyType = parser.ReadColumn<byte>(38);
		this.Height = parser.ReadColumn<byte>(39);
		this.Tribe = parser.ReadRowReference<byte, Tribe>(40, 1);
		this.Face = parser.ReadColumn<byte>(41);
		this.HairStyle = parser.ReadColumn<byte>(42);
		this.EnableHairHighlight = parser.ReadColumn<byte>(43) > 1;
		this.SkinColor = parser.ReadColumn<byte>(44);
		this.EyeHeterochromia = parser.ReadColumn<byte>(45);
		this.HairColor = parser.ReadColumn<byte>(46);
		this.HairHighlightColor = parser.ReadColumn<byte>(47);
		this.FacialFeature = parser.ReadColumn<byte>(48);
		this.FacialFeatureColor = parser.ReadColumn<byte>(49);
		this.Eyebrows = parser.ReadColumn<byte>(50);
		this.EyeColor = parser.ReadColumn<byte>(51);
		this.EyeShape = parser.ReadColumn<byte>(52);
		this.Nose = parser.ReadColumn<byte>(53);
		this.Jaw = parser.ReadColumn<byte>(54);
		this.Mouth = parser.ReadColumn<byte>(55);
		this.LipColor = parser.ReadColumn<byte>(56);
		this.BustOrTone1 = parser.ReadColumn<byte>(57);

		// These were flipped
		this.ExtraFeature1 = parser.ReadColumn<byte>(59);
		this.ExtraFeature2OrBust = parser.ReadColumn<byte>(58);

		this.FacePaint = parser.ReadColumn<byte>(60);
		this.FacePaintColor = parser.ReadColumn<byte>(61);

		Lumina.Excel.GeneratedSheets.NpcEquip? npcEquip = parser.ReadRowReference<ushort, Lumina.Excel.GeneratedSheets.NpcEquip>(63);

		if (npcEquip?.RowId == 175)
			npcEquip = null;

		this.MainHand = LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, parser.ReadColumn<ulong>(65));
		this.DyeMainHand = parser.ReadRowReference<byte, Stain>(66);
		this.Dye2MainHand = parser.ReadRowReference<byte, Stain>(67);
		this.OffHand = LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, parser.ReadColumn<ulong>(68));
		this.DyeOffHand = parser.ReadRowReference<byte, Stain>(69);
		this.Dye2OffHand = parser.ReadRowReference<byte, Stain>(70);
		this.Head = this.GetItem(ItemSlots.Head, parser.ReadColumn<uint>(71), npcEquip?.ModelHead);
		this.DyeHead = parser.ReadRowReference<byte, Stain>(72);
		this.Dye2Head = parser.ReadRowReference<byte, Stain>(73);
		this.Body = this.GetItem(ItemSlots.Body, parser.ReadColumn<uint>(75), npcEquip?.ModelBody);
		this.DyeBody = parser.ReadRowReference<byte, Stain>(76);
		this.Dye2Body = parser.ReadRowReference<byte, Stain>(77);
		this.Hands = this.GetItem(ItemSlots.Hands, parser.ReadColumn<uint>(78), npcEquip?.ModelHands);
		this.DyeHands = parser.ReadRowReference<byte, Stain>(79);
		this.Dye2Hands = parser.ReadRowReference<byte, Stain>(80);
		this.Legs = this.GetItem(ItemSlots.Legs, parser.ReadColumn<uint>(81), npcEquip?.ModelLegs);
		this.DyeLegs = parser.ReadRowReference<byte, Stain>(82);
		this.Dye2Legs = parser.ReadRowReference<byte, Stain>(83);
		this.Feet = this.GetItem(ItemSlots.Feet, parser.ReadColumn<uint>(84), npcEquip?.ModelFeet);
		this.DyeFeet = parser.ReadRowReference<byte, Stain>(85);
		this.Dye2Feet = parser.ReadRowReference<byte, Stain>(86);
		this.Ears = this.GetItem(ItemSlots.Ears, parser.ReadColumn<uint>(87), npcEquip?.ModelEars);
		this.DyeEars = parser.ReadRowReference<byte, Stain>(88);
		this.Dye2Ears = parser.ReadRowReference<byte, Stain>(89);
		this.Neck = this.GetItem(ItemSlots.Neck, parser.ReadColumn<uint>(90), npcEquip?.ModelNeck);
		this.DyeNeck = parser.ReadRowReference<byte, Stain>(91);
		this.Dye2Neck = parser.ReadRowReference<byte, Stain>(92);
		this.Wrists = this.GetItem(ItemSlots.Wrists, parser.ReadColumn<uint>(93), npcEquip?.ModelWrists);
		this.DyeWrists = parser.ReadRowReference<byte, Stain>(94);
		this.Dye2Wrists = parser.ReadRowReference<byte, Stain>(95);
		this.LeftRing = this.GetItem(ItemSlots.LeftRing, parser.ReadColumn<uint>(96), npcEquip?.ModelLeftRing);
		this.DyeLeftRing = parser.ReadRowReference<byte, Stain>(97);
		this.Dye2LeftRing = parser.ReadRowReference<byte, Stain>(98);
		this.RightRing = this.GetItem(ItemSlots.RightRing, parser.ReadColumn<uint>(99), npcEquip?.ModelRightRing);
		this.DyeRightRing = parser.ReadRowReference<byte, Stain>(100);
		this.Dye2RightRing = parser.ReadRowReference<byte, Stain>(101);
	}

	protected IItem GetItem(ItemSlots slot, uint baseVal, uint? equipVal)
	{
		if (equipVal != null && equipVal != 0 && equipVal != uint.MaxValue && equipVal != long.MaxValue)
			return LuminaExtensions.GetGearItem(slot, (uint)equipVal);

		return LuminaExtensions.GetGearItem(slot, baseVal);
	}
}
