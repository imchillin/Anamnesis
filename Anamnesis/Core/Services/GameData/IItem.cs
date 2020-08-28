// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;

	public interface IItem : IDataObject
	{
		string Name { get; }
		string Description { get; }
		IImageSource Icon { get; }

		ushort ModelSet { get; }
		ushort ModelBase { get; }
		ushort ModelVariant { get; }

		bool HasSubModel { get; }
		ushort SubModelSet { get; }
		ushort SubModelBase { get; }
		ushort SubModelVariant { get; }

		Classes EquipableClasses { get; }

		bool IsWeapon { get; }

		bool FitsInSlot(ItemSlots slot);
	}
}
