// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using System.ComponentModel;
	using System.Windows.Media.Media3D;
	using ConceptMatrix;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public class EquipmentWeaponViewModel : EquipmentBaseViewModel
	{
		private IMemory<Weapon> memory;

		public EquipmentWeaponViewModel(IMemory<Weapon> memory, ItemSlots slot)
			: base(slot)
		{
			this.modelSet = memory.Value.Set;
			this.modelBase = memory.Value.Base;
			this.modelVariant = memory.Value.Variant;
			this.dyeId = memory.Value.Dye;

			this.memory = memory;
			this.memory.ValueChanged += this.Memory_ValueChanged;

			this.HasModelSet = true;
			this.CanColor = true;
			this.Color = new Color();
			this.CanScale = true;
			this.Scale = new Vector3D(1, 1, 1);
			this.CanDye = true;

			this.Item = this.GetItem();
		}

		protected override void Apply()
		{
			Weapon w = this.memory.Value;
			w.Base = this.ModelBase;
			////w.Dye = this.Dye;
			w.Set = this.ModelSet;
			w.Variant = this.ModelVariant;
			this.memory.Value = w;
		}

		private void Memory_ValueChanged(object sender, object value)
		{
			Log.Write("Changed!");
		}
	}
}
