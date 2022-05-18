// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using System;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Item", 0x800968c9)]
public class Item : ExcelRow, IItem
{
	public string Name { get; private set; } = string.Empty;
	public string Description { get; private set; } = string.Empty;
	public ImageReference? Icon { get; private set; }
	public byte EquipLevel { get; private set; }
	public ushort ModelSet { get; private set; }
	public ushort ModelBase { get; private set; }
	public ushort ModelVariant { get; private set; }
	public ushort SubModelSet { get; private set; }
	public ushort SubModelBase { get; private set; }
	public ushort SubModelVariant { get; private set; }
	public Classes EquipableClasses { get; private set; } = Classes.None;
	public EquipSlotCategory? EquipSlot { get; private set; }
	public EquipRaceCategory? EquipRestriction { get; private set; }

	public bool IsWeapon { get; private set; }
	public bool HasSubModel => this.SubModelSet != 0;

	public Mod? Mod { get; private set; }

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public bool CanOwn => this.Category.HasFlag(ItemCategories.Premium) || this.Category.HasFlag(ItemCategories.Limited);
	public bool IsOwned
	{
		get => FavoritesService.IsOwned(this);
		set => FavoritesService.SetOwned(this, value);
	}

	public ItemCategories Category { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Category = GameDataService.GetCategory(this);

		this.Description = parser.ReadColumn<SeString>(8) ?? string.Empty;
		this.Name = parser.ReadColumn<SeString>(9) ?? string.Empty;
		this.Icon = parser.ReadImageReference<ushort>(10);
		////ItemLevel? itemLevel = parser.ReadRowReference<ushort, ItemLevel>(11);

		this.EquipSlot = parser.ReadRowReference<byte, EquipSlotCategory>(17);
		this.EquipLevel = parser.ReadColumn<byte>(40);
		this.EquipRestriction = parser.ReadRowReference<byte, EquipRaceCategory>(42);
		this.EquipableClasses = parser.ReadRowReference<byte, ClassJobCategory>(43)?.ToFlags() ?? Classes.None;

		this.IsWeapon = this.FitsInSlot(ItemSlots.MainHand) || this.FitsInSlot(ItemSlots.OffHand);

		if (this.IsWeapon)
		{
			this.ModelSet = parser.ReadWeaponSet(47);
			this.ModelBase = parser.ReadWeaponBase(47);
			this.ModelVariant = parser.ReadWeaponVariant(47);

			this.SubModelSet = parser.ReadWeaponSet(48);
			this.SubModelBase = parser.ReadWeaponBase(48);
			this.SubModelVariant = parser.ReadWeaponVariant(48);
		}
		else
		{
			this.ModelSet = parser.ReadSet(47);
			this.ModelBase = parser.ReadBase(47);
			this.ModelVariant = parser.ReadVariant(47);

			this.SubModelSet = parser.ReadSet(48);
			this.SubModelBase = parser.ReadBase(48);
			this.SubModelVariant = parser.ReadVariant(48);
		}

		this.Mod = TexToolsService.GetMod(this);
	}

	public bool FitsInSlot(ItemSlots slot)
	{
		return this.EquipSlot?.Contains(slot) ?? false;
	}

	private static Classes ToFlags(ClassJobCategory self)
	{
		Classes classes = Classes.None;

		foreach (Classes? job in Enum.GetValues(typeof(Classes)))
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
