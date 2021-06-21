// © Anamnesis.
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
		public WeaponExtendedViewModel(IntPtr pointer, IMemoryViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		[ModelField] public WeaponSubExtendedViewModel? SubModel { get; set; }
	}

	#pragma warning disable SA1402
	public class WeaponSubExtendedViewModel : MemoryViewModelBase<WeaponExtended>
	{
		public WeaponSubExtendedViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public Vector Scale { get; set; }
		[ModelField] public Color Tint { get; set; }

		public bool IsHidden
		{
			get
			{
				return this.Scale == Vector.Zero;
			}
			set
			{
				this.Scale = value ? Vector.Zero : Vector.One;
			}
		}

		public void Hide()
		{
			this.IsHidden = true;
		}
	}
}
