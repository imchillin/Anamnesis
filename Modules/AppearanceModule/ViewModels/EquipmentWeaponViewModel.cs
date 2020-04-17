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

		public EquipmentWeaponViewModel(ItemSlots slot, IBaseMemoryOffset baseOffset)
			: base(slot, baseOffset)
		{
			this.memory = baseOffset.GetMemory(slot == ItemSlots.MainHand ? Offsets.MainHand : Offsets.OffHand);
			this.memory.Name = slot.ToString();
			this.memory.ValueChanged += this.Memory_ValueChanged;

			this.modelSet = this.memory.Value.Set;
			this.modelBase = this.memory.Value.Base;
			this.modelVariant = this.memory.Value.Variant;
			this.dyeId = this.memory.Value.Dye;

			this.HasModelSet = true;
			this.CanColor = true;
			this.CanScale = true;
			this.CanDye = true;

			if (this.HasWeapon)
			{
				this.scaleMem = baseOffset.GetMemory(slot == ItemSlots.MainHand ? Offsets.MainHandScale : Offsets.OffhandScale);
				this.scaleMem.Name = slot.ToString() + "_Scale";
				this.scaleMem.ValueChanged += this.ScaleMem_ValueChanged;
				this.scaleMem.Freeze = true;

				this.colorMem = baseOffset.GetMemory(slot == ItemSlots.MainHand ? Offsets.MainHandColor : Offsets.OffhandColor);
				this.colorMem.Name = slot.ToString() + "_Color";
				this.colorMem.ValueChanged += this.ColorMem_ValueChanged;
				this.colorMem.Freeze = true;

				this.Scale = this.scaleMem.Value;
				this.Color = this.colorMem.Value;

				// for some reason, the initial value of the color memory is always 0 - black.
				// I'm not sure why this would be
				this.Color = Color.White;
			}

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

		public override void Dispose()
		{
			this.memory.ValueChanged -= this.Memory_ValueChanged;
			this.memory.Dispose();

			if (this.scaleMem != null)
			{
				this.scaleMem.ValueChanged -= this.ScaleMem_ValueChanged;
				this.scaleMem.Dispose();
			}

			if (this.colorMem != null)
			{
				this.colorMem.ValueChanged -= this.ColorMem_ValueChanged;
				this.colorMem.Dispose();
			}
		}

		protected override void Apply()
		{
			// Dont allow for setting none on the main hand
			if (this.Slot == ItemSlots.MainHand && !this.HasWeapon)
				return;

			if (this.scaleMem != null)
				this.scaleMem.Value = this.Scale;

			if (this.colorMem != null)
				this.colorMem.Value = this.Color;

			Weapon w = this.memory.Value;
			w.Base = this.ModelBase;
			w.Dye = this.DyeId;
			w.Set = this.ModelSet;
			w.Variant = this.ModelVariant;
			this.memory.Value = w;
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

		private void ColorMem_ValueChanged(object sender, object value)
		{
			this.Color = (Color)value;
		}
	}
}
