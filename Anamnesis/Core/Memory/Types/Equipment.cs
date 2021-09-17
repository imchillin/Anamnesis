// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Equipment
	{
		public Item Head;
		public Item Chest;
		public Item Arms;
		public Item Legs;
		public Item Feet;
		public Item Ear;
		public Item Neck;
		public Item Wrist;
		public Item RFinger;
		public Item LFinger;
	}
}
