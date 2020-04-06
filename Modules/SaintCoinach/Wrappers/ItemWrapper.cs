// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class ItemWrapper : ObjectWrapper<Item>, IItem
	{
		private Dictionary<ItemSlots, bool> fitsInSlotsCache = new Dictionary<ItemSlots, bool>();

		public ItemWrapper(Item value)
			: base(value)
		{
			// Warm the slotcache for all items to save time later
			foreach (ItemSlots slot in Enum.GetValues(typeof(ItemSlots)))
			{
				this.FitsInSlot(slot);
			}
		}

		public string Name
		{
			get
			{
				return this.Value.Name;
			}
		}

		public string Description
		{
			get
			{
				return this.Value.Description;
			}
		}

		public IImage Icon
		{
			get
			{
				return this.Value.Icon.ToIImage();
			}
		}

		public short ModelBaseId
		{
			get
			{
				return this.Value.ModelMain.Value1;
			}
		}

		public byte ModelVariantId
		{
			get
			{
				return (byte)this.Value.ModelMain.Value2;
			}
		}

		public byte ModelId
		{
			get
			{
				return (byte)this.Value.ModelMain.Value3;
			}
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			if (this.fitsInSlotsCache.ContainsKey(slot))
				return this.fitsInSlotsCache[slot];

			foreach (EquipSlot equipSlot in this.Value.EquipSlotCategory.PossibleSlots)
			{
				if (equipSlot.IsSlot(slot))
				{
					this.fitsInSlotsCache.Add(slot, true);
					return true;
				}
			}

			this.fitsInSlotsCache.Add(slot, false);
			return false;
		}
	}
}
