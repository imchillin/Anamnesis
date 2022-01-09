// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private const ushort ResetAnimationId = 0;
		private const ushort DrawWeaponAnimationid = 190;
		private const byte AnimationOverrideMode = 0x8;

		private NopHookViewModel? animationSpeedHook;

		private bool isEnabled = false;

		public bool Enabled
		{
			get => this.isEnabled;
			set
			{
				if (this.isEnabled != value)
				{
					this.SetEnabled(value);
				}
			}
		}

		public override Task Start()
		{
			GposeService.GposeStateChanging += this.GposeService_GposeStateChanging;
			PoseService.EnabledChanged += this.PoseService_EnabledChanged;

			this.animationSpeedHook = new NopHookViewModel(AddressService.AnimationSpeedPatch, 0x9);

			this.AutoUpdateEnabledStatus();

			return base.Start();
		}

		public void ApplyAnimationOverride(ActorMemory actor, ushort? animationId, float? animationSpeed, bool interrupt)
		{
			MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.AnimationMode)), AnimationOverrideMode, "Animation Mode Override");

			if (animationSpeed != null && actor.AnimationSpeed != animationSpeed)
			{
				MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.AnimationSpeed)), animationSpeed, "Animation Speed Override");
			}

			if (animationId != null && actor.AnimationOverride != animationId)
			{
				if (animationId < GameDataService.ActionTimelines.RowCount)
				{
					MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.AnimationOverride)), animationId, "Animation ID Override");
				}
			}

			if (interrupt)
			{
				MemoryService.Write<ushort>(actor.GetAddressOfProperty(nameof(ActorMemory.TargetAnimation)), 0, "Animation Interrupt");
			}
		}

		public void ResetAnimationOverride(ActorMemory actor) => this.ApplyAnimationOverride(actor, ResetAnimationId, 1, true);

		public void DrawWeapon(ActorMemory actor) => this.ApplyAnimationOverride(actor, DrawWeaponAnimationid, null, true);

		public override async Task Shutdown()
		{
			GposeService.GposeStateChanging -= this.GposeService_GposeStateChanging;
			PoseService.EnabledChanged -= this.PoseService_EnabledChanged;

			this.Enabled = false;

			await base.Shutdown();
		}

		private void SetEnabled(bool enabled)
		{
			if (this.isEnabled == enabled)
				return;

			this.isEnabled = enabled;

			if (enabled)
			{
				this.animationSpeedHook?.SetEnabled(true);
			}
			else
			{
				this.animationSpeedHook?.SetEnabled(false);
			}
		}

		private void AutoUpdateEnabledStatus()
		{
			this.Enabled = GposeService.Instance.IsGpose && !PoseService.Instance.IsEnabled;
		}

		private void GposeService_GposeStateChanging() => this.AutoUpdateEnabledStatus();

		private void PoseService_EnabledChanged(bool value) => this.AutoUpdateEnabledStatus();
	}
}
