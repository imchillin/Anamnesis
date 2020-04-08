// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using ConceptMatrix.Services;

	public class Equipment
	{
		public Item Head = new Item();
		public Item Chest = new Item();
		public Item Arms = new Item();
		public Item Legs = new Item();
		public Item Feet = new Item();
		public Item Ear = new Item();
		public Item Neck = new Item();
		public Item Wrist = new Item();
		public Item RFinger = new Item();
		public Item LFinger = new Item();

		public Item GetItem(ItemSlots slot)
		{
			switch (slot)
			{
				case ItemSlots.Head: return this.Head;
				case ItemSlots.Body: return this.Chest;
				case ItemSlots.Hands: return this.Arms;
				case ItemSlots.Legs: return this.Legs;
				case ItemSlots.Feet: return this.Feet;
				case ItemSlots.Ears: return this.Ear;
				case ItemSlots.Neck: return this.Neck;
				case ItemSlots.Wrists: return this.Wrist;
				case ItemSlots.RightRing: return this.RFinger;
				case ItemSlots.LeftRing: return this.LFinger;
			}

			return null;
		}

		public class Item
		{
			public ushort Base;
			public byte Variant;
			public byte Dye;
		}
	}
}
