// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	using System;
	using Anamnesis;

	public enum ItemSlots
	{
		MainHand,

		Head,
		Body,
		Hands,
		Waist,
		Legs,
		Feet,

		OffHand,
		Ears,
		Neck,
		Wrists,
		RightRing,
		LeftRing,

		SoulCrystal,
	}

	#pragma warning disable SA1649
	public static class EquipmentExtensions
	{
		public static Equipment.Item GetItem(this Equipment self, ItemSlots slot)
		{
			switch (slot)
			{
				case ItemSlots.Head: return self.Head;
				case ItemSlots.Body: return self.Chest;
				case ItemSlots.Hands: return self.Arms;
				case ItemSlots.Legs: return self.Legs;
				case ItemSlots.Feet: return self.Feet;
				case ItemSlots.Ears: return self.Ear;
				case ItemSlots.Neck: return self.Neck;
				case ItemSlots.Wrists: return self.Wrist;
				case ItemSlots.RightRing: return self.RFinger;
				case ItemSlots.LeftRing: return self.LFinger;
			}

			throw new Exception("Invalid slot " + slot);
		}
	}
}
