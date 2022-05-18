// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Sheets;
using Anamnesis.TexTools;

public interface IItem
{
	uint RowId { get; }

	string Name { get; }
	string? Description { get; }
	byte EquipLevel { get; }

	ImageReference? Icon { get; }

	ushort ModelSet { get; }
	ushort ModelBase { get; }
	ushort ModelVariant { get; }

	bool HasSubModel { get; }
	ushort SubModelSet { get; }
	ushort SubModelBase { get; }
	ushort SubModelVariant { get; }

	Classes EquipableClasses { get; }

	bool IsWeapon { get; }

	ItemCategories Category { get; }

	Mod? Mod { get; }
	bool IsFavorite { get; set; }
	bool CanOwn { get; }
	bool IsOwned { get; set; }

	bool FitsInSlot(ItemSlots slot);
}
