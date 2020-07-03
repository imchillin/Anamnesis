// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	public interface IItem : IDataObject
	{
		string Name { get; }
		string Description { get; }
		IImage Icon { get; }

		ushort ModelSet { get; }
		ushort ModelBase { get; }
		ushort ModelVariant { get; }

		bool HasSubModel { get; }
		ushort SubModelSet { get; }
		ushort SubModelBase { get; }
		ushort SubModelVariant { get; }

		bool IsWeapon { get; }

		bool FitsInSlot(ItemSlots slot);
	}
}
