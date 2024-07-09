// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using System;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("BNpcBase", 0x86278126)]
public class BattleNpc : ExcelRow, INpcBase
{
	private string? name;

	private ushort customizeRow;
	private ushort equipRow;
	private BattleNpcAppearance? appearance;

	public string Name => this.name ?? $"{this.TypeName} #{this.RowId}";
	public string Description { get; private set; } = string.Empty;
	public uint ModelCharaRow { get; private set; }

	public ImageReference? Icon => null;
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => this.name != null;
	public string TypeName => "Battle NPC";

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.name = GameDataService.GetNpcName(this);

		////Scale = parser.ReadColumn<float>(4);
		this.ModelCharaRow = (uint)parser.ReadColumn<ushort>(5);
		this.customizeRow = parser.ReadColumn<ushort>(6);
		this.equipRow = parser.ReadColumn<ushort>(7);
	}

	public INpcAppearance? GetAppearance()
	{
		if (this.appearance == null)
			this.appearance = new BattleNpcAppearance(this);

		return this.appearance;
	}

	public class BattleNpcAppearance : INpcAppearance
	{
		public BattleNpcAppearance(BattleNpc npc)
		{
			this.ModelCharaRow = npc.ModelCharaRow;

			Sheets.ExcelSheet<BNpcCustomize>? customizeSheet = GameDataService.GetSheet<BNpcCustomize>();
			BNpcCustomize? customize = customizeSheet?.GetOrDefault(npc.customizeRow);
			if (customize != null)
			{
				this.Race = GameDataService.Races.Get(Math.Max(customize.Race.Row, 1));
				this.Tribe = GameDataService.Tribes.Get(Math.Max(customize.Tribe.Row, 1));

				this.FacePaintColor = customize.FacePaintColor;
				this.FacePaint = customize.FacePaint;

				// These were flipped
				this.ExtraFeature1 = customize.ExtraFeature2OrBust;
				this.ExtraFeature2OrBust = customize.ExtraFeature1;

				this.Gender = customize.Gender;
				this.BodyType = customize.BodyType;
				this.Height = customize.Height;
				this.Face = customize.Face;
				this.HairStyle = customize.HairStyle;
				this.EnableHairHighlight = customize.HairHighlight > 1;
				this.SkinColor = customize.SkinColor;
				this.EyeHeterochromia = customize.EyeHeterochromia;
				this.HairHighlightColor = customize.HairHighlightColor;
				this.FacialFeature = customize.FacialFeature;
				this.FacialFeatureColor = customize.FacialFeatureColor;
				this.Eyebrows = customize.Eyebrows;
				this.EyeColor = customize.EyeColor;
				this.EyeShape = customize.EyeShape;
				this.Nose = customize.Nose;
				this.Jaw = customize.Jaw;
				this.Mouth = customize.Mouth;
				this.LipColor = customize.LipColor;
				this.BustOrTone1 = customize.BustOrTone1;
				this.HairColor = customize.HairColor;
			}

			Sheets.ExcelSheet<NpcEquip>? equipSheet = GameDataService.GetSheet<NpcEquip>();
			NpcEquip? npcEquip = equipSheet.Get(npc.equipRow);
			if (npcEquip != null)
			{
				this.MainHand = LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, npcEquip.ModelMainHand);
				this.DyeMainHand = GameDataService.Dyes.Get(npcEquip.DyeMainHand.Row);
				this.Dye2MainHand = GameDataService.Dyes.Get(npcEquip.Dye2MainHand.Row);
				this.OffHand = LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, npcEquip.ModelOffHand);
				this.DyeOffHand = GameDataService.Dyes.Get(npcEquip.DyeOffHand.Row);
				this.Dye2OffHand = GameDataService.Dyes.Get(npcEquip.Dye2OffHand.Row);
				this.Head = LuminaExtensions.GetGearItem(ItemSlots.Head, npcEquip.ModelHead);
				this.DyeHead = GameDataService.Dyes.Get(npcEquip.DyeHead.Row);
				this.Dye2Head = GameDataService.Dyes.Get(npcEquip.Dye2Head.Row);
				this.Body = LuminaExtensions.GetGearItem(ItemSlots.Body, npcEquip.ModelBody);
				this.DyeBody = GameDataService.Dyes.Get(npcEquip.DyeBody.Row);
				this.Dye2Body = GameDataService.Dyes.Get(npcEquip.Dye2Body.Row);
				this.Hands = LuminaExtensions.GetGearItem(ItemSlots.Hands, npcEquip.ModelHead);
				this.DyeHands = GameDataService.Dyes.Get(npcEquip.DyeHands.Row);
				this.Dye2Hands = GameDataService.Dyes.Get(npcEquip.Dye2Hands.Row);
				this.Legs = LuminaExtensions.GetGearItem(ItemSlots.Legs, npcEquip.ModelLegs);
				this.DyeLegs = GameDataService.Dyes.Get(npcEquip.DyeLegs.Row);
				this.Dye2Legs = GameDataService.Dyes.Get(npcEquip.Dye2Legs.Row);
				this.Feet = LuminaExtensions.GetGearItem(ItemSlots.Feet, npcEquip.ModelFeet);
				this.DyeFeet = GameDataService.Dyes.Get(npcEquip.DyeFeet.Row);
				this.Dye2Feet = GameDataService.Dyes.Get(npcEquip.Dye2Feet.Row);
				this.Ears = LuminaExtensions.GetGearItem(ItemSlots.Ears, npcEquip.ModelEars);
				this.DyeEars = GameDataService.Dyes.Get(npcEquip.DyeEars.Row);
				this.Dye2Ears = GameDataService.Dyes.Get(npcEquip.Dye2Ears.Row);
				this.Neck = LuminaExtensions.GetGearItem(ItemSlots.Neck, npcEquip.ModelNeck);
				this.DyeNeck = GameDataService.Dyes.Get(npcEquip.DyeNeck.Row);
				this.Dye2Neck = GameDataService.Dyes.Get(npcEquip.Dye2Neck.Row);
				this.Wrists = LuminaExtensions.GetGearItem(ItemSlots.Wrists, npcEquip.ModelWrists);
				this.DyeWrists = GameDataService.Dyes.Get(npcEquip.DyeWrists.Row);
				this.Dye2Wrists = GameDataService.Dyes.Get(npcEquip.Dye2Wrists.Row);
				this.LeftRing = LuminaExtensions.GetGearItem(ItemSlots.LeftRing, npcEquip.ModelLeftRing);
				this.DyeLeftRing = GameDataService.Dyes.Get(npcEquip.DyeLeftRing.Row);
				this.Dye2LeftRing = GameDataService.Dyes.Get(npcEquip.Dye2LeftRing.Row);
				this.RightRing = LuminaExtensions.GetGearItem(ItemSlots.RightRing, npcEquip.ModelRightRing);
				this.DyeRightRing = GameDataService.Dyes.Get(npcEquip.DyeRightRing.Row);
				this.Dye2RightRing = GameDataService.Dyes.Get(npcEquip.Dye2RightRing.Row);
			}
		}

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
		public IItem? Legs { get; private set; }
		public IDye? DyeLegs { get; private set; }
		public IDye? Dye2Legs { get; private set; }
		public IItem? Feet { get; private set; }
		public IDye? DyeFeet { get; private set; }
		public IDye? Dye2Feet { get; private set; }
		public IItem? Hands { get; private set; }
		public IDye? DyeHands { get; private set; }
		public IDye? Dye2Hands { get; private set; }
		public IItem? Wrists { get; private set; }
		public IDye? DyeWrists { get; private set; }
		public IDye? Dye2Wrists { get; private set; }
		public IItem? Neck { get; private set; }
		public IDye? DyeNeck { get; private set; }
		public IDye? Dye2Neck { get; private set; }
		public IItem? Ears { get; private set; }
		public IDye? DyeEars { get; private set; }
		public IDye? Dye2Ears { get; private set; }
		public IItem? LeftRing { get; private set; }
		public IDye? DyeLeftRing { get; private set; }
		public IDye? Dye2LeftRing { get; private set; }
		public IItem? RightRing { get; private set; }
		public IDye? DyeRightRing { get; private set; }
		public IDye? Dye2RightRing { get; private set; }
	}
}
