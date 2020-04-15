// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class EquipmentWeaponViewModel : EquipmentBaseViewModel
	{
		private IMemory<Weapon> memory;
		private IMemory<Color> colorMem;
		private IMemory<Vector> scaleMem;

		public EquipmentWeaponViewModel(ItemSlots slot, Selection selection)
			: base(slot, selection)
		{
			this.memory = selection.BaseAddress.GetMemory(slot == ItemSlots.MainHand ? Offsets.MainHand : Offsets.OffHand);
			this.memory.ValueChanged += this.Memory_ValueChanged;

			this.colorMem = selection.BaseAddress.GetMemory(slot == ItemSlots.MainHand ? Offsets.MainHandColor : Offsets.OffhandColor);
			this.scaleMem = selection.BaseAddress.GetMemory(slot == ItemSlots.MainHand ? Offsets.MainHandScale : Offsets.OffhandScale);
			this.scaleMem.ValueChanged += this.ScaleMem_ValueChanged;

			this.modelSet = this.memory.Value.Set;
			this.modelBase = this.memory.Value.Base;
			this.modelVariant = this.memory.Value.Variant;
			this.dyeId = this.memory.Value.Dye;

			this.HasModelSet = true;
			this.CanColor = true;
			this.CanScale = true;
			this.CanDye = true;
			this.Scale = this.scaleMem.Value;

			this.Item = this.GetItem();
			this.Dye = this.GetDye();
		}

		public bool HasWeapon
		{
			get
			{
				return this.ModelBase != 0;
			}
		}

		/*public override Color Color
		{
			get
			{
				if (!this.HasWeapon)
					return Color.White;

				return this.colorMem.Value;
			}

			set
			{
				this.colorMem.Value = value;
			}
		}*/

		public override void Dispose()
		{
			this.memory.ValueChanged -= this.Memory_ValueChanged;
			this.scaleMem.ValueChanged -= this.ScaleMem_ValueChanged;
			this.memory.Dispose();
			this.colorMem.Dispose();
			this.scaleMem.Dispose();
		}

		protected override void Apply()
		{
			// Dont allow for setting none on the main hand
			if (this.Slot == ItemSlots.MainHand && !this.HasWeapon)
				return;

			Weapon w = this.memory.Value;
			w.Base = this.ModelBase;
			w.Dye = this.DyeId;
			w.Set = this.ModelSet;
			w.Variant = this.ModelVariant;
			this.memory.Value = w;

			this.scaleMem.Value = this.Scale;
		}

		private void Memory_ValueChanged(object sender, object value)
		{
			this.modelSet = this.memory.Value.Set;
			this.modelBase = this.memory.Value.Base;
			this.modelVariant = this.memory.Value.Variant;
			this.dyeId = this.memory.Value.Dye;

			this.Item = this.GetItem();
			this.Dye = this.GetDye();
		}

		private void ScaleMem_ValueChanged(object sender, object value)
		{
			this.Scale = (Vector)value;
		}
	}
}
