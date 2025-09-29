// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;

/// <summary>Represents a base battle non-player entity from the game data.</summary>
[Sheet("BNpcBase", 0xD5D82616)]
public readonly struct BattleNpc(ExcelPage page, uint offset, uint row)
	: IExcelRow<BattleNpc>, INpcBase
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>The singular name of the battle non-player entity.</summary>
	public string Name
	{
		get
		{
			var npcName = GameDataService.GetNpcName(this);
			return !string.IsNullOrEmpty(npcName) ? npcName : $"{this.TypeName} #{this.RowId}";
		}
	}

	/// <summary>Gets the description of the battle non-player entity.</summary>
	public string Description => string.Empty;

	/// <summary>Gets the <c>ModelChara</c> sheet row ID of the battle non-player entity.</summary>
	public uint ModelCharaRow => this.ModelChara.RowId;

	/// <summary>Gets the ModelChara object reference of the battle non-player entity.</summary>
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadUInt16(offset + 10), page.Language);

	/// <summary>Gets the <c>BNpcCustomize</c> object reference of the battle non-player entity.</summary>
	public readonly RowRef<BNpcCustomize> BNpcCustomize => new(page.Module, (uint)page.ReadUInt16(offset + 12), page.Language);

	/// <summary>Gets the <c>NpcEquip</c> object reference of the battle non-player entity.</summary>
	public readonly RowRef<NpcEquip> NpcEquip => new(page.Module, (uint)page.ReadUInt16(offset + 14), page.Language);

	/// <inheritdoc/>
	public ImgRef? Icon => null;

	/// <inheritdoc/>
	public Mod? Mod => null;

	/// <inheritdoc/>
	public bool CanFavorite => true;

	/// <inheritdoc/>
	public bool HasName => GameDataService.GetNpcName(this) != null;

	/// <inheritdoc/>
	public string TypeName => "Battle NPC";

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, value);
	}

	/// <inheritdoc/>
	public byte FacePaintColor => this.BNpcCustomize.Value.FacePaintColor;

	/// <inheritdoc/>
	public byte FacePaint => this.BNpcCustomize.Value.FacePaint;

	/// <inheritdoc/>
	public byte ExtraFeature2OrBust => this.BNpcCustomize.Value.ExtraFeature2OrBust;

	/// <inheritdoc/>
	public byte ExtraFeature1 => this.BNpcCustomize.Value.ExtraFeature1;

	/// <inheritdoc/>
	public RowRef<Race> Race => GameDataService.CreateRef<Race>(Math.Max(this.BNpcCustomize.Value.Race.Value.RowId, 1));

	/// <inheritdoc/>
	public byte Gender => this.BNpcCustomize.Value.Gender;

	/// <inheritdoc/>
	public byte BodyType => this.BNpcCustomize.Value.BodyType;

	/// <inheritdoc/>
	public byte Height => this.BNpcCustomize.Value.Height;

	/// <inheritdoc/>
	public RowRef<Tribe> Tribe => GameDataService.CreateRef<Tribe>(Math.Max(this.BNpcCustomize.Value.Tribe.Value.RowId, 1));

	/// <inheritdoc/>
	public byte Face => this.BNpcCustomize.Value.Face;

	/// <inheritdoc/>
	public byte HairStyle => this.BNpcCustomize.Value.HairStyle;

	/// <inheritdoc/>
	public bool EnableHairHighlight => this.BNpcCustomize.Value.HairHighlight > 1;

	/// <inheritdoc/>
	public byte SkinColor => this.BNpcCustomize.Value.SkinColor;

	/// <inheritdoc/>
	public byte EyeHeterochromia => this.BNpcCustomize.Value.EyeHeterochromia;

	/// <inheritdoc/>
	public byte HairHighlightColor => this.BNpcCustomize.Value.HairHighlightColor;

	/// <inheritdoc/>
	public byte FacialFeature => this.BNpcCustomize.Value.FacialFeature;

	/// <inheritdoc/>
	public byte FacialFeatureColor => this.BNpcCustomize.Value.FacialFeatureColor;

	/// <inheritdoc/>
	public byte Eyebrows => this.BNpcCustomize.Value.Eyebrows;

	/// <inheritdoc/>
	public byte EyeColor => this.BNpcCustomize.Value.EyeColor;

	/// <inheritdoc/>
	public byte EyeShape => this.BNpcCustomize.Value.EyeShape;

	/// <inheritdoc/>
	public byte Nose => this.BNpcCustomize.Value.Nose;

	/// <inheritdoc/>
	public byte Jaw => this.BNpcCustomize.Value.Jaw;

	/// <inheritdoc/>
	public byte Mouth => this.BNpcCustomize.Value.Mouth;

	/// <inheritdoc/>
	public byte LipColor => this.BNpcCustomize.Value.LipColor;

	/// <inheritdoc/>
	public byte BustOrTone1 => this.BNpcCustomize.Value.BustOrTone1;

	/// <inheritdoc/>
	public byte HairColor => this.BNpcCustomize.Value.HairColor;

	/// <inheritdoc/>
	public IItem MainHand => LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, this.NpcEquip.ValueNullable?.ModelMainHand ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeMainHand => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeMainHand.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2MainHand => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2MainHand.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem OffHand => LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, this.NpcEquip.ValueNullable?.ModelOffHand ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeOffHand => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeOffHand.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2OffHand => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2OffHand.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Head => LuminaExtensions.GetGearItem(ItemSlots.Head, this.NpcEquip.ValueNullable?.ModelHead ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeHead => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeHead.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Head => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Head.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Body => LuminaExtensions.GetGearItem(ItemSlots.Body, this.NpcEquip.ValueNullable?.ModelBody ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeBody => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeBody.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Body => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Body.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Legs => LuminaExtensions.GetGearItem(ItemSlots.Legs, this.NpcEquip.ValueNullable?.ModelLegs ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeLegs => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeLegs.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Legs => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Legs.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Feet => LuminaExtensions.GetGearItem(ItemSlots.Feet, this.NpcEquip.ValueNullable?.ModelFeet ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeFeet => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeFeet.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Feet => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Feet.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Hands => LuminaExtensions.GetGearItem(ItemSlots.Hands, this.NpcEquip.ValueNullable?.ModelHands ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeHands => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeHands.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Hands => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Hands.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Wrists => LuminaExtensions.GetGearItem(ItemSlots.Wrists, this.NpcEquip.ValueNullable?.ModelWrists ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeWrists => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeWrists.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Wrists => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Wrists.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Neck => LuminaExtensions.GetGearItem(ItemSlots.Neck, this.NpcEquip.ValueNullable?.ModelNeck ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeNeck => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeNeck.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Neck => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Neck.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem Ears => LuminaExtensions.GetGearItem(ItemSlots.Ears, this.NpcEquip.ValueNullable?.ModelEars ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeEars => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeEars.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Ears => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2Ears.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem LeftRing => LuminaExtensions.GetGearItem(ItemSlots.LeftRing, this.NpcEquip.ValueNullable?.ModelLeftRing ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeLeftRing => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeLeftRing.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2LeftRing => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2LeftRing.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public IItem RightRing => LuminaExtensions.GetGearItem(ItemSlots.RightRing, this.NpcEquip.ValueNullable?.ModelRightRing ?? 0);

	/// <inheritdoc/>
	public RowRef<Stain> DyeRightRing => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.DyeRightRing.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <inheritdoc/>
	public RowRef<Stain> Dye2RightRing => GameDataService.CreateRef<Stain>(this.NpcEquip.ValueNullable?.Dye2RightRing.Value.RowId ?? ItemUtility.NoneDye.RowId);

	/// <summary>
	/// Creates a new instance of the <see cref="BattleNpc"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="BattleNpc"/> struct.</returns>
	static BattleNpc IExcelRow<BattleNpc>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
