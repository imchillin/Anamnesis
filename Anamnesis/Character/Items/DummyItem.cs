// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class DummyItem : IItem
	{
		public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			this.ModelSet = modelSet;
			this.ModelBase = modelBase;
			this.ModelVariant = modelVariant;
		}

		public uint Key => 0;
		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public string Name => LocalizationService.GetString("Item_Unknown");
		public string? Description => null;
		public ImageSource? Icon => null;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => null;

		public ushort ModelBase
		{
			get;
			private set;
		}

		public ushort ModelVariant
		{
			get;
			private set;
		}

		public ushort ModelSet
		{
			get;
			private set;
		}

		public ushort SubModelBase { get; }
		public ushort SubModelVariant { get; }
		public ushort SubModelSet { get; }

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public virtual bool FitsInSlot(ItemSlots slot)
		{
			return true;
		}
	}
}
