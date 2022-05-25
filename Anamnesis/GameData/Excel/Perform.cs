// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Perform", 0x7bf81fa9)]
public class Perform : ExcelRow, IItem
{
	public string Name { get; private set; } = string.Empty;
	public string Description { get; private set; } = string.Empty;

	public ushort ModelSet { get; private set; }
	public ushort ModelBase { get; private set; }
	public ushort ModelVariant { get; private set; }
	public Mod? Mod { get; private set; }

	public ImageReference? Icon => null;
	public bool HasSubModel => false;
	public ushort SubModelSet => 0;
	public ushort SubModelBase => 0;
	public ushort SubModelVariant => 0;
	public Classes EquipableClasses => Classes.All;
	public bool IsWeapon => true;
	public byte EquipLevel => 0;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public bool CanOwn => false;
	public bool IsOwned { get; set; }

	public ItemCategories Category => ItemCategories.Performance;

	public bool FitsInSlot(ItemSlots slot)
	{
		return slot == ItemSlots.MainHand;
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.ModelSet = parser.ReadWeaponSet(2);
		this.ModelBase = parser.ReadWeaponBase(2);
		this.ModelVariant = parser.ReadWeaponVariant(2);
		this.Name = parser.ReadColumn<SeString>(9) ?? string.Empty;

		this.Mod = TexToolsService.GetMod(this);
	}
}
