// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Weapon
	{
		public ushort Set;
		public ushort Base;
		public ushort Variant;
		public byte Dye;
	}
}
