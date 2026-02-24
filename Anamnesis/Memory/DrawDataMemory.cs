// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core.Extensions;
using PropertyChanged;
using System;

public class DrawDataMemory : MemoryBase
{
	[Flags]
	public enum CharacterFlagDefs : byte
	{
		None = 0,
		WeaponsVisible = 1 << 0,
		WeaponsDrawn = 1 << 2,
		VisorToggled = 1 << 4,
		HeadgearEarsHidden = 1 << 5,
	}

	[Bind(0x010)] public WeaponMemory? MainHand { get; set; }
	[Bind(0x080)] public WeaponMemory? OffHand { get; set; }
	[Bind(0x01D0)] public ActorEquipmentMemory? Equipment { get; set; }
	[Bind(0x0220)] public ActorCustomizeMemory? Customize { get; set; }
	[Bind(0x023E, BindFlags.ActorRefresh)] public bool HatHidden { get; set; }
	[Bind(0x023F, BindFlags.ActorRefresh)] public CharacterFlagDefs CharacterFlags { get; set; }
	[Bind(0x0240)] public GlassesMemory? Glasses { get; set; }

	[DependsOn(nameof(CharacterFlags))]
	public bool VisorToggled
	{
		get => this.CharacterFlags.HasFlagUnsafe(CharacterFlagDefs.VisorToggled);
		set
		{
			if (value)
			{
				this.CharacterFlags |= CharacterFlagDefs.VisorToggled;
			}
			else
			{
				this.CharacterFlags &= ~CharacterFlagDefs.VisorToggled;
			}
		}
	}

	[DependsOn(nameof(CharacterFlags))]
	public bool HeadgearEarsHidden
	{
		get => this.CharacterFlags.HasFlagUnsafe(CharacterFlagDefs.HeadgearEarsHidden);
		set
		{
			if (value)
			{
				this.CharacterFlags |= CharacterFlagDefs.HeadgearEarsHidden;
			}
			else
			{
				this.CharacterFlags &= ~CharacterFlagDefs.HeadgearEarsHidden;
			}
		}
	}
}
