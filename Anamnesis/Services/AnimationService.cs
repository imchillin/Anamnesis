// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private const ushort ResetAnimationId = 3;
		private const ushort DrawWeaponAnimationid = 190;

		private readonly byte[] originalPatchBytes = new byte[0x7];
		private readonly byte[] replacementPatchBytes = new byte[0x7];

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

			this.animationSpeedHook = new NopHookViewModel(AddressService.AnimationSpeedPatch, 0x9);

			MemoryService.Read(AddressService.AnimationOverridePatch, this.originalPatchBytes, this.originalPatchBytes.Length);
			Array.Copy(this.originalPatchBytes, this.replacementPatchBytes, this.replacementPatchBytes.Length);
			this.replacementPatchBytes[this.replacementPatchBytes.Length - 1] = (byte)0x2;

			this.GposeService_GposeStateChanging();

			return base.Start();
		}

		public void ApplyAnimationOverride(ActorMemory actor, ushort? animationId, float? animationSpeed, bool interrupt)
		{
			if (animationSpeed != null && actor.AnimationSpeed != animationSpeed)
			{
				MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.AnimationSpeed)), animationSpeed, "Animation Override");
			}

			if (animationId != null && actor.AnimationOverride != animationId)
			{
				if (animationId < GameDataService.ActionTimelines.RowCount)
				{
					MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.AnimationOverride)), animationId, "Animation Override");
				}
			}

			if (interrupt)
			{
				MemoryService.Write<ushort>(actor.GetAddressOfProperty(nameof(ActorMemory.TargetAnimation)), 0, "Animation Override");
			}
		}

		public void ResetAnimationOverride(ActorMemory actor) => this.ApplyAnimationOverride(actor, ResetAnimationId, 1, true);

		public void DrawWeapon(ActorMemory actor) => this.ApplyAnimationOverride(actor, DrawWeaponAnimationid, null, true);

		public override async Task Shutdown()
		{
			this.Enabled = false;

			await base.Shutdown();
		}

		private void SetEnabled(bool enabled)
		{
			if (this.isEnabled == enabled)
				return;

			if (enabled && !GposeService.Instance.IsGpose)
				return;

			this.isEnabled = enabled;

			if (enabled)
			{
				this.animationSpeedHook?.SetEnabled(true);
				MemoryService.Write(AddressService.AnimationOverridePatch, this.replacementPatchBytes);
			}
			else
			{
				this.animationSpeedHook?.SetEnabled(false);
				MemoryService.Write(AddressService.AnimationOverridePatch, this.originalPatchBytes);
			}
		}

		private void GposeService_GposeStateChanging()
		{
			this.Enabled = GposeService.Instance.IsGpose;
		}
	}
}
