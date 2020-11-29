// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct WeaponExtended
	{
		[FieldOffset(0x28)] public IntPtr SubModel;
		[FieldOffset(0x70)] public Vector Scale;
		[FieldOffset(0x258)] public Color Tint;
	}

	public class WeaponExtendedViewModel : WeaponSubExtendedViewModel
	{
		public WeaponExtendedViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public WeaponExtendedViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public WeaponSubExtendedViewModel? SubModel { get; set; }
	}

	#pragma warning disable SA1402
	public class WeaponSubExtendedViewModel : MemoryViewModelBase<WeaponExtended>
	{
		public WeaponSubExtendedViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public WeaponSubExtendedViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public Vector Scale { get; set; }
		[ModelField] public Color Tint { get; set; }
	}
}
