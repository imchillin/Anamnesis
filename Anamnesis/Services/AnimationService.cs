// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private const int TickDelay = 1000;
		private const ushort DrawWeaponAnimationId = 34;
		private const ushort IdleAnimationId = 3;

		private readonly HashSet<ActorMemory> overriddenActors = new();

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

		public bool BlendLocked { get; private set; }

		public override Task Start()
		{
			GposeService.GposeStateChanging += this.OnGposeStateChanging;

			this.animationSpeedHook = new NopHookViewModel(AddressService.AnimationSpeedPatch, 0x9);

			this.OnGposeStateChanging(GposeService.Instance.IsGpose);

			_ = Task.Run(this.CheckThread);

			return base.Start();
		}

		public override async Task Shutdown()
		{
			GposeService.GposeStateChanging -= this.OnGposeStateChanging;

			this.SpeedControlEnabled = false;

			this.ResetOverriddenActors();

			await base.Shutdown();
		}

		public void ApplyAnimationOverride(ActorMemory memory, ushort? animationId, bool interrupt)
		{
			if (!memory.IsValid)
				return;

			this.ApplyAnimation(memory, animationId, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
			this.overriddenActors.Add(memory);
		}

		public void BlendAnimation(ActorMemory memory, ushort animationId)
		{
			if (this.BlendLocked)
				return;

			if (!memory.IsValid)
				return;

			this.BlendLocked = true;

			Task.Run(async () =>
			{
				var oldAnim = memory.BaseAnimationOverride;
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.BaseAnimationOverride)), animationId, "Blend Animation ID Override");
				await Task.Delay(66);
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.BaseAnimationOverride)), oldAnim, "Blend Animation ID Override");
				this.BlendLocked = false;
			});
		}

		public void ResetAnimationOverride(ActorMemory memory)
		{
			if (!memory.IsValid)
				return;

			this.ApplyAnimation(memory, 0, true, ActorMemory.CharacterModes.Normal, 0);

			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.LipAnimationOverride)), 0, "Blend Animation ID Override");

			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.BaseAnimationSpeedInternal)), 1.0f, "Animation Speed Reset");
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.LipAnimationSpeedInternal)), 1.0f, "Animation Speed Reset");
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.AnimationSpeedTrigger)), 1.0f, "Animation Speed Reset");

			this.overriddenActors.Remove(memory);
		}

		public void ApplyIdle(ActorMemory memory) => this.ApplyAnimationOverride(memory, IdleAnimationId, true);

		public void DrawWeapon(ActorMemory memory) => this.ApplyAnimationOverride(memory, DrawWeaponAnimationId, true);

		private async Task CheckThread()
		{
			while (this.IsAlive)
			{
				await Task.Delay(TickDelay);

				this.CleanupInvalidActors();
			}
		}

		private void ApplyAnimation(ActorMemory memory, ushort? animationId, bool interrupt, ActorMemory.CharacterModes? mode, byte? modeInput)
		{
			if (animationId != null && memory.BaseAnimationOverride != animationId)
			{
				if (animationId < GameDataService.ActionTimelines.RowCount)
				{
					MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.BaseAnimationOverride)), animationId, "Animation ID Override");
				}
			}

			// Always set the input before the mode
			if (modeInput != null && memory.CharacterModeInput != modeInput)
			{
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput, "Animation Mode Input Override");
			}

			if (mode != null && memory.CharacterMode != mode)
			{
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeRaw)), mode, "Animation Mode Override");
			}

			if (interrupt)
			{
				MemoryService.Write<ushort>(memory.GetAddressOfProperty(nameof(ActorMemory.TargetAnimation)), 0, "Animation Interrupt");
			}

			memory.Tick();
		}

		private void SetSpeedControlEnabled(bool enabled)
		{
			if (this.speedControlEnabled == enabled)
				return;

			if (enabled)
			{
				this.animationSpeedHook?.SetEnabled(true);
			}
			else
			{
				this.animationSpeedHook?.SetEnabled(false);
			}

			this.speedControlEnabled = enabled;
		}

		private void OnGposeStateChanging(bool isGPose) => this.SpeedControlEnabled = isGPose;

		private void ResetOverriddenActors()
		{
			foreach (var actor in this.overriddenActors.ToList())
			{
				if(actor.IsValid && actor.IsAnimationOverridden)
				{
					this.ResetAnimationOverride(actor);
				}
			}
		}

		private void CleanupInvalidActors()
		{
			var stale = this.overriddenActors.Where(actor => !actor.IsValid).ToList();
			foreach (var actorRef in stale)
			{
				this.overriddenActors.Remove(actorRef);
			}
		}
	}
}
