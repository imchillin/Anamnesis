// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

/// <summary>
/// Associates item groupings with ID prefixes for easy identification of favorites in the general "items" array.
/// These groupings are loosely associated with items added to the EquipmentSelector via its LoadItems() method.
/// 
/// Use a 5 digit prefix to prevent collisions with existing game item data, which currently has rowIds up to ID ~48800.
/// Using prefixes that start with 42000, since max uint is 4,294,967,295. This should allow for 99,999 prefixed items.
/// There is no prefix for None, which is used for normal items.
/// </summary>
public enum ItemFavoriteCategory
{
	None,
	ChocoboSkin = 42000,
	BuddyEquipment = 42001,
	OneOffItem = 42002,
	CustomEquipment = 42003,
	Perform = 42004
}
