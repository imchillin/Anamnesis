// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class ActorEquipmentMemory : MemoryBase
{
	[Bind(0x000)] public ItemMemory? Head { get; set; }
	[Bind(0x008)] public ItemMemory? Chest { get; set; }
	[Bind(0x010)] public ItemMemory? Arms { get; set; }
	[Bind(0x018)] public ItemMemory? Legs { get; set; }
	[Bind(0x020)] public ItemMemory? Feet { get; set; }
	[Bind(0x028)] public ItemMemory? Ear { get; set; }
	[Bind(0x030)] public ItemMemory? Neck { get; set; }
	[Bind(0x038)] public ItemMemory? Wrist { get; set; }
	[Bind(0x040)] public ItemMemory? RFinger { get; set; }
	[Bind(0x048)] public ItemMemory? LFinger { get; set; }
}
