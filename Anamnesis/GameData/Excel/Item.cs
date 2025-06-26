// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>Represents an item in the game data.</summary>
[Sheet("Item", 0xE9A33C9D)]
public readonly unsafe struct Item(ExcelPage page, uint offset, uint row)
	: IExcelRow<Item>, IItem
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <inheritdoc/>
	public readonly string Name => page.ReadString(offset + 12, offset).ToString() ?? string.Empty;

	/// <inheritdoc/>
	public string Description => page.ReadString(offset + 8, offset).ToString() ?? string.Empty;

	/// <inheritdoc/>
	public ImgRef Icon => new(page.ReadUInt16(offset + 136));

	/// <inheritdoc/>
	public readonly byte EquipLevel => page.ReadUInt8(offset + 78);

	/// <inheritdoc/>
	public ulong Model => page.ReadUInt64(offset + 24);

	/// <inheritdoc/>
	public ushort ModelSet => this.IsWeapon ? page.ReadWeaponSet(offset + 24) : (ushort)0;

	/// <inheritdoc/>
	public ushort ModelBase => this.IsWeapon ? page.ReadWeaponBase(offset + 24) : page.ReadBase(offset + 24);

	/// <inheritdoc/>
	public ushort ModelVariant => this.IsWeapon ? page.ReadWeaponVariant(offset + 24) : page.ReadVariant(offset + 24);

	/// <inheritdoc/>
	public bool HasSubModel => this.SubModelSet != 0;

	/// <inheritdoc/>
	public ulong SubModel => page.ReadUInt64(offset + 32);

	/// <inheritdoc/>
	public ushort SubModelSet => this.IsWeapon ? page.ReadWeaponSet(offset + 32) : (ushort)0;

	/// <inheritdoc/>
	public ushort SubModelBase => this.IsWeapon ? page.ReadWeaponBase(offset + 32) : page.ReadBase(offset + 32);

	/// <inheritdoc/>
	public ushort SubModelVariant => this.IsWeapon ? page.ReadWeaponVariant(offset + 32) : page.ReadVariant(offset + 32);

	/// <summary>Gets the classes that can equip the item.</summary>
	public Classes EquipableClasses => this.ClassJobCategory.Value.ToFlags();

	/// <summary>
	/// Gets the EquipSlotCategory reference object, representing the the equipment slot data associated with the item.
	/// </summary>
	public readonly RowRef<EquipSlotCategory> EquipSlotCategory => new(page.Module, (uint)page.ReadUInt8(offset + 154), page.Language);

	/// <summary>
	/// Gets the EquipRaceCategory reference object, representing the item's equip restrictions.
	/// </summary>
	public RowRef<EquipRaceCategory> EquipRestriction => new(page.Module, (uint)page.ReadUInt8(offset + 80), page.Language);

	/// <summary>
	/// Gets the ClassJobCategory reference object, representing the class job category the item belongs to.
	/// </summary>
	public readonly RowRef<ClassJobCategory> ClassJobCategory => new(page.Module, (uint)page.ReadUInt8(offset + 81), page.Language);

	/// <inheritdoc/>
	public bool IsWeapon => (this.GetItemSlots() & ItemSlots.Weapons) != 0;

	/// <inheritdoc/>
	public Mod? Mod => TexToolsService.GetMod(this);

	/// <inheritdoc/>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, value);
	}

	/// <inheritdoc/>
	public bool CanOwn => this.Category.HasFlagUnsafe(ItemCategories.Premium) || this.Category.HasFlagUnsafe(ItemCategories.Limited);

	/// <inheritdoc/>
	public bool IsOwned
	{
		get => FavoritesService.IsOwned(this);
		set => FavoritesService.SetOwned(this, value);
	}

	/// <summary>Gets the item category.</summary>
	public ItemCategories Category => GameDataService.GetCategory(this);

	/// <summary>
	/// Creates a new instance of the <see cref="Item"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Item"/> struct.</returns>
	static Item IExcelRow<Item>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	/// <summary>
	/// Checks if the item fits in the specified slot.
	/// </summary>
	/// <param name="slot">The slot to check.</param>
	/// <returns>True if the item fits in the slot; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot) => this.EquipSlotCategory.Value.Contains(slot);

	/// <summary>
	/// Gets the item slots that the item can be equipped in.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ItemSlots GetItemSlots() => this.EquipSlotCategory.Value.GetItemSlots();

	/// <summary>
	/// Converts the class job category to a <see cref="Classes"/> enum.
	/// </summary>
	/// <param name="self">The class job category.</param>
	/// <returns>A <see cref="Classes"/> enum.</returns>
	private static Classes ToFlags(ClassJobCategory self)
	{
		Classes classes = Classes.None;

		foreach (Classes? job in Enum.GetValues<Classes>().Select(v => (Classes?)v))
		{
			if (job == null || job == Classes.None || job == Classes.All)
				continue;

			if (self.Contains((Classes)job))
			{
				classes |= (Classes)job;
			}
		}

		return classes;
	}
}
