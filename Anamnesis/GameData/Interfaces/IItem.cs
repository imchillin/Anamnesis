// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;
	using System.Windows.Media;
	using Anamnesis.TexTools;

	public interface IItem : IRow
	{
		ImageSource? Icon { get; }

		ushort ModelSet { get; }
		ushort ModelBase { get; }
		ushort ModelVariant { get; }

		bool HasSubModel { get; }
		ushort SubModelSet { get; }
		ushort SubModelBase { get; }
		ushort SubModelVariant { get; }

		Classes EquipableClasses { get; }

		bool IsWeapon { get; }

		Mod? Mod { get; }
		bool IsFavorite { get; set;  }

		bool FitsInSlot(ItemSlots slot);
	}
}
