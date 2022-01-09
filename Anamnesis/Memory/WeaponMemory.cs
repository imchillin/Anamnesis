// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class WeaponMemory : MemoryBase, IEquipmentItemMemory
	{
		[Bind(0x000, BindFlags.ActorRefresh)] public ushort Set { get; set; }
		[Bind(0x002, BindFlags.ActorRefresh)] public ushort Base { get; set; }
		[Bind(0x004, BindFlags.ActorRefresh)] public ushort Variant { get; set; }
		[Bind(0x006, BindFlags.ActorRefresh)] public byte Dye { get; set; }
		[Bind(0x008, BindFlags.Pointer)] public WeaponModelMemory? Model { get; set; }
	}
}
