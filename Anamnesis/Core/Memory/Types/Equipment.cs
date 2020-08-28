// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
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

		public class Item
		{
			public ushort Base;
			public byte Variant;
			public byte Dye;
		}
	}
}
