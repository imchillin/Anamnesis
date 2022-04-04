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

		public void ApplyAnimationOverride(ActorMemory memory, ushort? animationId, float? animationSpeed, bool interrupt)
		{
			if (!memory.IsValid)
				return;

			this.ApplyAnimation(memory, animationId, animationSpeed, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
			this.overriddenActors.Add(memory);
		}

		public void ResetAnimationOverride(ActorMemory memory)
		{
			if (!memory.IsValid)
				return;

			this.ApplyAnimation(memory, 0, 1.0f, true, ActorMemory.CharacterModes.Normal, 0);
			this.overriddenActors.Remove(memory);
		}

		public void PauseActor(ActorMemory memory)
		{
			if (!memory.IsValid)
				return;

			var oldMode = memory.CharacterMode;
			var oldModeInput = memory.CharacterModeInput;

			Task.Run(() =>
			{
				this.ApplyAnimation(memory, null, 0.0f, false, ActorMemory.CharacterModes.EmoteLoop, 0);
				Thread.Sleep(100);
				this.ApplyAnimation(memory, null, 0.0f, false, oldMode, oldModeInput);
			});
		}

		public void UnpauseActor(ActorMemory memory)
		{
			if (!memory.IsValid)
				return;

			var oldMode = memory.CharacterMode;
			var oldModeInput = memory.CharacterModeInput;

			Task.Run(() =>
			{
				this.ApplyAnimation(memory, null, 0.0f, false, ActorMemory.CharacterModes.EmoteLoop, 0);
				Thread.Sleep(100);
				this.ApplyAnimation(memory, null, 1.0f, false, oldMode, oldModeInput);
			});
		}

		public void ApplyIdle(ActorMemory memory) => this.ApplyAnimationOverride(memory, IdleAnimationId, 1.0f, true);

		public void DrawWeapon(ActorMemory memory) => this.ApplyAnimationOverride(memory, DrawWeaponAnimationId, 1.0f, true);

		private async Task CheckThread()
		{
			while (this.IsAlive)
			{
				await Task.Delay(TickDelay);

				this.CleanupInvalidActors();
			}
		}

		private void ApplyAnimation(ActorMemory memory, ushort? animationId, float? animationSpeed, bool interrupt, ActorMemory.CharacterModes? mode, byte? modeInput)
		{
			if (this.SpeedControlEnabled && animationSpeed != null && memory.AnimationSpeed != animationSpeed)
			{
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.AnimationSpeed)), animationSpeed, "Animation Speed Override");
			}

			if (animationId != null && memory.AnimationOverride != animationId)
			{
				if (animationId < GameDataService.ActionTimelines.RowCount)
				{
					MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.AnimationOverride)), animationId, "Animation ID Override");
				}
			}

			// Always set the input before the mode
			if (modeInput != null && memory.CharacterModeInput != modeInput)
			{
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput, "Animation Mode Input Override");
			}

			if (mode != null && memory.CharacterMode != mode)
			{
				MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterMode)), mode, "Animation Mode Override");
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
				if(actor.IsValid && actor.IsAnimationOverriden)
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
