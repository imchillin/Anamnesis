// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using static Anamnesis.Actor.Utilities.DyeUtility;

public class ItemMemory : MemoryBase, IEquipmentItemMemory
{
	[Bind(0x000, BindFlags.ActorRefresh)] public ushort Base { get; set; }
	[Bind(0x002, BindFlags.ActorRefresh)] public byte Variant { get; set; }
	[Bind(0x003, BindFlags.ActorRefresh)] public byte Dye { get; set; }
	[Bind(0x004, BindFlags.ActorRefresh)] public byte Dye2 { get; set; }

	// Items dont have a 'Set' but the UI wants to bind to something, so...
	public ushort Set { get; set; } = 0;
	public bool WeaponHidden { get; set; } = false;

	public void Clear(bool isPlayer)
	{
		this.Base = (ushort)(isPlayer ? 0 : 1);
		this.Variant = 0;
		this.Dye = 0;
		this.Dye2 = 0;
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

	public void ApplyDye(IDye dye, DyeSlot dyeSlot)
	{
		if (dyeSlot.HasFlagUnsafe(DyeSlot.First))
			this.Dye = (dye != null) ? dye.Id : DyeUtility.NoneDye.Id;

		if (dyeSlot.HasFlagUnsafe(DyeSlot.Second))
			this.Dye2 = (dye != null) ? dye.Id : DyeUtility.NoneDye.Id;
	}
}
