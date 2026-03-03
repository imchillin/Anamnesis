// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using PropertyChanged;
using RemoteController.Interop.Types;
using System.Threading;
using static Anamnesis.Actor.Utilities.DyeUtility;

public class ItemMemory : MemoryBase, IEquipmentItemMemory
{
	private readonly Lock itemLock = new();
	private ItemModelId itemModelId;

	[AlsoNotifyFor(nameof(Base), nameof(Variant), nameof(Dye), nameof(Dye2))]
	[Bind(0x000, BindFlags.ActorRefresh)]
	public ItemModelId ItemId
	{
		get
		{
			lock (this.itemLock)
			{
				return this.itemModelId;
			}
		}
		set
		{
			lock (this.itemLock)
			{
				this.itemModelId = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(ItemId))]
	public ushort Base
	{
		get
		{
			lock (this.itemLock)
			{
				return this.itemModelId.Id;
			}
		}
		set
		{
			lock (this.itemLock)
			{
				this.itemModelId.Id = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(ItemId))]
	public byte Variant
	{
		get
		{
			lock (this.itemLock)
			{
				return this.itemModelId.Variant;
			}
		}
		set
		{
			lock (this.itemLock)
			{
				this.itemModelId.Variant = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(ItemId))]
	public byte Dye
	{
		get
		{
			lock (this.itemLock)
			{
				return this.itemModelId.Dye;
			}
		}
		set
		{
			lock (this.itemLock)
			{
				this.itemModelId.Dye = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(ItemId))]
	public byte Dye2
	{
		get
		{
			lock (this.itemLock)
			{
				return this.itemModelId.Dye2;
			}
		}
		set
		{
			lock (this.itemLock)
			{
				this.itemModelId.Dye2 = value;
			}
		}
	}

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

	public void SwapDyeChannels()
	{
		(this.Dye2, this.Dye) = (this.Dye, this.Dye2);
	}
}
