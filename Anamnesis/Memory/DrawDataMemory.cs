// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core.Extensions;
using PropertyChanged;
using RemoteController.IPC;
using System;

public class DrawDataMemory : MemoryBase
{
	private const int DRIVER_COMMAND_TIMEOUT_MS = 1000;

	private bool hatHidden;

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

	[Bind(0x023E)]
	public bool HatHidden
	{
		get => this.hatHidden;
		set
		{
			bool? remoteResult = null;
			try
			{
				remoteResult = ControllerService.Instance?.SendDriverCommand<bool>(
					DriverCommand.ActorHideHeadgear, args: [this.Address, value ? (byte)1 : (byte)0], timeout: DRIVER_COMMAND_TIMEOUT_MS);
			}
			catch
			{
				Log.Warning($"Failed to send command {DriverCommand.ActorHideHeadgear}. Falling back to local state update.");
			}

			if (remoteResult.HasValue)
			{
				this.hatHidden = value;
			}
			else // Fallback: Do a direct memory write and trigger a forced actor redraw
			{
				this.hatHidden = value;
				if (this.Parent is ActorMemory actor)
				{
					_ = actor.Refresh(forceReload: true);
				}
			}

			this.OnPropertyChanged(nameof(this.HatHidden));
		}
	}

	[Bind(0x023F, BindFlags.ActorRefresh)] public CharacterFlagDefs CharacterFlags { get; set; }
	[Bind(0x0240)] public GlassesMemory? Glasses { get; set; }

	[DependsOn(nameof(CharacterFlags))]
	public bool VisorToggled
	{
		get => this.CharacterFlags.HasFlagUnsafe(CharacterFlagDefs.VisorToggled);
		set
		{
			bool? remoteResult = null;
			try
			{
				remoteResult = ControllerService.Instance?.SendDriverCommand<bool>(
					DriverCommand.ActorSetVisor, args: [this.Address, value ? (byte)1 : (byte)0], timeout: DRIVER_COMMAND_TIMEOUT_MS);
			}
			catch
			{
				Log.Warning($"Failed to send command {DriverCommand.ActorSetVisor}. Falling back to local state update.");
			}

			if (remoteResult == true)
				return;

			// Fallback
			if (value)
				this.CharacterFlags |= CharacterFlagDefs.VisorToggled;
			else
				this.CharacterFlags &= ~CharacterFlagDefs.VisorToggled;

			// Trigger full redraw
			if (this.Parent is ActorMemory actor)
				_ = actor.Refresh(forceReload: true);

			this.OnPropertyChanged(nameof(this.CharacterFlags));
		}
	}

	[DependsOn(nameof(CharacterFlags))]
	public bool HeadgearEarsHidden
	{
		get => this.CharacterFlags.HasFlagUnsafe(CharacterFlagDefs.HeadgearEarsHidden);
		set
		{
			bool? remoteResult = null;
			try
			{
				remoteResult = ControllerService.Instance?.SendDriverCommand<bool>(
					DriverCommand.ActorHideVieraEars, args: [this.Address, value ? (byte)1 : (byte)0], timeout: DRIVER_COMMAND_TIMEOUT_MS);
			}
			catch
			{
				Log.Warning($"Failed to send command {DriverCommand.ActorHideVieraEars}. Falling back to local state update.");
			}

			if (remoteResult == true)
				return;

			// Fallback
			if (value)
				this.CharacterFlags |= CharacterFlagDefs.HeadgearEarsHidden;
			else
				this.CharacterFlags &= ~CharacterFlagDefs.HeadgearEarsHidden;

			// Trigger full redraw
			if (this.Parent is ActorMemory actor)
				_ = actor.Refresh(forceReload: true);

			this.OnPropertyChanged(nameof(this.CharacterFlags));
		}
	}
}
