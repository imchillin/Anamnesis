// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using Anamnesis.GameData;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ItemViewModel : StructViewModelBase<Item>
	{
		public ItemViewModel(EquipmentViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public ushort Base { get; set; }
		[ModelField] public byte Variant { get; set; }
		[ModelField] public byte Dye { get; set; }

		public void Clear()
		{
			this.Base = 0;
			this.Variant = 0;
			this.Dye = 0;
		}

		public void Equip(IItem item)
		{
			this.Base = item.ModelBase;
			this.Variant = (byte)item.ModelVariant;
		}
	}
}
