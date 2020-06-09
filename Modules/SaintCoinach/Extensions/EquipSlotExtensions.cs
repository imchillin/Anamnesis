// Concept Matrix 3.
// Licensed under the MIT license.

namespace SaintCoinach.Xiv
{
	using System;
	using ConceptMatrix.GameData;

	public static class EquipSlotExtensions
	{
		public static bool IsSlot(this EquipSlot self, ItemSlots slot)
		{
			string name = self.Name;

			switch (slot)
			{
				// TODO: ensure this works in every language!
				case ItemSlots.MainHand: return name == "Main Hand";
				case ItemSlots.Head: return name == "Head";
				case ItemSlots.Body: return name == "Body";
				case ItemSlots.Hands: return name == "Hands";
				case ItemSlots.Waist: return name == "Waist";
				case ItemSlots.Legs: return name == "Legs";
				case ItemSlots.Feet: return name == "Feet";
				case ItemSlots.OffHand: return name == "Off Hand";
				case ItemSlots.Ears: return name == "Ears";
				case ItemSlots.Neck: return name == "Neck";
				case ItemSlots.Wrists: return name == "Wrists";
				case ItemSlots.RightRing: return name == "Right Ring";
				case ItemSlots.LeftRing: return name == "Left Ring";
			}

			throw new Exception($"Unknown item slot: {slot}");
		}
	}
}
