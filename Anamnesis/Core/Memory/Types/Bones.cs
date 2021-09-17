// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Bones
	{
		////[FieldOffset(0x00)]
		////public IntPtr HkAnimationFile;

		[FieldOffset(0x10)]
		public int Count;

		[FieldOffset(0x18)]
		public IntPtr TransformArray;
	}
}
