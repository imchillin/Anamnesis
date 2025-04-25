// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;

/// <summary>
/// Represents an event non-player character (NPC) in the game data.
/// </summary>
[Sheet("ENpcBase", 0x464052CD)]
public readonly unsafe struct EventNpc(ExcelPage page, uint offset, uint row)
	: IExcelRow<EventNpc>, INpcBase
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>Gets the singular name of the NPC.</summary>
	/// <remarks>
	/// If the NPC name is not available in the game data, a row-base type name will be returned.
	/// </remarks>
	public string Name
	{
		get
		{
			var npcName = GameDataService.GetNpcName(this);
			return !string.IsNullOrEmpty(npcName) ? npcName : $"{this.TypeName} #{this.RowId}";
		}
	}

	/// <summary>Gets the NPC's description.</summary>
	/// <remarks>
	/// Event NPCs do not have descriptions.
	/// </remarks>
	public string Description => string.Empty;

	/// <inheritdoc/>
	public uint ModelCharaRow => this.ModelChara.RowId;

	/// <summary>
	/// Gets a ModelChara reference object of the NPC's model from the game data.
	/// </summary>
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadUInt16(offset + 190), page.Language);

	/// <inheritdoc/>
	public ImgRef? Icon => null;

	/// <inheritdoc/>
	public Mod? Mod => null;

	/// <inheritdoc/>
	public bool CanFavorite => true;

	/// <inheritdoc/>
	public bool HasName => GameDataService.GetNpcName(this) != null;

	/// <inheritdoc/>
	public string TypeName => "Event NPC";

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<INpcBase>(this);
		set => FavoritesService.SetFavorite<INpcBase>(this, nameof(FavoritesService.Favorites.Models), value);
	}

	/// <summary>
	/// Gets a NpcEquip reference object of the NPC's equipment from the game data.
	/// </summary>
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

	/// <inheritdoc/>
	public readonly RowRef<Race> Race => new(page.Module, (uint)page.ReadUInt8(offset + 202), page.Language);

	/// <inheritdoc/>
	public readonly byte Gender => page.ReadUInt8(offset + 203);

	/// <inheritdoc/>
	public readonly byte BodyType => page.ReadUInt8(offset + 204);

	/// <inheritdoc/>
	public readonly byte Height => page.ReadUInt8(offset + 205);

	/// <inheritdoc/>
	public readonly RowRef<Tribe> Tribe => new(page.Module, (uint)page.ReadUInt8(offset + 206), page.Language);

	/// <inheritdoc/>
	public readonly byte Face => page.ReadUInt8(offset + 207);

	/// <inheritdoc/>
	public readonly byte HairStyle => page.ReadUInt8(offset + 208);

	/// <inheritdoc/>
	public readonly bool EnableHairHighlight => page.ReadUInt8(offset + 209) > 1;

	/// <inheritdoc/>
	public readonly byte SkinColor => page.ReadUInt8(offset + 210);

	/// <inheritdoc/>
	public readonly byte EyeHeterochromia => page.ReadUInt8(offset + 211);

	/// <inheritdoc/>
	public readonly byte HairColor => page.ReadUInt8(offset + 212);

	/// <inheritdoc/>
	public readonly byte HairHighlightColor => page.ReadUInt8(offset + 213);

	/// <inheritdoc/>
	public readonly byte FacialFeature => page.ReadUInt8(offset + 214);

	/// <inheritdoc/>
	public readonly byte FacialFeatureColor => page.ReadUInt8(offset + 215);

	/// <inheritdoc/>
	public readonly byte Eyebrows => page.ReadUInt8(offset + 216);

	/// <inheritdoc/>
	public readonly byte EyeColor => page.ReadUInt8(offset + 217);

	/// <inheritdoc/>
	public readonly byte EyeShape => page.ReadUInt8(offset + 218);

	/// <inheritdoc/>
	public readonly byte Nose => page.ReadUInt8(offset + 219);

	/// <inheritdoc/>
	public readonly byte Jaw => page.ReadUInt8(offset + 220);

	/// <inheritdoc/>
	public readonly byte Mouth => page.ReadUInt8(offset + 221);

	/// <inheritdoc/>
	public readonly byte LipColor => page.ReadUInt8(offset + 222);

	/// <inheritdoc/>
	public readonly byte BustOrTone1 => page.ReadUInt8(offset + 223);

	/// <inheritdoc/>
	public readonly byte ExtraFeature1 => page.ReadUInt8(offset + 224);

	/// <inheritdoc/>
	public readonly byte ExtraFeature2OrBust => page.ReadUInt8(offset + 225);

	/// <inheritdoc/>
	public readonly byte FacePaint => page.ReadUInt8(offset + 226);

	/// <inheritdoc/>
	public readonly byte FacePaintColor => page.ReadUInt8(offset + 227);

	/// <inheritdoc/>
	public readonly IItem MainHand => GetItem(ItemSlots.MainHand, (ulong)page.ReadUInt64(offset + 128), this.NpcEquip.Value.ModelMainHand);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeMainHand => new(page.Module, (uint)page.ReadUInt8(offset + 229), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2MainHand => new(page.Module, (uint)page.ReadUInt8(offset + 230), page.Language);

	/// <inheritdoc/>
	public readonly IItem OffHand => GetItem(ItemSlots.OffHand, (ulong)page.ReadUInt64(offset + 136), this.NpcEquip.Value.ModelOffHand);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeOffHand => new(page.Module, (uint)page.ReadUInt8(offset + 231), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2OffHand => new(page.Module, (uint)page.ReadUInt8(offset + 232), page.Language);

	/// <inheritdoc/>
	public readonly IItem Head => GetItem(ItemSlots.Head, (uint)page.ReadUInt32(offset + 148), this.NpcEquip.Value.ModelHead);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeHead => new(page.Module, (uint)page.ReadUInt8(offset + 233), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Head => new(page.Module, (uint)page.ReadUInt8(offset + 243), page.Language);

	/// <inheritdoc/>
	public readonly IItem Body => GetItem(ItemSlots.Body, (uint)page.ReadUInt32(offset + 152), this.NpcEquip.Value.ModelBody);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeBody => new(page.Module, (uint)page.ReadUInt8(offset + 234), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Body => new(page.Module, (uint)page.ReadUInt8(offset + 244), page.Language);

	/// <inheritdoc/>
	public readonly IItem Hands => LuminaExtensions.GetGearItem(ItemSlots.Hands, this.NpcEquip.ValueNullable?.ModelHands ?? 0);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeHands => new(page.Module, (uint)page.ReadUInt8(offset + 235), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Hands => new(page.Module, (uint)page.ReadUInt8(offset + 245), page.Language);

	/// <inheritdoc/>
	public readonly IItem Legs => GetItem(ItemSlots.Legs, (uint)page.ReadUInt32(offset + 160), this.NpcEquip.Value.ModelLegs);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeLegs => new(page.Module, (uint)page.ReadUInt8(offset + 236), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Legs => new(page.Module, (uint)page.ReadUInt8(offset + 246), page.Language);

	/// <inheritdoc/>
	public readonly IItem Feet => GetItem(ItemSlots.Feet, (uint)page.ReadUInt32(offset + 164), this.NpcEquip.Value.ModelFeet);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeFeet => new(page.Module, (uint)page.ReadUInt8(offset + 237), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Feet => new(page.Module, (uint)page.ReadUInt8(offset + 247), page.Language);

	/// <inheritdoc/>
	public readonly IItem Ears => GetItem(ItemSlots.Ears, (uint)page.ReadUInt32(offset + 168), this.NpcEquip.Value.ModelEars);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeEars => new(page.Module, (uint)page.ReadUInt8(offset + 238), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Ears => new(page.Module, (uint)page.ReadUInt8(offset + 248), page.Language);

	/// <inheritdoc/>
	public readonly IItem Neck => GetItem(ItemSlots.Neck, (uint)page.ReadUInt32(offset + 172), this.NpcEquip.Value.ModelNeck);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeNeck => new(page.Module, (uint)page.ReadUInt8(offset + 239), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Neck => new(page.Module, (uint)page.ReadUInt8(offset + 249), page.Language);

	/// <inheritdoc/>
	public readonly IItem Wrists => GetItem(ItemSlots.Wrists, (uint)page.ReadUInt32(offset + 176), this.NpcEquip.Value.ModelWrists);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeWrists => new(page.Module, (uint)page.ReadUInt8(offset + 240), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2Wrists => new(page.Module, (uint)page.ReadUInt8(offset + 250), page.Language);

	/// <inheritdoc/>
	public readonly IItem LeftRing => GetItem(ItemSlots.LeftRing, (uint)page.ReadUInt32(offset + 180), this.NpcEquip.Value.ModelLeftRing);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeLeftRing => new(page.Module, (uint)page.ReadUInt8(offset + 241), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> DyeRightRing => new(page.Module, (uint)page.ReadUInt8(offset + 242), page.Language);

	/// <inheritdoc/>
	public readonly IItem RightRing => GetItem(ItemSlots.RightRing, page.ReadUInt32(offset + 184), this.NpcEquip.Value.ModelRightRing);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2LeftRing => new(page.Module, (uint)page.ReadUInt8(offset + 251), page.Language);

	/// <inheritdoc/>
	public readonly RowRef<Stain> Dye2RightRing => new(page.Module, (uint)page.ReadUInt8(offset + 252), page.Language);

	/// <summary>
	/// Creates a new instance of the <see cref="EventNpc"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="EventNpc"/> struct.</returns>
	static EventNpc IExcelRow<EventNpc>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	/// <summary>
	/// Gets an item from the given slot and npc base and equipment model values.
	/// </summary>
	/// <param name="slot">The item slot.</param>
	/// <param name="baseVal">The base value of the item.</param>
	/// <param name="equipVal">The equipment model value of the item.</param>
	/// <returns>The found item in the game data.</returns>
	private static IItem GetItem(ItemSlots slot, uint baseVal, uint? equipVal)
	{
		if (equipVal != null && equipVal != 0 && equipVal != uint.MaxValue && equipVal != long.MaxValue)
			return LuminaExtensions.GetGearItem(slot, (uint)equipVal);

		return LuminaExtensions.GetGearItem(slot, baseVal);
	}

	/// <summary>
	/// Gets an item from the given slot and npc base and equipment model values.
	/// </summary>
	/// <param name="slot">The item slot.</param>
	/// <param name="baseVal">The base value of the item.</param>
	/// <param name="equipVal">The equipment model value of the item.</param>
	/// <returns>The found item in the game data.</returns>
	private static IItem GetItem(ItemSlots slot, ulong baseVal, ulong? equipVal)
	{
		return GetItem(slot, (uint)baseVal, (uint?)equipVal);
	}
}
