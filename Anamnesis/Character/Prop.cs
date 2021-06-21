// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class Prop : IJsonRow, IItem
	{
		public uint Key { get; set; }

		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public ushort ModelSet { get; set; }

		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public ImageSource? Icon => null;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => null;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
		}
	}
}
