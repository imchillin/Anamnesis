// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using RemoteController.Interop.Types;

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

	public void WriteTo(ref HumanDrawData drawData)
	{
		drawData.Head = this.Head?.ItemId ?? default;
		drawData.Chest = this.Chest?.ItemId ?? default;
		drawData.Arms = this.Arms?.ItemId ?? default;
		drawData.Legs = this.Legs?.ItemId ?? default;
		drawData.Feet = this.Feet?.ItemId ?? default;
		drawData.Ear = this.Ear?.ItemId ?? default;
		drawData.Neck = this.Neck?.ItemId ?? default;
		drawData.Wrist = this.Wrist?.ItemId ?? default;
		drawData.RFinger = this.RFinger?.ItemId ?? default;
		drawData.LFinger = this.LFinger?.ItemId ?? default;
	}
}
