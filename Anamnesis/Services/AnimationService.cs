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
	using Anamnesis.PoseModule;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private const int TickDelay = 1000;
		private const ushort DrawWeaponAnimationId = 34;
		private const ushort IdleAnimationId = 3;

		private readonly ConcurrentDictionary<IntPtr, AnimationState> animationStates = new();

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

		public void DrawWeapon(ActorMemory actor) => this.PlayAnimation(actor, DrawWeaponAnimationId, 1.0f, true);
		public void IdleCharacter(ActorMemory actor) => this.PlayAnimation(actor, IdleAnimationId, 1.0f, true);

		public void PlayAnimation(ActorMemory actor, ushort? animationId, float? animationSpeed, bool interrupt)
		{
			var animState = this.GetAnimationState(actor);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
				this.Unpause(actor);

			if (animState.Status == AnimationState.AnimationStatus.Inactive)
			{
				animState.OriginalCharacterMode = actor.CharacterMode;
				animState.OriginalCharacterModeInput = actor.CharacterModeInput;
			}

			animState.Status = AnimationState.AnimationStatus.Active;
			this.ApplyAnimationOverride(actor, animationId, animationSpeed, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
		}

		public void ResetAnimationOverride(ActorMemory actor)
		{
			var animState = this.GetAnimationState(actor);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
				this.Unpause(actor);

			if (animState.Status == AnimationState.AnimationStatus.Active)
			{
				ActorMemory.CharacterModes mode = animState.OriginalCharacterMode;
				byte modeInput = animState.OriginalCharacterModeInput;

				if (!this.CanSafelyApplyMode(actor, mode, modeInput))
				{
					mode = ActorMemory.CharacterModes.Normal;
					modeInput = 0;
				}

				animState.Status = AnimationState.AnimationStatus.Inactive;
				this.ApplyAnimationOverride(actor, 0, 1f, true, mode, modeInput);
			}
		}

		public void TogglePaused(ActorMemory actor, float newSpeed = 1.0f)
		{
			var animState = this.GetAnimationState(actor);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
			{
				this.Unpause(actor, newSpeed);
			}
			else
			{
				this.Pause(actor);
			}
		}

		public void Unpause(ActorMemory actor, float newSpeed = 1.0f)
		{
			var animState = this.GetAnimationState(actor);

			if (animState.Status == AnimationState.AnimationStatus.Paused)
			{
				ActorMemory.CharacterModes mode = animState.PausedCharacterMode;
				byte modeInput = animState.PausedCharacterModeInput;

				if (!this.CanSafelyApplyMode(actor, mode, modeInput))
				{
					mode = ActorMemory.CharacterModes.Normal;
					modeInput = 0;
				}

				animState.Status = animState.PausedStatus;
				this.ApplyAnimationOverride(actor, null, newSpeed, false, mode, modeInput);
			}
		}

		public void Pause(ActorMemory actor)
		{
			var animState = this.GetAnimationState(actor);

			if (animState.Status != AnimationState.AnimationStatus.Paused)
			{
				animState.PausedStatus = animState.Status;
				animState.PausedCharacterMode = actor.CharacterMode;
				animState.PausedCharacterModeInput = actor.CharacterModeInput;
				animState.Status = AnimationState.AnimationStatus.Paused;

				this.ApplyAnimationOverride(actor, null, 0.0f, false, ActorMemory.CharacterModes.EmoteLoop, 0);
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
			var actorTable = TargetService.GetActorTable();

			foreach ((_, var state) in this.animationStates)
			{
				if (this.IsValidEntry(state, actorTable))
				{
					this.ResetAnimationOverride(state.Actor!);
				}
			}

			this.animationStates.Clear();
		}

		private void CleanupInvalidOverrides()
		{
			var actorTable = TargetService.GetActorTable();

			var stale = this.animationStates.Select(x => x.Value).Where(state => !this.IsValidEntry(state, actorTable)).ToList();
			foreach (var state in stale)
			{
				AnimationState? removedState;
				this.animationStates.TryRemove(state.Address, out removedState);
			}
		}

		private void CleanupPaused()
		{
			var actorTable = TargetService.GetActorTable();

			foreach ((_, var state) in this.animationStates)
			{
				if (this.IsValidEntry(state, actorTable))
				{
					this.Unpause(state.Actor!);
				}
			}
		}

		private bool IsValidEntry(AnimationState state, List<IntPtr> actorTable)
		{
			return state.Actor != null && state.Actor!.Address != IntPtr.Zero && state.Address == state.Actor!.Address && actorTable.Contains(state.Address) && state.ObjectId == state.Actor.ObjectId;
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

		private void ApplyAnimationOverride(ActorMemory actor, ushort? animationId, float? animationSpeed, bool interrupt, ActorMemory.CharacterModes mode, byte modeInput)
		{
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

			MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput, "Animation Mode Override"); // Always set the input before the mode
			MemoryService.Write(actor.GetAddressOfProperty(nameof(ActorMemory.CharacterMode)), mode, "Animation Mode Override");

			if (interrupt)
			{
				MemoryService.Write<ushort>(actor.GetAddressOfProperty(nameof(ActorMemory.TargetAnimation)), 0, "Animation Interrupt");
			}

			actor.Tick();
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

		private AnimationState GetAnimationState(ActorMemory actor)
		{
			AnimationState animState = new();

			if (this.animationStates.TryAdd(actor.Address, animState))
			{
				animState.Actor = actor;
				animState.ObjectId = actor.ObjectId;
				animState.Address = actor.Address;
				animState.OriginalCharacterMode = actor.CharacterMode;
				animState.OriginalCharacterModeInput = actor.CharacterModeInput;
			}

			return this.animationStates[actor.Address];
		}

		public class AnimationState
		{
			public enum AnimationStatus
			{
				Inactive,
				Active,
				Paused,
			}

			public uint ObjectId { get; set; }
			public IntPtr Address { get; set; }
			public ActorMemory? Actor { get; set; }

			public AnimationStatus Status { get; set; } = AnimationStatus.Inactive;
			public ActorMemory.CharacterModes OriginalCharacterMode { get; set; }
			public byte OriginalCharacterModeInput { get; set; }

			public AnimationStatus PausedStatus { get; set; }
			public ActorMemory.CharacterModes PausedCharacterMode { get; set; }
			public byte PausedCharacterModeInput { get; set; }
		}
	}
}
