// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class WeaponViewModel : StructViewModelBase<Weapon>
	{
		public WeaponViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public ushort Set { get; set; }
		[ModelField] public ushort Base { get; set; }
		[ModelField] public ushort Variant { get; set; }
		[ModelField] public byte Dye { get; set; }
	}
}
