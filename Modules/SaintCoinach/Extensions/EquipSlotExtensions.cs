// Concept Matrix 3.
// Licensed under the MIT license.

namespace SaintCoinach.Xiv
{
	using System;
	using ConceptMatrix;
	using ConceptMatrix.Services;

	public static class EquipSlotExtensions
	{
		public static bool IsSlot(this EquipSlot self, ItemSlots slot)
		{
			switch (slot)
			{
				// TODO: ensuire this works in every laguage!
				case ItemSlots.MainHand: return self.Name == "Main Hand";
				case ItemSlots.Head: return self.Name == "Head";
				case ItemSlots.Body: return self.Name == "Body";
				case ItemSlots.Hands: return self.Name == "Hands";
				case ItemSlots.Waist: return self.Name == "Waist";
				case ItemSlots.Legs: return self.Name == "Legs";
				case ItemSlots.Feet: return self.Name == "Feet";
				case ItemSlots.OffHand: return self.Name == "Off Hand";
				case ItemSlots.Ears: return self.Name == "Ears";
				case ItemSlots.Neck: return self.Name == "Neck";
				case ItemSlots.Wrists: return self.Name == "Wrists";
				case ItemSlots.RightRing: return self.Name == "Right Ring";
				case ItemSlots.LeftRing: return self.Name == "Left Wing";
			}

			throw new Exception($"Unknown item slot: {slot}");
		}
	}
}
