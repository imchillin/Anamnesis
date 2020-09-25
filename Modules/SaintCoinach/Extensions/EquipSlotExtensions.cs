// Concept Matrix 3.
// Licensed under the MIT license.

namespace SaintCoinach.Xiv
{
	using System;
	using System.Collections.Generic;
	using Anamnesis.GameData;

	public static class EquipSlotExtensions
	{
		private static Dictionary<EquipSlot, ItemSlots> slotLookup = new Dictionary<EquipSlot, ItemSlots>();

		public static bool IsSlot(this EquipSlot self, ItemSlots slot)
		{
			return self.ToSlot() == slot;
		}

		public static ItemSlots ToSlot(this EquipSlot self)
		{
			ItemSlots slot;
			lock (slotLookup)
			{
				if (!slotLookup.TryGetValue(self, out slot))
				{
					string name = self.Name;

					slot = name switch
					{
						"Main Hand" => ItemSlots.MainHand,
						"Head" => ItemSlots.Head,
						"Body" => ItemSlots.Body,
						"Hands" => ItemSlots.Hands,
						"Waist" => ItemSlots.Waist,
						"Legs" => ItemSlots.Legs,
						"Feet" => ItemSlots.Feet,
						"Off Hand" => ItemSlots.OffHand,
						"Ears" => ItemSlots.Ears,
						"Neck" => ItemSlots.Neck,
						"Wrists" => ItemSlots.Wrists,
						"Right Ring" => ItemSlots.RightRing,
						"Left Ring" => ItemSlots.LeftRing,

						"Soul Crystal" => ItemSlots.SoulCrystal,

						_ => throw new Exception("Unknown slot name: " + name),
					};

					slotLookup.Add(self, slot);
				}
			}

			return slot;
		}
	}
}
