// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;
using Lumina.Excel.Sheets;

/// <summary>Represents companion data (e.g. minions) in the game data.</summary>
[Sheet("Companion", 0xEAEEBEF0)]
public readonly struct Companion(ExcelPage page, uint offset, uint row)
	: IExcelRow<Companion>, INpcBase
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>Gets the singular name of the companion.</summary>
	/// <remarks>
	/// If the name is not available, a row-based type name will be returned.
	/// </remarks>
	public string Name
	{
		get
		{
			var name = page.ReadString(offset, offset).ToString();
			return !string.IsNullOrEmpty(name) ? name : $"{this.TypeName} #{this.RowId}";
		}
	}

	/// <summary>
	/// Gets the minion's description.
	/// </summary>
	/// <remarks>
	/// Not available in this Excel sheet. For minion descriptions, refer to the CompanionTransient sheet.
	/// </remarks>
	public string Description => string.Empty;

	/// <inheritdoc/>
	public uint ModelCharaRow => this.Model.RowId;

	/// <summary>
	/// Gets the ModelChara reference object for the companion's model data.
	/// </summary>
	public readonly RowRef<ModelChara> Model => new(page.Module, (uint)page.ReadUInt16(offset + 16), page.Language);

	/// <inheritdoc/>
	public ImgRef? Icon => new(page.ReadUInt16(offset + 22));

	/// <inheritdoc/>
	public Mod? Mod => null;

	/// <inheritdoc/>
	public bool CanFavorite => true;

	/// <inheritdoc/>
	public bool HasName => page.ReadString(offset, offset).ToString() != null;

	/// <inheritdoc/>
	public string TypeName => "Minion";

	/// <inheritdoc/>
	public byte FacePaintColor => 0;

	/// <inheritdoc/>
	public byte FacePaint => 0;

	/// <inheritdoc/>
	public byte ExtraFeature2OrBust => 0;

	/// <inheritdoc/>
	public byte ExtraFeature1 => 0;

	/// <inheritdoc/>
	public RowRef<Race> Race => GameDataService.CreateRef<Race>(1);

	/// <inheritdoc/>
	public byte Gender => 0;

	/// <inheritdoc/>
	public byte BodyType => 0;

	/// <inheritdoc/>
	public byte Height => 0;

	/// <inheritdoc/>
	public RowRef<Tribe> Tribe => GameDataService.CreateRef<Tribe>(1);

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

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, value);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Companion"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Companion"/> struct.</returns>
	static Companion IExcelRow<Companion>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
