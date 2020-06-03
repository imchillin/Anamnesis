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
		private readonly IMemory<Weapon> memory;
		private readonly IMemory<Color> colorMem;
		private readonly IMemory<Vector> scaleMem;

		public EquipmentWeaponViewModel(ItemSlots slot, Actor actor)
			: base(slot, actor)
		{
			this.memory = actor.GetMemory(slot == ItemSlots.MainHand ? Offsets.Main.MainHand : Offsets.Main.OffHand);
			this.memory.ValueChanged += this.Memory_ValueChanged;

			this.modelSet = this.memory.Value.Set;
			this.modelBase = this.memory.Value.Base;
			this.modelVariant = this.memory.Value.Variant;
			this.dyeId = this.memory.Value.Dye;

			if (this.HasWeapon)
			{
				this.scaleMem = actor.GetMemory(slot == ItemSlots.MainHand ? Offsets.Main.MainHandScale : Offsets.Main.OffhandScale);
				this.scaleMem.ValueChanged += this.ScaleMem_ValueChanged;

				this.colorMem = actor.GetMemory(slot == ItemSlots.MainHand ? Offsets.Main.MainHandColor : Offsets.Main.OffhandColor);
				this.colorMem.ValueChanged += this.ColorMem_ValueChanged;

				this.scale = this.scaleMem.Value;
				this.color = this.colorMem.Value;
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
