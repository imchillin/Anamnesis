// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class WeaponMemory : MemoryBase
	{
		[Bind(0x000)] public ushort Set { get; set; }
		[Bind(0x002)] public ushort Base { get; set; }
		[Bind(0x004)] public ushort Variant { get; set; }
		[Bind(0x006)] public byte Dye { get; set; }
	}
}
