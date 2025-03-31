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

[Sheet("Item", 0xE9A33C9D)]
public readonly unsafe struct Item(ExcelPage page, uint offset, uint row)
	: IExcelRow<Item>, IItem
{
	public uint RowId => row;

	public readonly string Name => page.ReadString(offset + 12, offset).ToString() ?? string.Empty;
	public string Description => page.ReadString(offset + 8, offset).ToString() ?? string.Empty;
	public ImgRef Icon => new(page.ReadUInt16(offset + 136));
	public readonly byte EquipLevel => page.ReadUInt8(offset + 78);

	public ulong Model => page.ReadUInt64(offset + 24);
	public ushort ModelSet => this.IsWeapon ? page.ReadWeaponSet(offset + 24) : page.ReadSet(offset + 24);
	public ushort ModelBase => this.IsWeapon ? page.ReadWeaponBase(offset + 24) : page.ReadBase(offset + 24);
	public ushort ModelVariant => this.IsWeapon ? page.ReadWeaponVariant(offset + 24) : page.ReadVariant(offset + 24);
	public ulong SubModel => page.ReadUInt64(offset + 32);
	public ushort SubModelSet => this.IsWeapon ? page.ReadWeaponSet(offset + 32) : page.ReadSet(offset + 32);
	public ushort SubModelBase => this.IsWeapon ? page.ReadWeaponBase(offset + 32) : page.ReadBase(offset + 32);
	public ushort SubModelVariant => this.IsWeapon ? page.ReadWeaponVariant(offset + 32) : page.ReadVariant(offset + 32);
	public Classes EquipableClasses => this.ClassJobCategory.Value.ToFlags();
	public readonly RowRef<EquipSlotCategory> EquipSlotCategory => new(page.Module, (uint)page.ReadUInt8(offset + 154), page.Language);
	public RowRef<EquipRaceCategory> EquipRestriction => new(page.Module, (uint)page.ReadUInt8(offset + 80), page.Language);
	public readonly RowRef<ClassJobCategory> ClassJobCategory => new(page.Module, (uint)page.ReadUInt8(offset + 81), page.Language);

	public bool IsWeapon => (this.GetItemSlots() & ItemSlots.Weapons) != 0;
	public bool HasSubModel => this.SubModelSet != 0;

	public Mod? Mod => TexToolsService.GetMod(this);

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
	}

	public bool CanOwn => this.Category.HasFlagUnsafe(ItemCategories.Premium) || this.Category.HasFlagUnsafe(ItemCategories.Limited);
	public bool IsOwned
	{
		get => FavoritesService.IsOwned(this);
		set => FavoritesService.SetOwned(this, value);
	}

	public ItemCategories Category => GameDataService.GetCategory(this);

	static Item IExcelRow<Item>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot) => this.EquipSlotCategory.Value.Contains(slot);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ItemSlots GetItemSlots() => this.EquipSlotCategory.Value.GetItemSlots();

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
