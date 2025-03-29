// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;

[Sheet("ENpcBase", 0x464052CD)]
public readonly unsafe struct EventNpc(ExcelPage page, uint offset, uint row)
	: IExcelRow<EventNpc>, INpcBase
{
	public uint RowId => row;
	public string Name => GameDataService.GetNpcName(this) ?? $"{this.TypeName} #{this.RowId}";
	public string Description => string.Empty;
	public uint ModelCharaRow => this.ModelChara.RowId;
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadUInt16(offset + 190), page.Language);

	public ImageReference? Icon => null;
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => GameDataService.GetNpcName(this) != null;
	public string TypeName => "Event NPC";

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

	public readonly RowRef<NpcEquip> NpcEquip
	{
		get
		{
			var npcEquip = new RowRef<NpcEquip>(page.Module, (uint)page.ReadUInt16(offset + 192), page.Language);
			if (npcEquip.Value.RowId == 175)
				return default;
			return npcEquip;
		}
	}

	public readonly RowRef<Race> Race => new(page.Module, (uint)page.ReadUInt8(offset + 202), page.Language);
	public readonly byte Gender => page.ReadUInt8(offset + 203);
	public readonly byte BodyType => page.ReadUInt8(offset + 204);
	public readonly byte Height => page.ReadUInt8(offset + 205);
	public readonly RowRef<Tribe> Tribe => new(page.Module, (uint)page.ReadUInt8(offset + 206), page.Language);
	public readonly byte Face => page.ReadUInt8(offset + 207);
	public readonly byte HairStyle => page.ReadUInt8(offset + 208);
	public readonly bool EnableHairHighlight => page.ReadUInt8(offset + 209) > 1;
	public readonly byte SkinColor => page.ReadUInt8(offset + 210);
	public readonly byte EyeHeterochromia => page.ReadUInt8(offset + 211);
	public readonly byte HairColor => page.ReadUInt8(offset + 212);
	public readonly byte HairHighlightColor => page.ReadUInt8(offset + 213);
	public readonly byte FacialFeature => page.ReadUInt8(offset + 214);
	public readonly byte FacialFeatureColor => page.ReadUInt8(offset + 215);
	public readonly byte Eyebrows => page.ReadUInt8(offset + 216);
	public readonly byte EyeColor => page.ReadUInt8(offset + 217);
	public readonly byte EyeShape => page.ReadUInt8(offset + 218);
	public readonly byte Nose => page.ReadUInt8(offset + 219);
	public readonly byte Jaw => page.ReadUInt8(offset + 220);
	public readonly byte Mouth => page.ReadUInt8(offset + 221);
	public readonly byte LipColor => page.ReadUInt8(offset + 222);
	public readonly byte BustOrTone1 => page.ReadUInt8(offset + 223);
	public readonly byte ExtraFeature1 => page.ReadUInt8(offset + 224);
	public readonly byte ExtraFeature2OrBust => page.ReadUInt8(offset + 225);
	public readonly byte FacePaint => page.ReadUInt8(offset + 226);
	public readonly byte FacePaintColor => page.ReadUInt8(offset + 227);

	public readonly IItem MainHand => GetItem(ItemSlots.MainHand, (ulong)page.ReadUInt64(offset + 128), this.NpcEquip.Value.ModelMainHand);
	public readonly RowRef<Stain> DyeMainHand => new(page.Module, (uint)page.ReadUInt8(offset + 229), page.Language);
	public readonly RowRef<Stain> Dye2MainHand => new(page.Module, (uint)page.ReadUInt8(offset + 230), page.Language);

	public readonly IItem OffHand => GetItem(ItemSlots.OffHand, (ulong)page.ReadUInt64(offset + 136), this.NpcEquip.Value.ModelOffHand);
	public readonly RowRef<Stain> DyeOffHand => new(page.Module, (uint)page.ReadUInt8(offset + 231), page.Language);
	public readonly RowRef<Stain> Dye2OffHand => new(page.Module, (uint)page.ReadUInt8(offset + 232), page.Language);

	public readonly IItem Head => GetItem(ItemSlots.Head, (uint)page.ReadUInt32(offset + 148), this.NpcEquip.Value.ModelHead);
	public readonly RowRef<Stain> DyeHead => new(page.Module, (uint)page.ReadUInt8(offset + 233), page.Language);
	public readonly RowRef<Stain> Dye2Head => new(page.Module, (uint)page.ReadUInt8(offset + 243), page.Language);

	public readonly IItem Body => GetItem(ItemSlots.Body, (uint)page.ReadUInt32(offset + 152), this.NpcEquip.Value.ModelBody);
	public readonly RowRef<Stain> DyeBody => new(page.Module, (uint)page.ReadUInt8(offset + 234), page.Language);
	public readonly RowRef<Stain> Dye2Body => new(page.Module, (uint)page.ReadUInt8(offset + 244), page.Language);

	public readonly IItem Hands => GetItem(ItemSlots.Hands, (uint)page.ReadUInt32(offset + 156), this.NpcEquip.Value.ModelHands);
	public readonly RowRef<Stain> DyeHands => new(page.Module, (uint)page.ReadUInt8(offset + 235), page.Language);
	public readonly RowRef<Stain> Dye2Hands => new(page.Module, (uint)page.ReadUInt8(offset + 245), page.Language);

	public readonly IItem Legs => GetItem(ItemSlots.Legs, (uint)page.ReadUInt32(offset + 160), this.NpcEquip.Value.ModelLegs);
	public readonly RowRef<Stain> DyeLegs => new(page.Module, (uint)page.ReadUInt8(offset + 236), page.Language);
	public readonly RowRef<Stain> Dye2Legs => new(page.Module, (uint)page.ReadUInt8(offset + 246), page.Language);

	public readonly IItem Feet => GetItem(ItemSlots.Feet, (uint)page.ReadUInt32(offset + 164), this.NpcEquip.Value.ModelFeet);
	public readonly RowRef<Stain> DyeFeet => new(page.Module, (uint)page.ReadUInt8(offset + 237), page.Language);
	public readonly RowRef<Stain> Dye2Feet => new(page.Module, (uint)page.ReadUInt8(offset + 247), page.Language);

	public readonly IItem Ears => GetItem(ItemSlots.Ears, (uint)page.ReadUInt32(offset + 168), this.NpcEquip.Value.ModelEars);
	public readonly RowRef<Stain> DyeEars => new(page.Module, (uint)page.ReadUInt8(offset + 238), page.Language);
	public readonly RowRef<Stain> Dye2Ears => new(page.Module, (uint)page.ReadUInt8(offset + 248), page.Language);

	public readonly IItem Neck => GetItem(ItemSlots.Neck, (uint)page.ReadUInt32(offset + 172), this.NpcEquip.Value.ModelNeck);
	public readonly RowRef<Stain> DyeNeck => new(page.Module, (uint)page.ReadUInt8(offset + 239), page.Language);
	public readonly RowRef<Stain> Dye2Neck => new(page.Module, (uint)page.ReadUInt8(offset + 249), page.Language);

	public readonly IItem Wrists => GetItem(ItemSlots.Wrists, (uint)page.ReadUInt32(offset + 176), this.NpcEquip.Value.ModelWrists);
	public readonly RowRef<Stain> DyeWrists => new(page.Module, (uint)page.ReadUInt8(offset + 240), page.Language);
	public readonly RowRef<Stain> Dye2Wrists => new(page.Module, (uint)page.ReadUInt8(offset + 250), page.Language);

	public readonly IItem LeftRing => GetItem(ItemSlots.LeftRing, (uint)page.ReadUInt32(offset + 180), this.NpcEquip.Value.ModelLeftRing);
	public readonly RowRef<Stain> DyeLeftRing => new(page.Module, (uint)page.ReadUInt8(offset + 241), page.Language);
	public readonly RowRef<Stain> DyeRightRing => new(page.Module, (uint)page.ReadUInt8(offset + 242), page.Language);

	public readonly IItem RightRing => GetItem(ItemSlots.RightRing, page.ReadUInt32(offset + 184), this.NpcEquip.Value.ModelRightRing);
	public readonly RowRef<Stain> Dye2LeftRing => new(page.Module, (uint)page.ReadUInt8(offset + 251), page.Language);
	public readonly RowRef<Stain> Dye2RightRing => new(page.Module, (uint)page.ReadUInt8(offset + 252), page.Language);

	static EventNpc IExcelRow<EventNpc>.Create(ExcelPage page, uint offset, uint row) =>
	new(page, offset, row);

	private static IItem GetItem(ItemSlots slot, uint baseVal, uint? equipVal)
	{
		if (equipVal != null && equipVal != 0 && equipVal != uint.MaxValue && equipVal != long.MaxValue)
			return LuminaExtensions.GetGearItem(slot, (uint)equipVal);

		return LuminaExtensions.GetGearItem(slot, baseVal);
	}

	private static IItem GetItem(ItemSlots slot, ulong baseVal, ulong? equipVal)
	{
		return GetItem(slot, (uint)baseVal, (uint?)equipVal);
	}
}
