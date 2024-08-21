// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;

using Anamnesis.GameData.Sheets;
using Anamnesis.GameData;
using Anamnesis.Services;
using Anamnesis.TexTools;

public class EmperorsAccessoryItem : IItem
{
	public string Name => LocalizationService.GetString("Item_EmperorsBody");
	public string Description => LocalizationService.GetString("Item_EmperorsBodyDesc");
	public ImageReference? Icon => GameDataService.Items.Get(10033)?.Icon;
	public ushort ModelBase => 53;
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
		return slot == ItemSlots.Ears || slot == ItemSlots.Neck || slot == ItemSlots.Wrists || slot == ItemSlots.LeftRing || slot == ItemSlots.RightRing;
	}
}
