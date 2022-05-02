// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GameData;

public class ItemMemory : MemoryBase, IEquipmentItemMemory
{
	[Bind(0x000, BindFlags.ActorRefresh)] public ushort Base { get; set; }
	[Bind(0x002, BindFlags.ActorRefresh)] public byte Variant { get; set; }
	[Bind(0x003, BindFlags.ActorRefresh)] public byte Dye { get; set; }

	// Item's dont have a 'Set' but the UI wants to bind to something, so...
	public ushort Set { get; set; } = 0;
	public bool WeaponHidden { get; set; } = false;

	public void Clear(bool isPlayer)
	{
		this.Base = (ushort)(isPlayer ? 0 : 1);
		this.Variant = 0;
		this.Dye = 0;
	}

	public void Equip(IItem item)
	{
		this.Base = item.ModelBase;
		this.Variant = (byte)item.ModelVariant;
	}

	public bool Is(IItem? item)
	{
		if (item == null)
			return false;

		return this.Base == item.ModelBase && this.Variant == item.ModelVariant;
	}
}
