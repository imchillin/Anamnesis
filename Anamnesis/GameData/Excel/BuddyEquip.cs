// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.TexTools;
using Lumina.Excel;

[Sheet("BuddyEquip", 0xB429792A)]
public readonly struct BuddyEquip(ExcelPage page, uint offset, uint row)
	: IExcelRow<BuddyEquip>
{
	public uint RowId => row;

	public readonly string Name => page.ReadString(offset + 8, offset).ToString();
	public BuddyItem? Head
	{
		get
		{
			int headData = page.ReadInt32(offset + 20);
			ushort headBase = (ushort)headData;
			ushort headVariant = (ushort)(headData >> 16);
			ushort headIcon = page.ReadUInt16(offset + 32);

			if (headBase == 0 && headVariant == 0)
				return null;

			return new(this.Name, ItemSlots.Head, headBase, headVariant, headIcon);
		}
	}

	public BuddyItem? Body
	{
		get
		{
			int bodyData = page.ReadInt32(offset + 24);
			ushort bodyBase = (ushort)bodyData;
			ushort bodyVariant = (ushort)(bodyData >> 16);
			ushort bodyIcon = page.ReadUInt16(offset + 34);

			if (bodyBase == 0 && bodyVariant == 0)
				return null;

			return new(this.Name, ItemSlots.Body, bodyBase, bodyVariant, bodyIcon);
		}
	}

	public BuddyItem? Feet
	{
		get
		{
			int legsData = page.ReadInt32(offset + 28);
			ushort legsBase = (ushort)legsData;
			ushort legsVariant = (ushort)(legsData >> 16);
			ushort legsIcon = page.ReadUInt16(offset + 36);

			if (legsBase != 0 || legsVariant != 0)
				return new(this.Name, ItemSlots.Feet, legsBase, legsVariant, legsIcon);

			return null;
		}
	}

	static BuddyEquip IExcelRow<BuddyEquip>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	public class BuddyItem(string name, ItemSlots slot, ushort modelBase, ushort modelVariant, ushort icon)
		: IItem
	{
		public string Name { get; private set; } = name;
		public ItemSlots Slot { get; private set; } = slot;
		public ushort ModelBase { get; private set; } = modelBase;
		public ushort ModelVariant { get; private set; } = modelVariant;
		public ImageReference? Icon { get; private set; } = new(icon);

		public uint RowId => 0;
		public string? Description => null;
		public bool HasSubModel => false;

		public ulong Model => 0;
		public ushort ModelSet => 0;
		public ulong SubModel => 0;
		public ushort SubModelSet => 0;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public Classes EquipableClasses => Classes.All;
		public bool IsWeapon => false;
		public ItemCategories Category => ItemCategories.None;
		public Mod? Mod => null;
		public bool IsFavorite { get; set; }
		public bool CanOwn => false;
		public bool IsOwned { get; set; }
		public byte EquipLevel => 0;

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == this.Slot;
		}
	}
}
