// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Item
	{
		public ushort Base;
		public byte Variant;
		public byte Dye;
	}
}
