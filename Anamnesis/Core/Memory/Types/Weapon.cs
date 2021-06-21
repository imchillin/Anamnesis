// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Runtime.InteropServices;
	using PropertyChanged;

	[StructLayout(LayoutKind.Sequential)]
	public struct Weapon
	{
		public ushort Set;
		public ushort Base;
		public ushort Variant;
		public byte Dye;
	}

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
