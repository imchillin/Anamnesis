// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class EquipmentViewModel : StructViewModelBase<Equipment>
	{
		public EquipmentViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public ItemViewModel? Head { get; set; }
		[ModelField] public ItemViewModel? Chest { get; set; }
		[ModelField] public ItemViewModel? Arms { get; set; }
		[ModelField] public ItemViewModel? Legs { get; set; }
		[ModelField] public ItemViewModel? Feet { get; set; }
		[ModelField] public ItemViewModel? Ear { get; set; }
		[ModelField] public ItemViewModel? Neck { get; set; }
		[ModelField] public ItemViewModel? Wrist { get; set; }
		[ModelField] public ItemViewModel? RFinger { get; set; }
		[ModelField] public ItemViewModel? LFinger { get; set; }
	}
}
