// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule
{
	using System.Collections.Generic;
	using ConceptMatrix;
	using ConceptMatrix.GameData;

	public class Prop : IItem
	{
		public int Key { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public ushort ModelSet { get; set; }

		public bool IsWeapon { get => true; }
		public bool HasSubModel { get => false; }
		public ushort SubModelBase { get => 0; }
		public ushort SubModelVariant { get => 0; }
		public ushort SubModelSet { get => 0; }
		public IImageSource Icon { get => null; }

		public Classes EquipableClasses
		{
			get
			{
				return Classes.All;
			}
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
		}
	}
}
