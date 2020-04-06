// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public class EquipmentItemViewModel : EquipmentBaseViewModel
	{
		public EquipmentItemViewModel(Equipment target, ItemSlots slot)
			: base(slot)
		{
			Equipment.Item item = target.GetItem(slot);

			if (item == null)
				return;

			this.modelBase = item.Base;
			this.modelVariant = item.Variant;
			this.dyeId = item.Dye;

			// hmm. doesn't seem to be any reason you _cant_ dye accessories...
			// TODO: test what happens when you dye an accessory.
			this.CanDye = slot != ItemSlots.Ears
				&& slot != ItemSlots.Neck
				&& slot != ItemSlots.Wrists
				&& slot != ItemSlots.LeftRing
				&& slot != ItemSlots.RightRing;

			this.CanColor = false;
			this.CanScale = false;
			this.HasModelSet = false;

			this.Item = this.GetItem();
		}

		protected override void Apply()
		{
		}
	}
}
