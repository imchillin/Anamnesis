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

		public int ModelBaseId
		{
			get
			{
				return this.Value.ModelMain.Value2;
			}
		}

		public int ModelVariantId
		{
			get
			{
				return this.Value.ModelMain.Value3;
			}
		}

		public int ModelId
		{
			get
			{
				return this.Value.ModelMain.Value1;
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
