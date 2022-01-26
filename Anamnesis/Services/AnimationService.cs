// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Utils;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private const int TickDelay = 1000;
		private const ushort DrawWeaponAnimationId = 34;
		private const ushort IdleAnimationId = 3;

		private readonly ConcurrentDictionary<ActorRef<ActorMemory>, AnimationState> animationStates = new();

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
			TerritoryService.TerritoryChanged += this.TerritoryService_TerritoryChanged;

			this.animationSpeedHook = new NopHookViewModel(AddressService.AnimationSpeedPatch, 0x9);

			this.GposeService_GposeStateChanging();

			_ = Task.Run(this.CheckThread);

			return base.Start();
		}

		public override async Task Shutdown()
		{
			GposeService.GposeStateChanging -= this.GposeService_GposeStateChanging;
			TerritoryService.TerritoryChanged -= this.TerritoryService_TerritoryChanged;

			this.SpeedControlEnabled = false;

			this.CleanupAllAnimationOverrides();

			await base.Shutdown();
		}

		public void DrawWeapon(ActorRef<ActorMemory> actor) => this.PlayAnimation(actor, DrawWeaponAnimationId, 1.0f, true);
		public void IdleCharacter(ActorRef<ActorMemory> actor) => this.PlayAnimation(actor, IdleAnimationId, 1.0f, true);

		public void PlayAnimation(ActorRef<ActorMemory> actorRef, ushort? animationId, float? animationSpeed, bool interrupt)
		{
			if (!actorRef.TryGetMemory(out var memory))
				return;

			var animState = this.GetAnimationState(actorRef);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
				this.Unpause(actorRef);

			if (animState.Status == AnimationState.AnimationStatus.Inactive)
			{
				animState.OriginalCharacterMode = memory.CharacterMode;
				animState.OriginalCharacterModeInput = memory.CharacterModeInput;
			}

			animState.Status = AnimationState.AnimationStatus.Active;
			this.ApplyAnimationOverride(memory, animationId, animationSpeed, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
		}

		public void ResetAnimationOverride(ActorRef<ActorMemory> actorRef)
		{
			if (!actorRef.TryGetMemory(out var memory))
				return;

			var animState = this.GetAnimationState(actorRef);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
				this.Unpause(actorRef);

			if (animState.Status == AnimationState.AnimationStatus.Active)
			{
				ActorMemory.CharacterModes mode = animState.OriginalCharacterMode;
				byte modeInput = animState.OriginalCharacterModeInput;

				if (!this.CanSafelyApplyMode(memory, mode, modeInput))
				{
					mode = ActorMemory.CharacterModes.Normal;
					modeInput = 0;
				}

				animState.Status = AnimationState.AnimationStatus.Inactive;
				this.ApplyAnimationOverride(memory, 0, 1f, true, mode, modeInput);
			}
		}

		public void TogglePaused(ActorRef<ActorMemory> actorRef, float newSpeed = 1.0f)
		{
			if (!actorRef.IsValid)
				return;

			var animState = this.GetAnimationState(actorRef);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
			{
				this.Unpause(actorRef, newSpeed);
			}
			else
			{
				this.Pause(actorRef);
			}
		}

		public void Unpause(ActorRef<ActorMemory> actorRef, float newSpeed = 1.0f)
		{
			if (!actorRef.TryGetMemory(out var memory))
				return;

			var animState = this.GetAnimationState(actorRef);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
			{
				ActorMemory.CharacterModes mode = animState.PausedCharacterMode;
				byte modeInput = animState.PausedCharacterModeInput;

				if (!this.CanSafelyApplyMode(memory, mode, modeInput))
				{
					mode = ActorMemory.CharacterModes.Normal;
					modeInput = 0;
				}

				animState.Status = animState.PausedStatus;
				this.ApplyAnimationOverride(memory, null, newSpeed, false, mode, modeInput);
			}
		}

		public void Pause(ActorRef<ActorMemory> actorRef)
		{
			if (!actorRef.TryGetMemory(out var memory))
				return;

			var animState = this.GetAnimationState(actorRef);

			if (animState.Status != AnimationState.AnimationStatus.Paused)
			{
				animState.PausedStatus = animState.Status;
				animState.PausedCharacterMode = memory.CharacterMode;
				animState.PausedCharacterModeInput = memory.CharacterModeInput;
				animState.Status = AnimationState.AnimationStatus.Paused;

				this.ApplyAnimationOverride(memory, null, 0.0f, false, ActorMemory.CharacterModes.EmoteLoop, 0);
			}
		}

		private async Task CheckThread()
		{
			while (this.IsAlive)
			{
				await Task.Delay(TickDelay);

				this.CleanupInvalidOverrides();
			}
		}

		private void CleanupAllAnimationOverrides()
		{
			foreach ((var actorRef, _) in this.animationStates)
			{
					this.ResetAnimationOverride(actorRef);
			}

			this.animationStates.Clear();
		}

		private void CleanupInvalidOverrides()
		{
			var stale = this.animationStates.Select(x => x.Key).Where(actor => !actor.IsValid).ToList();
			foreach (var actorRef in stale)
			{
				this.animationStates.TryRemove(actorRef, out var removedState);
			}
		}

		private void CleanupPaused()
		{
			foreach ((var actorRef, _) in this.animationStates)
			{
				this.Unpause(actorRef);
			}
		}

		private bool CanSafelyApplyMode(ActorMemory actor, ActorMemory.CharacterModes mode, byte modeInput)
		{
			// We do some special handling for mounts and ornaments so we don't crash if an overworld actor dismounted during posing
			if (mode == ActorMemory.CharacterModes.HasAttachment)
			{
				if ((modeInput == 0 && actor.Mount == null) || (modeInput != 0 && actor.Ornament == null))
					return false;
			}

			return true;
		}

		private void ApplyAnimationOverride(ActorMemory memory, ushort? animationId, float? animationSpeed, bool interrupt, ActorMemory.CharacterModes mode, byte modeInput)
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

			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput, "Animation Mode Override"); // Always set the input before the mode
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterMode)), mode, "Animation Mode Override");

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
				this.CleanupPaused();
				this.animationSpeedHook?.SetEnabled(false);
			}

			this.speedControlEnabled = enabled;
		}

		private void GposeService_GposeStateChanging() => this.SpeedControlEnabled = GposeService.Instance.IsGpose;

		private void TerritoryService_TerritoryChanged() => this.CleanupAllAnimationOverrides();

		private AnimationState GetAnimationState(ActorRef<ActorMemory> actorRef)
		{
			AnimationState animState = new();

			if (this.animationStates.TryAdd(actorRef, animState))
			{
				if(actorRef.TryGetMemory(out var memory))
				{
					animState.OriginalCharacterMode = memory.CharacterMode;
					animState.OriginalCharacterModeInput = memory.CharacterModeInput;
				}
				else
				{
					animState.OriginalCharacterMode = ActorMemory.CharacterModes.Normal;
					animState.OriginalCharacterModeInput = 0;
				}
			}

			return this.animationStates[actorRef];
		}

		public class AnimationState
		{
			public enum AnimationStatus
			{
				Inactive,
				Active,
				Paused,
			}

			public AnimationStatus Status { get; set; } = AnimationStatus.Inactive;
			public ActorMemory.CharacterModes OriginalCharacterMode { get; set; }
			public byte OriginalCharacterModeInput { get; set; }

			public AnimationStatus PausedStatus { get; set; }
			public ActorMemory.CharacterModes PausedCharacterMode { get; set; }
			public byte PausedCharacterModeInput { get; set; }
		}
	}
}
