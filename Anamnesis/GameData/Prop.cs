// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Windows.Media;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class Prop : IJsonRow, IItem
	{
		public uint RowId { get; set; }

		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public ushort ModelSet { get; set; }

		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public ImageReference? Icon => null;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => null;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool CanOwn => false;
		public bool IsOwned { get; set; }

		public ItemCategories Category => ItemCategories.Props;

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
		}
	}
}
