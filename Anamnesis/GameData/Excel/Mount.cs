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

/// <summary>Represents a mount in the game data.</summary>
[Sheet("Mount", 0x5B89058F)]
public readonly struct Mount(ExcelPage page, uint offset, uint row)
	: IExcelRow<Mount>, INpcBase
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>Gets the mount model identifier.</summary>
	public readonly int EquipHead => page.ReadInt32(offset + 32);

	/// <summary>Gets the mount body equipment slot.</summary>
	public readonly int EquipBody => page.ReadInt32(offset + 36);

	/// <summary>Gets the mount leg equipment slot.</summary>
	public readonly int EquipLeg => page.ReadInt32(offset + 40);

	/// <summary>Gets the mount foot equipment slot.</summary>
	public readonly int EquipFoot => page.ReadInt32(offset + 44);

	/// <summary>Returns the singular name of the mount.</summary>
	/// <remarks>
	/// If the name is not found, a row-based type name is returned.
	/// </remarks>
	public string Name => page.ReadString(offset, offset).ToString() ?? $"{this.TypeName} #{this.RowId}";

	/// <summary>Gets the description of the mount.</summary>
	/// <remarks>
	/// Mounts do not have descriptions in the game data.
	/// </remarks>
	public string Description => string.Empty;

	/// <summary>Gets the mount model row identifier.</summary>
	public uint ModelCharaRow => this.ModelChara.RowId;

	/// <summary>Gets the ModelChara object reference of the mount.</summary>
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadInt32(offset + 28), page.Language);

	/// <summary>Gets the mount customization row identifier</summary>
	public uint MountCustomizeRow => this.MountCustomize.RowId;

	/// <summary>Gets the mount customization data for player characters.</summary>
	public readonly RowRef<MountCustomize> MountCustomize => new(page.Module, (uint)page.ReadUInt8(offset + 70), page.Language);

	/// <inheritdoc/>
	public ImgRef? Icon => new(page.ReadUInt16(offset + 52));

	/// <inheritdoc/>
	public Mod? Mod => null;

	/// <inheritdoc/>
	public bool CanFavorite => true;

	/// <inheritdoc/>
	public bool HasName => page.ReadString(offset, offset).ToString() != null;

	/// <inheritdoc/>
	public string TypeName => "Mount";

	/// <inheritdoc/>
	public byte FacePaintColor => 0;

	/// <inheritdoc/>
	public byte FacePaint => 0;

	/// <inheritdoc/>
	public byte ExtraFeature2OrBust => 0;

	/// <inheritdoc/>
	public byte ExtraFeature1 => 0;

	/// <inheritdoc/>
	public RowRef<Race> Race => default;

	/// <inheritdoc/>
	public byte Gender => 0;

	/// <inheritdoc/>
	public byte BodyType => 0;

	/// <inheritdoc/>
	public byte Height => 0;

	/// <inheritdoc/>
	public RowRef<Tribe> Tribe => default;

	/// <inheritdoc/>
	public byte Face => 0;

	/// <inheritdoc/>
	public byte HairStyle => 0;

	/// <inheritdoc/>
	public bool EnableHairHighlight => false;

	/// <inheritdoc/>
	public byte SkinColor => 0;

	/// <inheritdoc/>
	public byte EyeHeterochromia => 0;

	/// <inheritdoc/>
	public byte HairHighlightColor => 0;

	/// <inheritdoc/>
	public byte FacialFeature => 0;

	/// <inheritdoc/>
	public byte FacialFeatureColor => 0;

	/// <inheritdoc/>
	public byte Eyebrows => 0;

	/// <inheritdoc/>
	public byte EyeColor => 0;

	/// <inheritdoc/>
	public byte EyeShape => 0;

	/// <inheritdoc/>
	public byte Nose => 0;

	/// <inheritdoc/>
	public byte Jaw => 0;

	/// <inheritdoc/>
	public byte Mouth => 0;

	/// <inheritdoc/>
	public byte LipColor => 0;

	/// <inheritdoc/>
	public byte BustOrTone1 => 0;

	/// <inheritdoc/>
	public byte HairColor => 0;

	/// <inheritdoc/>
	public IItem MainHand => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeMainHand => default;

	/// <inheritdoc/>
	public IItem OffHand => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeOffHand => default;

	/// <inheritdoc/>
	public IItem Head => LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)this.EquipHead);

	/// <inheritdoc/>
	public RowRef<Stain> DyeHead => default;

	/// <inheritdoc/>
	public IItem Body => LuminaExtensions.GetGearItem(ItemSlots.Body, (uint)this.EquipBody);

	/// <inheritdoc/>
	public RowRef<Stain> DyeBody => default;

	/// <inheritdoc/>
	public IItem Legs => LuminaExtensions.GetGearItem(ItemSlots.Legs, (uint)this.EquipLeg);

	/// <inheritdoc/>
	public RowRef<Stain> DyeLegs => default;

	/// <inheritdoc/>
	public IItem Feet => LuminaExtensions.GetGearItem(ItemSlots.Feet, (uint)this.EquipFoot);

	/// <inheritdoc/>
	public RowRef<Stain> DyeFeet => default;

	/// <inheritdoc/>
	public IItem Hands => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeHands => default;

	/// <inheritdoc/>
	public IItem Wrists => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeWrists => default;

	/// <inheritdoc/>
	public IItem Neck => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeNeck => default;

	/// <inheritdoc/>
	public IItem Ears => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeEars => default;

	/// <inheritdoc/>
	public IItem LeftRing => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeLeftRing => default;

	/// <inheritdoc/>
	public IItem RightRing => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeRightRing => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2MainHand => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2OffHand => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Head => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Body => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Legs => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Feet => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Hands => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Wrists => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Neck => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Ears => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2LeftRing => default;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2RightRing => default;

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Mount"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Mount"/> struct.</returns>
	static Mount IExcelRow<Mount>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
