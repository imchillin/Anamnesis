// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;

	public class DummyItem : IItem
	{
		public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			this.ModelSet = modelSet;
			this.ModelBase = modelBase;
			this.ModelVariant = modelVariant;
		}

		public int Key => 0;
		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public string Name => "Unknown";
		public string? Description => null;
		public ImageSource? Icon => null;
		public Classes EquipableClasses => Classes.All;

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

		public bool FitsInSlot(ItemSlots slot)
		{
			return true;
		}
	}
}
