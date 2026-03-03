// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis;
using Anamnesis.Core;
using PropertyChanged;
using RemoteController.IPC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class PoseService : ServiceBase<PoseService>
{
	private bool isEnabled;

	public delegate void PoseEvent(bool value);

	public static event PoseEvent? EnabledChanged;
	public static event PoseEvent? FreezeWorldStateEnabledChanged;

	public static string? SelectedBonesText { get; set; }

	public bool IsEnabled
	{
		get => this.isEnabled;
		set
		{
			if (this.isEnabled == value)
				return;

			this.SetEnabled(value);
		}
	}

	public bool FreezePhysics
	{
		get => ControllerService.Instance.SendDriverCommand<bool>(DriverCommand.GetFreezePhysics) ?? false;
		set => ControllerService.Instance.SendDriverCommand<bool>(DriverCommand.SetFreezePhysics, value);
	}

	public bool WorldStateNotFrozen => !this.FreezeWorldState;

	public bool FreezeWorldState
	{
		get => ControllerService.Instance.SendDriverCommand<bool>(DriverCommand.GetFreezeWorldVisualState) ?? false;
		set
		{
			ControllerService.Instance.SendDriverCommand<bool>(DriverCommand.SetFreezeWorldVisualState, value);
			this.RaisePropertyChanged(nameof(this.FreezeWorldState));
			this.RaisePropertyChanged(nameof(this.WorldStateNotFrozen));
			FreezeWorldStateEnabledChanged?.Invoke(value);
		}
	}

	public bool EnableParenting { get; set; } = true;

	public bool CanEdit { get; set; }

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance, ControllerService.Instance, GposeService.Instance];

	public override async Task Shutdown()
	{
		GposeService.GposeStateChanged -= this.OnGposeStateChanged;
		await base.Shutdown();
	}

	public void SetEnabled(bool enabled)
	{
		// Don't try to enable posing unless we are in gpose
		if (enabled && !GposeService.Instance.IsGpose)
			throw new Exception("Attempt to enable posing outside of gpose");

		if (this.isEnabled == enabled)
			return;

		// Send command to remote controller
		bool? result = ControllerService.Instance.SendDriverCommand<bool>(DriverCommand.SetPosingEnabled, enabled);
		if (result != true)
		{
			Log.Warning($"Failed to {(enabled ? "enable" : "disable")} posing via remote controller.");
			return;
		}

		this.isEnabled = enabled;

		// Freeze physics when posing is enabled
		this.FreezePhysics = enabled;
		this.EnableParenting = true;

		EnabledChanged?.Invoke(enabled);
		this.RaisePropertyChanged(nameof(this.IsEnabled));
	}

	protected override async Task OnStart()
	{
		await base.OnStart();
		GposeService.GposeStateChanged += this.OnGposeStateChanged;
	}

	private void OnGposeStateChanged(bool isGPose)
	{
		if (!isGPose)
		{
			this.isEnabled = false;
			EnabledChanged?.Invoke(false);
			this.RaisePropertyChanged(nameof(this.IsEnabled));
			this.RaisePropertyChanged(nameof(this.FreezeWorldState));
			this.RaisePropertyChanged(nameof(this.WorldStateNotFrozen));
		}
	}
}
