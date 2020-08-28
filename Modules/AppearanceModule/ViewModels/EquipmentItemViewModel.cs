// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.ViewModels
{
	using System;
	using Anamnesis.GameData;
	using Anamnesis.Memory;

	public class EquipmentItemViewModel : EquipmentBaseViewModel
	{
		private readonly IMarshaler<Equipment> memory;

		public EquipmentItemViewModel(IMarshaler<Equipment> equipmentMemory, ItemSlots slot, Actor actor)
			: base(slot, actor)
		{
			this.memory = equipmentMemory;
			this.memory.ValueChanged += this.OnMemoryValueChanged;

			this.OnMemoryValueChanged(null, null);
		}

		public override void Dispose()
		{
			this.memory.Dispose();
		}

		public override void Apply()
		{
			Equipment eq = this.memory.Value;

			Equipment.Item i = eq.GetItem(this.Slot);

			if (i.Base == this.ModelBase &&
				i.Dye == this.DyeId &&
				i.Variant == this.ModelVariant)
				return;

			i.Base = this.ModelBase;
			i.Dye = this.DyeId;
			i.Variant = (byte)this.ModelVariant;

			this.memory.SetValue(eq, true);

			this.Actor.ActorRefresh();
		}

		[PropertyChanged.SuppressPropertyChangedWarnings]
		private void OnMemoryValueChanged(object sender, object value)
		{
			if (!this.memory.Active)
				return;

			Equipment eq = this.memory.Value;

			Equipment.Item item = this.memory.Value.GetItem(this.Slot);

			if (item == null)
				return;

			this.modelBase = item.Base;
			this.modelVariant = item.Variant;
			this.dyeId = item.Dye;

			this.Item = this.GetItem();
			this.Dye = this.GetDye();
		}
	}
}
