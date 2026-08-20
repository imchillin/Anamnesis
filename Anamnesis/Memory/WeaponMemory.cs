// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using Anamnesis.Services;
using PropertyChanged;
using RemoteController.Interop.Types;
using System;
using System.Numerics;
using System.Threading;
using static Anamnesis.Actor.Utilities.DyeUtility;

public class WeaponMemory : MemoryBase, IEquipmentItemMemory
{
	private readonly Lock weaponLock = new();
	private WeaponModelId weaponModelId;

	[AlsoNotifyFor(nameof(Set), nameof(Base), nameof(Variant), nameof(Dye), nameof(Dye2))]
	[Bind(WeaponData.MODEL_ID_OFFSET, BindFlags.ActorRefresh | BindFlags.WeaponRefresh)]
	public WeaponModelId WeaponModelId
	{
		get
		{
			lock (this.weaponLock)
			{
				return this.weaponModelId;
			}
		}
		set
		{
			lock (this.weaponLock)
			{
				this.weaponModelId = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(WeaponModelId))]
	public ushort Set
	{
		get
		{
			lock (this.weaponLock)
			{
				return this.weaponModelId.Set;
			}
		}
		set
		{
			lock (this.weaponLock)
			{
				this.weaponModelId.Set = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(WeaponModelId))]
	public ushort Base
	{
		get
		{
			lock (this.weaponLock)
			{
				return this.weaponModelId.Base;
			}
		}
		set
		{
			lock (this.weaponLock)
			{
				this.weaponModelId.Base = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(WeaponModelId))]
	public ushort Variant
	{
		get
		{
			lock (this.weaponLock)
			{
				return this.weaponModelId.Variant;
			}
		}
		set
		{
			lock (this.weaponLock)
			{
				this.weaponModelId.Variant = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(WeaponModelId))]
	public byte Dye
	{
		get
		{
			lock (this.weaponLock)
			{
				return this.weaponModelId.Dye;
			}
		}
		set
		{
			lock (this.weaponLock)
			{
				this.weaponModelId.Dye = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(WeaponModelId))]
	public byte Dye2
	{
		get
		{
			lock (this.weaponLock)
			{
				return this.weaponModelId.Dye2;
			}
		}
		set
		{
			lock (this.weaponLock)
			{
				this.weaponModelId.Dye2 = value;
			}
		}
	}

	[Bind(WeaponData.DRAW_OBJECT_OFFSET, BindFlags.Pointer)] public WeaponModelMemory? Model { get; set; }
	[Bind(WeaponData.STATE_OFFSET)] public WeaponStateFlags State { get; set; }

	public IItem? EquippedItem { get; set; }

	[DependsOn(nameof(State))]
	public bool WeaponHidden
	{
		get => this.State.HasFlagUnsafe(WeaponStateFlags.Hidden);
		set
		{
			if (value)
			{
				this.State |= WeaponStateFlags.Hidden;
			}
			else
			{
				this.State &= ~WeaponStateFlags.Hidden;
			}

			if (this.Model != null)
			{
				if (value)
				{
					this.Model.Flags = (byte)(this.Model.Flags & ~(byte)DrawObjectFlags.Visible);
				}
				else
				{
					// When unhiding, if weapon scale is set to 0 by the user, reset it to 1 so that the weapon is visible again.
					this.Model.Flags |= (byte)DrawObjectFlags.Visible;
					if (this.Model.Transform != null && this.Model.Transform.Scale == Vector3.Zero)
					{
						this.Model.Transform.Scale = Vector3.One;
					}
				}
			}
		}
	}

	public void Clear(bool isPlayer)
	{
		if (GposeService.InstanceOrNull?.IsGpose != true)
			return;

		bool useEmperorsFists = true;

		if (this.Parent is ActorMemory actor)
		{
			if (actor.DrawData.OffHand == this && actor.DrawData.MainHand != null)
			{
				IItem? mainHandItem = ItemUtility.GetItem(ItemSlots.MainHand, actor.DrawData.MainHand.Set, actor.DrawData.MainHand.Base, actor.DrawData.MainHand.Variant, actor.IsChocobo);

				if (mainHandItem != null &&
					(mainHandItem.EquipableClasses.HasFlagUnsafe(Classes.Pugilist) ||
					mainHandItem.EquipableClasses.HasFlagUnsafe(Classes.Monk)))
				{
					useEmperorsFists = true;
				}
				else
				{
					useEmperorsFists = false;
				}
			}
		}

		this.Set = useEmperorsFists ? ItemUtility.EmperorsNewFists.ModelSet : (ushort)0;
		this.Base = useEmperorsFists ? ItemUtility.EmperorsNewFists.ModelBase : (ushort)0;
		this.Variant = useEmperorsFists ? ItemUtility.EmperorsNewFists.ModelVariant : (ushort)0;
		this.Dye = 0;
		this.Dye2 = 0;
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
