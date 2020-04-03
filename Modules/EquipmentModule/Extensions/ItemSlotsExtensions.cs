// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule
{
	using ConceptMatrix.Services;

	public static class ItemSlotsExtensions
	{
		public static string ToDisplayName(this ItemSlots self)
		{
			switch (self)
			{
				case ItemSlots.MainHand: return "Main Hand";
				case ItemSlots.OffHand: return "Off Hand";
				case ItemSlots.RightRing: return "Right Ring";
				case ItemSlots.LeftRing: return "Left Ring";
			}

			return self.ToString();
		}
	}
}
