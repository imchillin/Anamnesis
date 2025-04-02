// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;

/// <summary>
/// Represents a resident non-playable character (NPC) in the game data.
/// </summary>
[Sheet("ENpcResident", 0xF74FA88C)]
public readonly struct ResidentNpc(ExcelPage page, uint offset, uint row)
	: IExcelRow<ResidentNpc>, INpcBase
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets a reference to the associated Event NPC.</summary>
	public readonly RowRef<EventNpc> EventNpc => GameDataService.CreateRef<EventNpc>(this.RowId);

	/// <summary>Gets the name of the Resident NPC.</summary>
	public readonly string Name => page.ReadString(offset, offset).ToString() ?? $"{this.TypeName} #{this.RowId}";

	/// <summary>Gets the description of the Resident NPC.</summary>
	/// <remarks>This contains the resident NPC's title.</remarks>
	public readonly string Description => page.ReadString(offset + 8, offset).ToString() ?? string.Empty;

	/// <inheritdoc/>
	public readonly uint ModelCharaRow => this.EventNpc.Value.ModelCharaRow;

	/// <inheritdoc/>
	public readonly ImgRef? Icon => null;

	/// <inheritdoc/>
	public readonly Mod? Mod => null;

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

	/// <inheritdoc/>
	public readonly bool CanFavorite => true;

	/// <inheritdoc/>
	public readonly bool HasName => !page.ReadString(offset, offset).IsEmpty;

	/// <inheritdoc/>
	public readonly string TypeName => "Resident NPC";

	/// <inheritdoc/>
	public byte FacePaintColor => this.EventNpc.Value.FacePaintColor;

	/// <inheritdoc/>
	public byte FacePaint => this.EventNpc.Value.FacePaint;

	/// <inheritdoc/>
	public byte ExtraFeature2OrBust => this.EventNpc.Value.ExtraFeature2OrBust;

	/// <inheritdoc/>
	public byte ExtraFeature1 => this.EventNpc.Value.ExtraFeature1;

	/// <inheritdoc/>
	public RowRef<Race> Race => this.EventNpc.Value.Race;

	/// <inheritdoc/>
	public byte Gender => this.EventNpc.Value.Gender;

	/// <inheritdoc/>
	public byte BodyType => this.EventNpc.Value.BodyType;

	/// <inheritdoc/>
	public byte Height => this.EventNpc.Value.Height;

	/// <inheritdoc/>
	public RowRef<Tribe> Tribe => this.EventNpc.Value.Tribe;

	/// <inheritdoc/>
	public byte Face => this.EventNpc.Value.Face;

	/// <inheritdoc/>
	public byte HairStyle => this.EventNpc.Value.HairStyle;

	/// <inheritdoc/>
	public bool EnableHairHighlight => this.EventNpc.Value.EnableHairHighlight;

	/// <inheritdoc/>
	public byte SkinColor => this.EventNpc.Value.SkinColor;

	/// <inheritdoc/>
	public byte EyeHeterochromia => this.EventNpc.Value.EyeHeterochromia;

	/// <inheritdoc/>
	public byte HairHighlightColor => this.EventNpc.Value.HairHighlightColor;

	/// <inheritdoc/>
	public byte FacialFeature => this.EventNpc.Value.FacialFeature;

	/// <inheritdoc/>
	public byte FacialFeatureColor => this.EventNpc.Value.FacialFeatureColor;

	/// <inheritdoc/>
	public byte Eyebrows => this.EventNpc.Value.Eyebrows;

	/// <inheritdoc/>
	public byte EyeColor => this.EventNpc.Value.EyeColor;

	/// <inheritdoc/>
	public byte EyeShape => this.EventNpc.Value.EyeShape;

	/// <inheritdoc/>
	public byte Nose => this.EventNpc.Value.Nose;

	/// <inheritdoc/>
	public byte Jaw => this.EventNpc.Value.Jaw;

	/// <inheritdoc/>
	public byte Mouth => this.EventNpc.Value.Mouth;

	/// <inheritdoc/>
	public byte LipColor => this.EventNpc.Value.LipColor;

	/// <inheritdoc/>
	public byte BustOrTone1 => this.EventNpc.Value.BustOrTone1;

	/// <inheritdoc/>
	public byte HairColor => this.EventNpc.Value.HairColor;

	/// <inheritdoc/>
	public IItem MainHand => this.EventNpc.Value.MainHand;

	/// <inheritdoc/>
	public RowRef<Stain> DyeMainHand => this.EventNpc.Value.DyeMainHand;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2MainHand => this.EventNpc.Value.Dye2MainHand;

	/// <inheritdoc/>
	public IItem OffHand => this.EventNpc.Value.OffHand;

	/// <inheritdoc/>
	public RowRef<Stain> DyeOffHand => this.EventNpc.Value.DyeOffHand;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2OffHand => this.EventNpc.Value.Dye2OffHand;

	/// <inheritdoc/>
	public IItem Head => this.EventNpc.Value.Head;

	/// <inheritdoc/>
	public RowRef<Stain> DyeHead => this.EventNpc.Value.DyeHead;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Head => this.EventNpc.Value.Dye2Head;

	/// <inheritdoc/>
	public IItem Body => this.EventNpc.Value.Body;

	/// <inheritdoc/>
	public RowRef<Stain> DyeBody => this.EventNpc.Value.DyeBody;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Body => this.EventNpc.Value.Dye2Body;

	/// <inheritdoc/>
	public IItem Legs => this.EventNpc.Value.Legs;

	/// <inheritdoc/>
	public RowRef<Stain> DyeLegs => this.EventNpc.Value.DyeLegs;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Legs => this.EventNpc.Value.Dye2Legs;

	/// <inheritdoc/>
	public IItem Feet => this.EventNpc.Value.Feet;

	/// <inheritdoc/>
	public RowRef<Stain> DyeFeet => this.EventNpc.Value.DyeFeet;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Feet => this.EventNpc.Value.Dye2Feet;

	/// <inheritdoc/>
	public IItem Hands => this.EventNpc.Value.Hands;

	/// <inheritdoc/>
	public RowRef<Stain> DyeHands => this.EventNpc.Value.DyeHands;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Hands => this.EventNpc.Value.Dye2Hands;

	/// <inheritdoc/>
	public IItem Wrists => this.EventNpc.Value.Wrists;

	/// <inheritdoc/>
	public RowRef<Stain> DyeWrists => this.EventNpc.Value.DyeWrists;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Wrists => this.EventNpc.Value.Dye2Wrists;

	/// <inheritdoc/>
	public IItem Neck => this.EventNpc.Value.Neck;

	/// <inheritdoc/>
	public RowRef<Stain> DyeNeck => this.EventNpc.Value.DyeNeck;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Neck => this.EventNpc.Value.Dye2Neck;

	/// <inheritdoc/>
	public IItem Ears => this.EventNpc.Value.Ears;

	/// <inheritdoc/>
	public RowRef<Stain> DyeEars => this.EventNpc.Value.DyeEars;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2Ears => this.EventNpc.Value.Dye2Ears;

	/// <inheritdoc/>
	public IItem LeftRing => this.EventNpc.Value.LeftRing;

	/// <inheritdoc/>
	public RowRef<Stain> DyeLeftRing => this.EventNpc.Value.DyeLeftRing;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2LeftRing => this.EventNpc.Value.Dye2LeftRing;

	/// <inheritdoc/>
	public IItem RightRing => this.EventNpc.Value.RightRing;

	/// <inheritdoc/>
	public RowRef<Stain> DyeRightRing => this.EventNpc.Value.DyeRightRing;

	/// <inheritdoc/>
	public RowRef<Stain> Dye2RightRing => this.EventNpc.Value.Dye2RightRing;

	/// <summary>
	/// Creates a new instance of the <see cref="ResidentNpc"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="ResidentNpc"/> struct.</returns>
	static ResidentNpc IExcelRow<ResidentNpc>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
