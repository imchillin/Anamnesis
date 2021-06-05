// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class DummyNoneItem : IItem
	{
		public string Name => LocalizationService.GetString("Item_None");
		public string? Description => LocalizationService.GetString("Item_NoneDesc");
		public ImageSource? Icon => null;
		public ushort ModelBase => 0;
		public ushort ModelVariant => 0;
		public ushort ModelSet => 0;
		public uint Key => 0;
		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => null;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return true;
		}
	}
}
