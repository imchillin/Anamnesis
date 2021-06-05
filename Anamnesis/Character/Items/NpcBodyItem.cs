// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class NpcBodyItem : IItem
	{
		public string Name => LocalizationService.GetString("Item_NpcBody");
		public string? Description => LocalizationService.GetString("Item_NpcBodyDesc");
		public ImageSource? Icon => null;
		public ushort ModelBase => 9903;
		public ushort ModelVariant => 1;
		public ushort ModelSet => 0;
		public uint Key => 0;
		public bool IsWeapon => false;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => TexToolsService.GetMod(this.Name);

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.Body || slot == ItemSlots.Feet || slot == ItemSlots.Hands || slot == ItemSlots.Legs;
		}
	}
}
