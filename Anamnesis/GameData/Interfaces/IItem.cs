// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Sheets;
using Anamnesis.TexTools;

/// <summary>Represents an item in the game data.</summary>
public interface IItem
{
	/// <summary>Gets the row ID.</summary>
	uint RowId { get; }

	/// <summary>Gets the name of the item.</summary>
	string Name { get; }

	/// <summary>Gets the description of the item.</summary>
	string? Description { get; }

	/// <summary>Gets the level required to equip the item.</summary>
	byte EquipLevel { get; }

	/// <summary>Gets the icon reference of the item.</summary>
	ImgRef? Icon { get; }

	/// <summary>Gets the model of the item.</summary>
	/// <remarks>
	/// <para>
	/// Use <see cref="ModelSet"/>, <see cref="ModelBase"/>, and <see cref="ModelVariant"/> to get the individual components.
	/// </para>
	/// To improve performance, it is preferable to use this property over the individual components.
	/// </remarks>
	ulong Model { get; }

	/// <summary>Gets the model set of the item.</summary>
	ushort ModelSet { get; }

	/// <summary>Gets the model base of the item.</summary>
	ushort ModelBase { get; }

	/// <summary>Gets the model variant of the item.</summary>
	ushort ModelVariant { get; }

	/// <summary>Gets a value indicating whether the item has a sub-model.</summary>
	bool HasSubModel { get; }

	/// <summary>Gets the sub-model of the item.</summary>
	/// <remarks>
	/// <para>
	/// Use <see cref="SubModelSet"/>, <see cref="SubModelBase"/>, and <see cref="SubModelVariant"/> to get the individual components.
	/// </para>
	/// To improve performance, it is preferable to use this property over the individual components.
	/// </remarks>
	ulong SubModel { get; }

	/// <summary>Gets the sub-model set of the item.</summary>
	ushort SubModelSet { get; }

	/// <summary>Gets the sub-model base of the item.</summary>
	ushort SubModelBase { get; }

	/// <summary>Gets the sub-model variant of the item.</summary>
	ushort SubModelVariant { get; }

	/// <summary>Gets the classes that can equip the item.</summary>
	Classes EquipableClasses { get; }

	/// <summary>Gets a value indicating whether the item is a weapon.</summary>
	bool IsWeapon { get; }

	/// <summary>Gets the item's category.</summary>
	ItemCategories Category { get; }

	/// <summary>Gets the TexTools mod associated with the item.</summary>
	Mod? Mod { get; }

	/// <summary>Gets or sets a value indicating whether the item is a favorite.</summary>
	bool IsFavorite { get; set; }

	/// <summary>Gets a value indicating whether the item can be owned.</summary>
	bool CanOwn { get; }

	/// <summary>Gets or sets a value indicating whether the item is owned.</summary>
	bool IsOwned { get; set; }

	/// <summary>Determines whether the item fits in the specified slot.</summary>
	/// <param name="slot">The slot to check.</param>
	/// <returns>True if the item fits in the slot; otherwise, false.</returns>
	bool FitsInSlot(ItemSlots slot);
}
