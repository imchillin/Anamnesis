// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private const ushort DrawWeaponAnimationid = 190;

		private NopHookViewModel? animationSpeedHook;

		private bool speedControlEnabled = false;

		public bool SpeedControlEnabled
		{
			get => this.speedControlEnabled;
			set
			{
				if (this.speedControlEnabled != value)
				{
					this.SetSpeedControlEnabled(value);
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

		public void PlayAnimation(ActorMemory actor, ushort? animationId, float? animationSpeed, bool interrupt) => this.ApplyAnimationOverride(actor, animationId, animationSpeed, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
		public void ResetAnimationOverride(ActorMemory actor) => this.ApplyAnimationOverride(actor, 0, 1f, true, ActorMemory.CharacterModes.Normal, 0);
		public void DrawWeapon(ActorMemory actor) => this.ApplyAnimationOverride(actor, DrawWeaponAnimationid, null, true, ActorMemory.CharacterModes.AnimLock, 0);
		public void StopAnimation(ActorMemory actor) => this.ApplyAnimationOverride(actor, null, 0.0f, false, ActorMemory.CharacterModes.EmoteLoop, 0);

		public override async Task Shutdown()
		{
			GposeService.GposeStateChanging -= this.GposeService_GposeStateChanging;
			PoseService.EnabledChanged -= this.PoseService_EnabledChanged;

			this.SpeedControlEnabled = false;

			await base.Shutdown();
		}

		private void ApplyAnimationOverride(ActorMemory actor, ushort? animationId, float? animationSpeed, bool interrupt, ActorMemory.CharacterModes mode, byte modeInput)
		{
			MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.CharacterMode)), mode, "Animation Mode Override");
			MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput, "Animation Mode Override");

			if (this.SpeedControlEnabled && animationSpeed != null && actor.AnimationSpeed != animationSpeed)
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

		private void SetSpeedControlEnabled(bool enabled)
		{
			if (this.speedControlEnabled == enabled)
				return;

			this.speedControlEnabled = enabled;

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
			this.SpeedControlEnabled = GposeService.Instance.IsGpose;
		}

		private void GposeService_GposeStateChanging() => this.AutoUpdateEnabledStatus();

		private void PoseService_EnabledChanged(bool value) => this.AutoUpdateEnabledStatus();
	}
}
