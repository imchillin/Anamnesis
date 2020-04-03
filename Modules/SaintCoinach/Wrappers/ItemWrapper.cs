// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class ItemWrapper : ObjectWrapper<Item>, IItem
	{
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

		public string MainModelQuad
		{
			get
			{
				return this.Value.ModelMain.ToString();
			}
		}

		public string SubModelQuad
		{
			get
			{
				return this.Value.ModelSub.ToString();
			}
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			foreach (EquipSlot equipSlot in this.Value.EquipSlotCategory.PossibleSlots)
			{
				if (equipSlot.IsSlot(slot))
				{
					return true;
				}
			}

			return false;
		}
	}
}
