// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.Windows.Media.Media3D;
	using ConceptMatrix;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

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

			this.modelSet = this.memory.Value.Set;
			this.modelBase = this.memory.Value.Base;
			this.modelVariant = this.memory.Value.Variant;
			this.dyeId = this.memory.Value.Dye;

			this.HasModelSet = true;
			this.CanColor = true;
			this.CanScale = true;
			this.CanDye = true;

			this.Item = this.GetItem();
			this.Dye = this.GetDye();
		}

		public override Vector Scale
		{
			get
			{
				return this.scaleMem.Value;
			}

			set
			{
				this.scaleMem.Value = value;
			}
		}

		public override Color Color
		{
			get
			{
				return this.colorMem.Value;
			}

			set
			{
				this.colorMem.Value = value;
			}
		}

		protected override void Apply()
		{
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
	}
}
