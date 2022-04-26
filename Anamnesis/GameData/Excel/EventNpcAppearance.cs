// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ENpcBase", 0x927347d8)]
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
	public IItem? OffHand { get; private set; }
	public IDye? DyeOffHand { get; private set; }
	public IItem? Head { get; private set; }
	public IDye? DyeHead { get; private set; }
	public IItem? Body { get; private set; }
	public IDye? DyeBody { get; private set; }
	public IItem? Legs { get; private set; }
	public IDye? DyeLegs { get; private set; }
	public IItem? Feet { get; private set; }
	public IDye? DyeFeet { get; private set; }
	public IItem? Hands { get; private set; }
	public IDye? DyeHands { get; private set; }
	public IItem? Wrists { get; private set; }
	public IDye? DyeWrists { get; private set; }
	public IItem? Neck { get; private set; }
	public IDye? DyeNeck { get; private set; }
	public IItem? Ears { get; private set; }
	public IDye? DyeEars { get; private set; }
	public IItem? LeftRing { get; private set; }
	public IDye? DyeLeftRing { get; private set; }
	public IItem? RightRing { get; private set; }
	public IDye? DyeRightRing { get; private set; }

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
		this.OffHand = LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, parser.ReadColumn<ulong>(67));
		this.DyeOffHand = parser.ReadRowReference<byte, Stain>(68);
		this.Head = this.GetItem(ItemSlots.Head, parser.ReadColumn<uint>(69), npcEquip?.ModelHead);
		this.DyeHead = parser.ReadRowReference<byte, Stain>(70);
		this.Body = this.GetItem(ItemSlots.Body, parser.ReadColumn<uint>(72), npcEquip?.ModelBody);
		this.DyeBody = parser.ReadRowReference<byte, Stain>(73);
		this.Hands = this.GetItem(ItemSlots.Hands, parser.ReadColumn<uint>(74), npcEquip?.ModelHands);
		this.DyeHands = parser.ReadRowReference<byte, Stain>(75);
		this.Legs = this.GetItem(ItemSlots.Legs, parser.ReadColumn<uint>(76), npcEquip?.ModelLegs);
		this.DyeLegs = parser.ReadRowReference<byte, Stain>(77);
		this.Feet = this.GetItem(ItemSlots.Feet, parser.ReadColumn<uint>(78), npcEquip?.ModelFeet);
		this.DyeFeet = parser.ReadRowReference<byte, Stain>(79);
		this.Ears = this.GetItem(ItemSlots.Ears, parser.ReadColumn<uint>(80), npcEquip?.ModelEars);
		this.DyeEars = parser.ReadRowReference<byte, Stain>(81);
		this.Neck = this.GetItem(ItemSlots.Neck, parser.ReadColumn<uint>(82), npcEquip?.ModelNeck);
		this.DyeNeck = parser.ReadRowReference<byte, Stain>(83);
		this.Wrists = this.GetItem(ItemSlots.Wrists, parser.ReadColumn<uint>(84), npcEquip?.ModelWrists);
		this.DyeWrists = parser.ReadRowReference<byte, Stain>(85);
		this.LeftRing = this.GetItem(ItemSlots.LeftRing, parser.ReadColumn<uint>(86), npcEquip?.ModelLeftRing);
		this.DyeLeftRing = parser.ReadRowReference<byte, Stain>(87);
		this.RightRing = this.GetItem(ItemSlots.RightRing, parser.ReadColumn<uint>(88), npcEquip?.ModelRightRing);
		this.DyeRightRing = parser.ReadRowReference<byte, Stain>(89);
	}

	protected IItem GetItem(ItemSlots slot, uint baseVal, uint? equipVal)
	{
		if (equipVal != null && equipVal != 0 && equipVal != uint.MaxValue && equipVal != long.MaxValue)
			return LuminaExtensions.GetGearItem(slot, (uint)equipVal);

		return LuminaExtensions.GetGearItem(slot, baseVal);
	}
}
