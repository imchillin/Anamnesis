// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.ViewModels
{
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class EquipmentWeaponViewModel : EquipmentBaseViewModel
	{
		private readonly IMarshaler<Weapon> memory;

		public EquipmentWeaponViewModel(ItemSlots slot, Actor actor)
			: base(slot, actor)
		{
			this.memory = actor.GetMemory(slot == ItemSlots.MainHand ? Offsets.Main.MainHand : Offsets.Main.OffHand);
			this.memory.ValueChanged += this.OnMemoryValueChanged;

			this.OnMemoryValueChanged(null, null);
		}

		public override void Dispose()
		{
			this.memory.ValueChanged -= this.OnMemoryValueChanged;
			this.memory.Dispose();
		}

		public override void Apply()
		{
			Weapon w = this.memory.Value;

			if (w.Base == this.ModelBase &&
				w.Dye == this.DyeId &&
				w.Set == this.ModelSet &&
				w.Variant == this.ModelVariant)
				return;

			w.Base = this.ModelBase;
			w.Dye = this.DyeId;
			w.Set = this.ModelSet;
			w.Variant = this.ModelVariant;
			this.memory.Value = w;

			this.Actor.ActorRefresh();
		}

		[PropertyChanged.SuppressPropertyChangedWarnings]
		private void OnMemoryValueChanged(object sender, object value)
		{
			this.modelSet = this.memory.Value.Set;
			this.modelBase = this.memory.Value.Base;
			this.modelVariant = this.memory.Value.Variant;
			this.dyeId = this.memory.Value.Dye;

			this.Item = this.GetItem();
			this.Dye = this.GetDye();
		}
	}
}
