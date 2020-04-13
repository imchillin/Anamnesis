// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using ConceptMatrix.GameData;

	public class EquipmentItemViewModel : EquipmentBaseViewModel
	{
		private IMemory<Equipment> memory;

		public EquipmentItemViewModel(IMemory<Equipment> equipmentMemory, ItemSlots slot, Selection selection)
			: base(slot, selection)
		{
			this.memory = equipmentMemory;
			Equipment.Item item = equipmentMemory.Value.GetItem(slot);

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
			this.Dye = this.GetDye();
		}

		public override void Dispose()
		{
			this.memory.Dispose();
		}

		protected override void Apply()
		{
			Equipment eq = this.memory.Value;

			Equipment.Item i = eq.GetItem(this.Slot);
			i.Base = this.ModelBase;
			i.Dye = this.DyeId;
			i.Variant = (byte)this.ModelVariant;

			this.memory.Value = eq;
		}
	}
}
