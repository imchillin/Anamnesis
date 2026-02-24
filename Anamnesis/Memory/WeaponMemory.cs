// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using PropertyChanged;
using RemoteController.Interop.Types;
using System;
using System.Numerics;
using static Anamnesis.Actor.Utilities.DyeUtility;

// TODO: Same as ItemMemory, we could read/write this as one object to reduce the number of external process reads/writes during sync.
// TODO: Convert this to use WeaponModelId.
public class WeaponMemory : MemoryBase, IEquipmentItemMemory
{
	[Flags]
	public enum WeaponFlagDefs : byte
	{
		WeaponHidden = 1 << 1,
	}

	[Bind(0x000, BindFlags.ActorRefresh | BindFlags.WeaponRefresh)] public ushort Set { get; set; }
	[Bind(0x002, BindFlags.ActorRefresh | BindFlags.WeaponRefresh)] public ushort Base { get; set; }
	[Bind(0x004, BindFlags.ActorRefresh | BindFlags.WeaponRefresh)] public ushort Variant { get; set; }
	[Bind(0x006, BindFlags.ActorRefresh | BindFlags.WeaponRefresh)] public byte Dye { get; set; }
	[Bind(0x007, BindFlags.ActorRefresh | BindFlags.WeaponRefresh)] public byte Dye2 { get; set; }
	[Bind(0x018, BindFlags.Pointer)] public WeaponModelMemory? Model { get; set; }
	[Bind(0x040)] public bool IsSheathed { get; set; }
	[Bind(0x060)] public WeaponFlagDefs WeaponFlags { get; set; }

	[DependsOn(nameof(WeaponFlags), nameof(IsSheathed))]
	public bool WeaponHidden
	{
		get => (this.IsSheathed && this.WeaponFlags.HasFlagUnsafe(WeaponFlagDefs.WeaponHidden)) || (!this.IsSheathed && this.Model?.Transform?.Scale == Vector3.Zero);
		set
		{
			if (value)
			{
				this.WeaponFlags |= WeaponFlagDefs.WeaponHidden;
			}
			else
			{
				this.WeaponFlags &= ~WeaponFlagDefs.WeaponHidden;
			}

			if (this.Model?.Transform == null)
				return;

			// If the weapon is unsheathed (in hands) the visibility flag won't work,
			// so fall back to setting the weapons scale to 0.
			if (!this.IsSheathed)
			{
				this.Model.Transform.Scale = value ? Vector3.Zero : Vector3.One;
			}

			// Special handling for a weapon with 0 scale that has been sheathed attempting to un-hide
			else if (!value && this.Model.Transform.Scale == Vector3.Zero)
			{
				this.Model.Transform.Scale = Vector3.One;
			}
		}
	}

	public void Clear(bool isPlayer)
	{
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

	public WeaponModelId ToModelId()
	{
		return new WeaponModelId
		{
			Set = this.Set,
			Base = this.Base,
			Variant = this.Variant,
			Dye1 = this.Dye,
			Dye2 = this.Dye2,
		};
	}
}
