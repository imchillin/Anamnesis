// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class WeaponExtendedViewModel : WeaponSubExtendedViewModel
	{
		public WeaponExtendedViewModel(IntPtr pointer, IMemoryViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		[ModelField] public WeaponSubExtendedViewModel? SubModel { get; set; }
	}
}
