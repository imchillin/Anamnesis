// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class InvisibleBodyItem : IItem
	{
		public string Name => LocalizationService.GetString("Item_InvisibleBody");
		public string? Description => LocalizationService.GetString("Item_InvisibleBodyDesc");
		public ImageSource? Icon => null;
		public ushort ModelSet => 0;
		public ushort ModelBase => 6103;
		public ushort ModelVariant => 254;
		public bool HasSubModel => false;
		public ushort SubModelSet => 0;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public Classes EquipableClasses => Classes.All;
		public bool IsWeapon => false;
		public Mod? Mod => null;
		public uint Key => 0;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.Body;
		}
	}
}
