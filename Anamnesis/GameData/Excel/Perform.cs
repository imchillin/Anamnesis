// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;
using System.Runtime.CompilerServices;

/// <summary>Represents a Performance action in the game data.</summary>
[Sheet("Perform", 0x7BF81FA9)]
public readonly struct Perform(ExcelPage page, uint offset, uint row)
	: IExcelRow<Perform>, IItem
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <inheritdoc/>
	public readonly string Name => page.ReadString(offset + 16, offset).ToString() ?? string.Empty;

	/// <inheritdoc/>
	public string Description => string.Empty;

	public ulong Model => page.ReadUInt64(offset + 8);

	/// <inheritdoc/>
	public ushort ModelSet => page.ReadWeaponSet(offset + 8);

	/// <inheritdoc/>
	public ushort ModelBase => page.ReadWeaponBase(offset + 8);

	/// <inheritdoc/>
	public ushort ModelVariant => page.ReadWeaponVariant(offset + 8);

	/// <inheritdoc/>
	public Mod? Mod => TexToolsService.GetMod(this);

	/// <inheritdoc/>
	public ImgRef? Icon => null;

	/// <inheritdoc/>
	public bool HasSubModel => false;

	public ulong SubModel => 0;

	/// <inheritdoc/>
	public ushort SubModelSet => 0;

	/// <inheritdoc/>
	public ushort SubModelBase => 0;

	/// <inheritdoc/>
	public ushort SubModelVariant => 0;

	/// <inheritdoc/>
	public Classes EquipableClasses => Classes.All;

	/// <inheritdoc/>
	public bool IsWeapon => true;

	/// <inheritdoc/>
	public byte EquipLevel => 0;

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
	}

	/// <inheritdoc/>
	public bool CanOwn => false;

	/// <inheritdoc/>
	public bool IsOwned
	{
		get => false;
		set { } // Interface requires a setter but this is a read-only struct
	}

	/// <inheritdoc/>
	public ItemCategories Category => ItemCategories.Performance;

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
