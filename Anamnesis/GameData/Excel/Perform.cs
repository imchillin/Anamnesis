// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;
using System.Runtime.CompilerServices;

/// <summary>Represents a performance instrument (e.g. bard instruments) in the game data.</summary>
[Sheet("Perform", 0x7BF81FA9)]
public readonly struct Perform(ExcelPage page, uint offset, uint row)
	: IExcelRow<Perform>, IItem
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <inheritdoc/>
	public readonly string Name => page.ReadString(offset + 16, offset).ToString() ?? string.Empty;

	/// <inheritdoc/>
	public readonly string Description => string.Empty;

	/// <inheritdoc/>
	public readonly ulong Model => page.ReadUInt64(offset + 8);

	/// <inheritdoc/>
	public readonly ushort ModelSet => page.ReadWeaponSet(offset + 8);

	/// <inheritdoc/>
	public readonly ushort ModelBase => page.ReadWeaponBase(offset + 8);

	/// <inheritdoc/>
	public readonly ushort ModelVariant => page.ReadWeaponVariant(offset + 8);

	/// <inheritdoc/>
	public readonly Mod? Mod => TexToolsService.GetMod(this);

	/// <inheritdoc/>
	public readonly ImgRef? Icon => null;

	/// <inheritdoc/>
	public readonly bool HasSubModel => false;

	/// <inheritdoc/>
	public readonly ulong SubModel => 0;

	/// <inheritdoc/>
	public readonly ushort SubModelSet => 0;

	/// <inheritdoc/>
	public readonly ushort SubModelBase => 0;

	/// <inheritdoc/>
	public readonly ushort SubModelVariant => 0;

	/// <inheritdoc/>
	public readonly Classes EquipableClasses => Classes.All;

	/// <inheritdoc/>
	public readonly bool IsWeapon => true;

	/// <inheritdoc/>
	public readonly byte EquipLevel => 0;

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
	}

	/// <inheritdoc/>
	public readonly bool CanOwn => false;

	/// <inheritdoc/>
	public readonly bool IsOwned
	{
		get => false;
		set { } // Interface requires a setter but this is a read-only struct
	}

	/// <inheritdoc/>
	public readonly ItemCategories Category => ItemCategories.Performance;

	/// <inheritdoc/>
	public ItemFavoriteCategory FavoriteItemCategory => ItemFavoriteCategory.Perform;

	/// <summary>
	/// Creates a new instance of the <see cref="Perform"/> struct.
	/// </summary>
	/// <param name="page">The Excel page containing the data.</param>
	/// <param name="offset">The offset within the page where the data starts.</param>
	/// <param name="row">The row ID of the data.</param>
	/// <returns>A new instance of the <see cref="Perform"/> struct.</returns>
	static Perform IExcelRow<Perform>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot) => slot == ItemSlots.MainHand;
}
