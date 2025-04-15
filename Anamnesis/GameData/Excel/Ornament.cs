// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;

/// <summary>Represents an ornament object in the game data.</summary>
[Sheet("Ornament", 0x3D312C8F)]
public readonly struct Ornament(ExcelPage page, uint offset, uint row)
	: IExcelRow<Ornament>, INpcBase
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the singular name of the ornament.</summary>
	/// <remarks>
	/// If the ornament does not have a name, a default row-based type name is returned.
	/// </remarks>
	public readonly string Name => page.ReadString(offset, offset).ToString() ?? $"{this.TypeName} #{this.RowId}";

	/// <summary>Gets the description of the ornament.</summary>
	/// <remarks>
	/// Ornaments do not have descriptions.
	/// This is simply done to implement the <see cref="INpcBase"/> interface.
	/// </remarks>
	public readonly string Description => string.Empty;

	/// <inheritdoc/>
	public readonly uint ModelCharaRow => (uint)page.ReadUInt16(offset + 16);

	/// <summary>
	/// Gets the attachment point of the ornament to the character.
	/// </summary>
	public readonly byte AttachPoint => page.ReadUInt8(offset + 26);

	/// <inheritdoc/>
	public readonly ImgRef? Icon => new(page.ReadUInt16(offset + 20));

	/// <inheritdoc/>
	public readonly Mod? Mod => null;

	/// <inheritdoc/>
	public readonly bool CanFavorite => true;

	/// <inheritdoc/>
	public readonly bool HasName => page.ReadString(offset, offset).ToString() != null;

	/// <inheritdoc/>
	public readonly string TypeName => "Ornament";

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

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
	public IItem Head => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeHead => default;

	/// <inheritdoc/>
	public IItem Body => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeBody => default;

	/// <inheritdoc/>
	public IItem Legs => ItemUtility.NoneItem;

	/// <inheritdoc/>
	public RowRef<Stain> DyeLegs => default;

	/// <inheritdoc/>
	public IItem Feet => ItemUtility.NoneItem;

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

	/// <summary>
	/// Creates a new instance of the <see cref="Ornament"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Ornament"/> struct.</returns>
	static Ornament IExcelRow<Ornament>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
