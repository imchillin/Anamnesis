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

[Sheet("Mount", 0x5B89058F)]
public readonly struct Mount(ExcelPage page, uint offset, uint row)
	: IExcelRow<Mount>, INpcBase
{
	public uint RowId => row;

	public readonly int EquipHead => page.ReadInt32(offset + 32);
	public readonly int EquipBody => page.ReadInt32(offset + 36);
	public readonly int EquipLeg => page.ReadInt32(offset + 40);
	public readonly int EquipFoot => page.ReadInt32(offset + 44);

	public string Name => page.ReadString(offset, offset).ToString() ?? $"{this.TypeName} #{this.RowId}";
	public string Description => string.Empty;
	public uint ModelCharaRow => this.ModelChara.RowId;
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadInt32(offset + 28), page.Language);
	public uint MountCustomizeRow => this.MountCustomize.RowId;
	public readonly RowRef<MountCustomize> MountCustomize => new(page.Module, (uint)page.ReadUInt8(offset + 70), page.Language);

	public ImgRef? Icon => new(page.ReadUInt16(offset + 52));
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => page.ReadString(offset, offset).ToString() != null;
	public string TypeName => "Mount";

	public byte FacePaintColor => 0;
	public byte FacePaint => 0;
	public byte ExtraFeature2OrBust => 0;
	public byte ExtraFeature1 => 0;
	public RowRef<Race> Race => default;
	public byte Gender => 0;
	public byte BodyType => 0;
	public byte Height => 0;
	public RowRef<Tribe> Tribe => default;
	public byte Face => 0;
	public byte HairStyle => 0;
	public bool EnableHairHighlight => false;
	public byte SkinColor => 0;
	public byte EyeHeterochromia => 0;
	public byte HairHighlightColor => 0;
	public byte FacialFeature => 0;
	public byte FacialFeatureColor => 0;
	public byte Eyebrows => 0;
	public byte EyeColor => 0;
	public byte EyeShape => 0;
	public byte Nose => 0;
	public byte Jaw => 0;
	public byte Mouth => 0;
	public byte LipColor => 0;
	public byte BustOrTone1 => 0;
	public byte HairColor => 0;

	public IItem MainHand => ItemUtility.NoneItem;
	public RowRef<Stain> DyeMainHand => default;
	public IItem OffHand => ItemUtility.NoneItem;
	public RowRef<Stain> DyeOffHand => default;
	public IItem Head => LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)this.EquipHead);
	public RowRef<Stain> DyeHead => default;
	public IItem Body => LuminaExtensions.GetGearItem(ItemSlots.Body, (uint)this.EquipBody);
	public RowRef<Stain> DyeBody => default;
	public IItem Legs => LuminaExtensions.GetGearItem(ItemSlots.Legs, (uint)this.EquipLeg);
	public RowRef<Stain> DyeLegs => default;
	public IItem Feet => LuminaExtensions.GetGearItem(ItemSlots.Feet, (uint)this.EquipFoot);
	public RowRef<Stain> DyeFeet => default;
	public IItem Hands => ItemUtility.NoneItem;
	public RowRef<Stain> DyeHands => default;
	public IItem Wrists => ItemUtility.NoneItem;
	public RowRef<Stain> DyeWrists => default;
	public IItem Neck => ItemUtility.NoneItem;
	public RowRef<Stain> DyeNeck => default;
	public IItem Ears => ItemUtility.NoneItem;
	public RowRef<Stain> DyeEars => default;
	public IItem LeftRing => ItemUtility.NoneItem;
	public RowRef<Stain> DyeLeftRing => default;
	public IItem RightRing => ItemUtility.NoneItem;
	public RowRef<Stain> DyeRightRing => default;
	public RowRef<Stain> Dye2MainHand => default;
	public RowRef<Stain> Dye2OffHand => default;
	public RowRef<Stain> Dye2Head => default;
	public RowRef<Stain> Dye2Body => default;
	public RowRef<Stain> Dye2Legs => default;
	public RowRef<Stain> Dye2Feet => default;
	public RowRef<Stain> Dye2Hands => default;
	public RowRef<Stain> Dye2Wrists => default;
	public RowRef<Stain> Dye2Neck => default;
	public RowRef<Stain> Dye2Ears => default;
	public RowRef<Stain> Dye2LeftRing => default;
	public RowRef<Stain> Dye2RightRing => default;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

	static Mount IExcelRow<Mount>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
