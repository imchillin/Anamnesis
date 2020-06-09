// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	public interface IItem : IDataObject
	{
		string Name { get; }
		string Description { get; }
		IImage Icon { get; }

		ushort ModelBase { get; }
		ushort ModelVariant { get; }
		ushort WeaponSet { get; }

		bool IsWeapon { get; }

		bool FitsInSlot(ItemSlots slot);
	}
}
