// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("BuddyEquip", columnHash: 0xb429792a)]
public class BuddyEquip : ExcelRow
{
	public BuddyItem? Head { get; private set; }
	public BuddyItem? Body { get; private set; }
	public BuddyItem? Feet { get; private set; }

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		string name = parser.ReadColumn<SeString>(8) ?? string.Empty;

		int h = parser.ReadColumn<int>(9);
		ushort headBase = (ushort)h;
		ushort headVariant = (ushort)(h >> 16);
		ushort headIcon = parser.ReadColumn<ushort>(13);
		if (headBase != 0 || headVariant != 0)
			this.Head = new(name, ItemSlots.Head, headBase, headVariant, headIcon);

		int b = parser.ReadColumn<int>(10);
		ushort bodyBase = (ushort)b;
		ushort bodyVariant = (ushort)(b >> 16);
		ushort bodyIcon = parser.ReadColumn<ushort>(14);
		if (bodyBase != 0 || bodyVariant != 0)
			this.Body = new(name, ItemSlots.Body, bodyBase, bodyVariant, bodyIcon);

		int l = parser.ReadColumn<int>(11);
		ushort legsBase = (ushort)l;
		ushort legsVariant = (ushort)(l >> 16);
		ushort legsIcon = parser.ReadColumn<ushort>(15);
		if (legsBase != 0 || legsVariant != 0)
		{
			this.Feet = new(name, ItemSlots.Feet, legsBase, legsVariant, legsIcon);
		}
	}

	public class BuddyItem : IItem
	{
		public BuddyItem(string name, ItemSlots slot, ushort modelBase, ushort modelVariant, ushort icon)
		{
			this.Name = name;
			this.Slot = slot;
			this.ModelBase = modelBase;
			this.ModelVariant = modelVariant;
			this.Icon = new(icon);
		}

		public string Name { get; private set; } = string.Empty;
		public ItemSlots Slot { get; private set; }
		public ushort ModelBase { get; private set; }
		public ushort ModelVariant { get; private set; }
		public ImageReference? Icon { get; private set; }

		public uint RowId => 0;
		public string? Description => null;
		public bool HasSubModel => false;
		public ushort ModelSet => 0;
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
