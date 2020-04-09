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

		public bool IsWeapon
		{
			get
			{
				return this.FitsInSlot(ItemSlots.MainHand) || this.FitsInSlot(ItemSlots.OffHand);
			}
		}

		public ushort ModelBase
		{
			get
			{
				return this.IsWeapon ? (ushort)this.Value.ModelMain.Value2 : (ushort)this.Value.ModelMain.Value1;
			}
		}

		public ushort ModelVariant
		{
			get
			{
				return this.IsWeapon ? (ushort)this.Value.ModelMain.Value3 : (ushort)this.Value.ModelMain.Value2;
			}
		}

		public ushort WeaponSet
		{
			get
			{
				return (ushort)this.Value.ModelMain.Value1;
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
