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
}
