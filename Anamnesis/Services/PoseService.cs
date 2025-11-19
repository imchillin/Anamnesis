// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis;
using Anamnesis.Core;
using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class PoseService : ServiceBase<PoseService>
{
	private NopHook? freezeRot1;                // SyncModelSpace
	private NopHook? freezeRot2;                // CalculateBoneModelSpace
	private NopHook? freezeRot3;                // hkaLookAtIkSolve
	private NopHook? freezeScale1;              // SyncModelSpace
	private NopHook? freeseScale2;              // CalculateBoneModelSpace
	private NopHook? freezePosition;            // SyncModelSpace
	private NopHook? freezePosition2;           // CalculateBoneModelSpace
	private NopHook? freezePhysics1;            // Rotation
	private NopHook? freezePhysics2;            // Position
	private NopHook? freezePhysics3;            // Scale
	private NopHook? freezeWorldPosition;
	private NopHook? freezeWorldRotation;
	private NopHook? freezeGposeTargetPosition;
	private NopHook? kineDriverPosition;
	private NopHook? kineDriverRotation;
	private NopHook? kineDriverScale;

	private bool isEnabled;

	public delegate void PoseEvent(bool value);

	public static event PoseEvent? EnabledChanged;
	public static event PoseEvent? FreezeWorldPositionsEnabledChanged;

	public static string? SelectedBonesText { get; set; }

	public bool IsEnabled
	{
		get
		{
			return this.isEnabled;
		}

		set
		{
			if (this.IsEnabled == value)
				return;

			this.SetEnabled(value);
		}
	}

	public bool FreezePhysics
	{
		get
		{
			return this.freezePhysics1?.Enabled ?? false;
		}
		set
		{
			if (value)
			{
				this.freezePhysics2?.SetEnabled(value);
				this.freezePhysics1?.SetEnabled(value);
			}
			else
			{
				this.freezePhysics1?.SetEnabled(value);
				this.freezePhysics2?.SetEnabled(value);
			}
		}
	}

	public bool FreezePositions
	{
		get
		{
			return this.freezePosition?.Enabled ?? false;
		}
		set
		{
			if (value)
			{
				this.freezePosition?.SetEnabled(value);
				this.freezePosition2?.SetEnabled(value);
				this.kineDriverPosition?.SetEnabled(value);
			}
			else
			{
				this.kineDriverPosition?.SetEnabled(value);
				this.freezePosition2?.SetEnabled(value);
				this.freezePosition?.SetEnabled(value);
			}
		}
	}

	public bool FreezeScale
	{
		get
		{
			return this.freezeScale1?.Enabled ?? false;
		}
		set
		{
			if (value)
			{
				this.freezePhysics3?.SetEnabled(value);
				this.freeseScale2?.SetEnabled(value);
				this.freezeScale1?.SetEnabled(value);
				this.kineDriverScale?.SetEnabled(value);
			}
			else
			{
				this.kineDriverScale?.SetEnabled(value);
				this.freezeScale1?.SetEnabled(value);
				this.freeseScale2?.SetEnabled(value);
				this.freezePhysics3?.SetEnabled(value);
			}
		}
	}

	public bool FreezeRotation
	{
		get
		{
			return this.freezeRot1?.Enabled ?? false;
		}
		set
		{
			if (value)
			{
				this.freezeRot2?.SetEnabled(value);
				this.freezeRot1?.SetEnabled(value);
				this.freezeRot3?.SetEnabled(value);
				this.kineDriverRotation?.SetEnabled(value);
			}
			else
			{
				this.kineDriverRotation?.SetEnabled(value);
				this.freezeRot3?.SetEnabled(value);
				this.freezeRot1?.SetEnabled(value);
				this.freezeRot2?.SetEnabled(value);
			}
		}
	}

	public bool WorldPositionNotFrozen => !this.FreezeWorldPosition;

	public bool FreezeWorldPosition
	{
		get
		{
			return this.freezeWorldPosition?.Enabled ?? false;
		}
		set
		{
			this.freezeWorldPosition?.SetEnabled(value);
			this.freezeWorldRotation?.SetEnabled(value);
			this.freezeGposeTargetPosition?.SetEnabled(value);
			this.RaisePropertyChanged(nameof(this.FreezeWorldPosition));
			this.RaisePropertyChanged(nameof(this.WorldPositionNotFrozen));
			FreezeWorldPositionsEnabledChanged?.Invoke(this.IsEnabled);
		}
	}

	public bool EnableParenting { get; set; } = true;

	public bool CanEdit { get; set; }

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance, GposeService.Instance];

	public override async Task Shutdown()
	{
		GposeService.GposeStateChanged -= this.OnGposeStateChanged;
		this.SetEnabled(false);
		this.FreezeWorldPosition = false;
		await base.Shutdown();
	}

	public void SetEnabled(bool enabled)
	{
		// Don't try to enable posing unless we are in gpose
		if (enabled && !GposeService.Instance.IsGpose)
			throw new Exception("Attempt to enable posing outside of gpose");

		if (this.isEnabled == enabled)
			return;

		this.isEnabled = enabled;
		this.FreezeRotation = enabled;
		this.FreezePositions = enabled;
		this.FreezePhysics = enabled;
		this.FreezeScale = false;
		this.EnableParenting = true;

		/*if (enabled)
		{
			this.FreezeWorldPosition = true;
			AnimationService.Instance.PausePinnedActors();
		}*/

		EnabledChanged?.Invoke(enabled);

		this.RaisePropertyChanged(nameof(this.IsEnabled));
	}

	protected override async Task OnStart()
	{
		this.freezePosition = new NopHook(AddressService.SkeletonFreezePosition, 5);
		this.freezePosition2 = new NopHook(AddressService.SkeletonFreezePosition2, 5);
		this.freezeRot1 = new NopHook(AddressService.SkeletonFreezeRotation, 6);
		this.freezeRot2 = new NopHook(AddressService.SkeletonFreezeRotation2, 6);
		this.freezeRot3 = new NopHook(AddressService.SkeletonFreezeRotation3, 4);
		this.freezeScale1 = new NopHook(AddressService.SkeletonFreezeScale, 6);
		this.freeseScale2 = new NopHook(AddressService.SkeletonFreezeScale2, 6);
		this.freezePhysics1 = new NopHook(AddressService.SkeletonFreezePhysics, 4);
		this.freezePhysics2 = new NopHook(AddressService.SkeletonFreezePhysics2, 3);
		this.freezePhysics3 = new NopHook(AddressService.SkeletonFreezePhysics3, 4);
		this.freezeWorldPosition = new NopHook(AddressService.WorldPositionFreeze, 16);
		this.freezeWorldRotation = new NopHook(AddressService.WorldRotationFreeze, 4);
		this.freezeGposeTargetPosition = new NopHook(AddressService.GPoseCameraTargetPositionFreeze, 5);
		this.kineDriverPosition = new NopHook(AddressService.KineDriverPosition, 5);
		this.kineDriverRotation = new NopHook(AddressService.KineDriverRotation, 6);
		this.kineDriverScale = new NopHook(AddressService.KineDriverScale, 6);

		await base.OnStart();

		GposeService.GposeStateChanged += this.OnGposeStateChanged;
	}

	private void OnGposeStateChanged(bool isGPose)
	{
		if (!isGPose)
		{
			this.SetEnabled(false);
			this.FreezeWorldPosition = false;
		}
	}
}
