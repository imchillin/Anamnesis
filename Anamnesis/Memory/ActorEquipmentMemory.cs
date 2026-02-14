// © Anamnesis.
// Licensed under the MIT license.

using System;
using System.Buffers.Binary;

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

	public void WriteTo(Span<byte> destination)
	{
		if (destination.Length < 80)
			throw new ArgumentException("Destination must be at least 80 bytes.", nameof(destination));

		WriteSlot(destination[0x00..], this.Head);
		WriteSlot(destination[0x08..], this.Chest);
		WriteSlot(destination[0x10..], this.Arms);
		WriteSlot(destination[0x18..], this.Legs);
		WriteSlot(destination[0x20..], this.Feet);
		WriteSlot(destination[0x28..], this.Ear);
		WriteSlot(destination[0x30..], this.Neck);
		WriteSlot(destination[0x38..], this.Wrist);
		WriteSlot(destination[0x40..], this.RFinger);
		WriteSlot(destination[0x48..], this.LFinger);
	}

	private static void WriteSlot(Span<byte> slot, ItemMemory? item)
	{
		if (item == null)
		{
			slot[..8].Clear();
			return;
		}

		var baseBytes = BitConverter.GetBytes(item.Base);
		baseBytes.CopyTo(slot[0..2]);

		var variantBytes = BitConverter.GetBytes((ushort)item.Variant);
		variantBytes.CopyTo(slot[2..4]);

		slot[4] = item.Dye;
		slot[5] = item.Dye2;
		slot[6] = 0; // padding
		slot[7] = 0;
	}
}
