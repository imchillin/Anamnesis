// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;

[Sheet("BNpcBase", 0xD5D82616)]
public readonly struct BattleNpc(ExcelPage page, uint offset, uint row)
	: IExcelRow<BattleNpc>, INpcBase
{
	public uint RowId => row;

	public string Name => GameDataService.GetNpcName(this) ?? $"{this.TypeName} #{this.RowId}";
	public string Description => string.Empty;
	public uint ModelCharaRow => this.ModelChara.RowId;
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadUInt16(offset + 10), page.Language);
	public readonly RowRef<BNpcCustomize> BNpcCustomize => new(page.Module, (uint)page.ReadUInt16(offset + 12), page.Language);
	public readonly RowRef<NpcEquip> NpcEquip => new(page.Module, (uint)page.ReadUInt16(offset + 14), page.Language);

	public ImgRef? Icon => null;
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => GameDataService.GetNpcName(this) != null;
	public string TypeName => "Battle NPC";

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

	// TODO: See if we can go back to using IAppearance to reduce code duplication
	public byte FacePaintColor => this.BNpcCustomize.Value.FacePaintColor;
	public byte FacePaint => this.BNpcCustomize.Value.FacePaint;
	public byte ExtraFeature2OrBust => this.BNpcCustomize.Value.ExtraFeature2OrBust;
	public byte ExtraFeature1 => this.BNpcCustomize.Value.ExtraFeature1;
	public RowRef<Race> Race => GameDataService.CreateRef<Race>(Math.Max(this.BNpcCustomize.Value.Race.Value.RowId, 1));
	public byte Gender => this.BNpcCustomize.Value.Gender;
	public byte BodyType => this.BNpcCustomize.Value.BodyType;
	public byte Height => this.BNpcCustomize.Value.Height;
	public RowRef<Tribe> Tribe => GameDataService.CreateRef<Tribe>(Math.Max(this.BNpcCustomize.Value.Tribe.Value.RowId, 1));
	public byte Face => this.BNpcCustomize.Value.Face;
	public byte HairStyle => this.BNpcCustomize.Value.HairStyle;
	public bool EnableHairHighlight => this.BNpcCustomize.Value.HairHighlight > 1;
	public byte SkinColor => this.BNpcCustomize.Value.SkinColor;
	public byte EyeHeterochromia => this.BNpcCustomize.Value.EyeHeterochromia;
	public byte HairHighlightColor => this.BNpcCustomize.Value.HairHighlightColor;
	public byte FacialFeature => this.BNpcCustomize.Value.FacialFeature;
	public byte FacialFeatureColor => this.BNpcCustomize.Value.FacialFeatureColor;
	public byte Eyebrows => this.BNpcCustomize.Value.Eyebrows;
	public byte EyeColor => this.BNpcCustomize.Value.EyeColor;
	public byte EyeShape => this.BNpcCustomize.Value.EyeShape;
	public byte Nose => this.BNpcCustomize.Value.Nose;
	public byte Jaw => this.BNpcCustomize.Value.Jaw;
	public byte Mouth => this.BNpcCustomize.Value.Mouth;
	public byte LipColor => this.BNpcCustomize.Value.LipColor;
	public byte BustOrTone1 => this.BNpcCustomize.Value.BustOrTone1;
	public byte HairColor => this.BNpcCustomize.Value.HairColor;

	public IItem MainHand => LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, this.NpcEquip.Value.ModelMainHand);
	public RowRef<Stain> DyeMainHand => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeMainHand.Value.RowId);
	public RowRef<Stain> Dye2MainHand => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2MainHand.Value.RowId);
	public IItem OffHand => LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, this.NpcEquip.Value.ModelOffHand);
	public RowRef<Stain> DyeOffHand => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeOffHand.Value.RowId);
	public RowRef<Stain> Dye2OffHand => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2OffHand.Value.RowId);
	public IItem Head => LuminaExtensions.GetGearItem(ItemSlots.Head, this.NpcEquip.Value.ModelHead);
	public RowRef<Stain> DyeHead => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeHead.Value.RowId);
	public RowRef<Stain> Dye2Head => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Head.Value.RowId);
	public IItem Body => LuminaExtensions.GetGearItem(ItemSlots.Body, this.NpcEquip.Value.ModelBody);
	public RowRef<Stain> DyeBody => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeBody.Value.RowId);
	public RowRef<Stain> Dye2Body => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Body.Value.RowId);
	public IItem Legs => LuminaExtensions.GetGearItem(ItemSlots.Legs, this.NpcEquip.Value.ModelLegs);
	public RowRef<Stain> DyeLegs => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeLegs.Value.RowId);
	public RowRef<Stain> Dye2Legs => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Legs.Value.RowId);
	public IItem Feet => LuminaExtensions.GetGearItem(ItemSlots.Feet, this.NpcEquip.Value.ModelFeet);
	public RowRef<Stain> DyeFeet => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeFeet.Value.RowId);
	public RowRef<Stain> Dye2Feet => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Feet.Value.RowId);
	public IItem Hands => LuminaExtensions.GetGearItem(ItemSlots.Hands, this.NpcEquip.Value.ModelHands);
	public RowRef<Stain> DyeHands => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeHands.Value.RowId);
	public RowRef<Stain> Dye2Hands => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Hands.Value.RowId);
	public IItem Wrists => LuminaExtensions.GetGearItem(ItemSlots.Wrists, this.NpcEquip.Value.ModelWrists);
	public RowRef<Stain> DyeWrists => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeWrists.Value.RowId);
	public RowRef<Stain> Dye2Wrists => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Wrists.Value.RowId);
	public IItem Neck => LuminaExtensions.GetGearItem(ItemSlots.Neck, this.NpcEquip.Value.ModelNeck);
	public RowRef<Stain> DyeNeck => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeNeck.Value.RowId);
	public RowRef<Stain> Dye2Neck => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Neck.Value.RowId);
	public IItem Ears => LuminaExtensions.GetGearItem(ItemSlots.Ears, this.NpcEquip.Value.ModelEars);
	public RowRef<Stain> DyeEars => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeEars.Value.RowId);
	public RowRef<Stain> Dye2Ears => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2Ears.Value.RowId);
	public IItem LeftRing => LuminaExtensions.GetGearItem(ItemSlots.LeftRing, this.NpcEquip.Value.ModelLeftRing);
	public RowRef<Stain> DyeLeftRing => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeLeftRing.Value.RowId);
	public RowRef<Stain> Dye2LeftRing => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2LeftRing.Value.RowId);
	public IItem RightRing => LuminaExtensions.GetGearItem(ItemSlots.RightRing, this.NpcEquip.Value.ModelRightRing);
	public RowRef<Stain> DyeRightRing => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.DyeRightRing.Value.RowId);
	public RowRef<Stain> Dye2RightRing => GameDataService.CreateRef<Stain>(this.NpcEquip.Value.Dye2RightRing.Value.RowId);

	static BattleNpc IExcelRow<BattleNpc>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
