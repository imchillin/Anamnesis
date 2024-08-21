// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;

public class NpcBodyItem : IItem
{
	public string Name => LocalizationService.GetString("Item_NpcBody");
	public string Description => LocalizationService.GetString("Item_NpcBodyDesc");
	public ImageReference? Icon => GameDataService.Items.Get(10033)?.Icon;
	public ushort ModelBase => 9903;
	public ushort ModelVariant => 1;
	public ushort ModelSet => 0;
	public uint RowId => 0;
	public bool IsWeapon => false;
	public bool HasSubModel => false;
	public ushort SubModelBase => 0;
	public ushort SubModelVariant => 0;
	public ushort SubModelSet => 0;
	public Classes EquipableClasses => Classes.All;
	public Mod? Mod => TexToolsService.GetMod(this.Name);
	public byte EquipLevel => 0;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public bool CanOwn => false;
	public bool IsOwned { get; set; }

	public ItemCategories Category => ItemCategories.Standard;

	public bool FitsInSlot(ItemSlots slot)
	{
		return slot == ItemSlots.Body || slot == ItemSlots.Feet || slot == ItemSlots.Hands || slot == ItemSlots.Legs;
	}
}
