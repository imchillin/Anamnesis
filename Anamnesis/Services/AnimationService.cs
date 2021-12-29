// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private readonly List<ActorAnimation> animatingActors = new();
		private NopHookViewModel? animationNopHook;
		private NopHookViewModel? slowMotionNopHook;

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
			this.animationNopHook = new NopHookViewModel(AddressService.AnimationPatch, 0x7);
			this.slowMotionNopHook = new NopHookViewModel(AddressService.SlowMotionPatch, 0x9);
			Task.Run(this.CheckThread);
			return base.Start();
		}

		public void AnimateActor(ActorMemory actor, uint desiredAnimation, ActorMemory.AnimationModes animationMode = ActorMemory.AnimationModes.Normal, int repeatAfter = 0)
		{
			this.ClearAnimation(actor);

			ActorAnimation animationEntry = new()
			{
				Actor = actor,
				AnimationId = desiredAnimation,
				AnimationMode = animationMode,
				RepeatAfter = repeatAfter,
			};

			this.animatingActors.Add(animationEntry);
		}

		public void ClearAnimation(ActorBasicMemory actor)
		{
			this.animatingActors.RemoveAll(p => p.Actor == actor);
		}

		public void ClearAll()
		{
			this.animatingActors.Clear();
		}

		public override async Task Shutdown()
		{
			this.Enabled = false;

			await base.Shutdown();
		}

		private async Task CheckThread()
		{
			while (this.IsAlive)
			{
				if (this.isEnabled)
				{
					if (!GposeService.Instance.IsGpose)
						this.Enabled = false; // Should only run in gpose

					foreach (ActorAnimation actor in this.animatingActors)
					{
						this.TickActor(actor);
					}
				}

				await Task.Delay(100);
			}
		}

		private void TickActor(ActorAnimation animation)
		{
			if (animation.Actor != null)
			{
				if (!animation.Actor.IsAnimating)
				{
					if(animation.State != ActorAnimation.ExecutionState.Begin)
						animation.LastPlayed = DateTime.Now;

					return;
				}

				if (animation.State == ActorAnimation.ExecutionState.Executed && animation.RepeatAfter == 0)
					return;

				if (animation.State == ActorAnimation.ExecutionState.Executed && animation.RepeatAfter != 0)
				{
					if (DateTime.Now > animation.LastPlayed.Add(TimeSpan.FromSeconds(animation.RepeatAfter)))
					{
						animation.State = ActorAnimation.ExecutionState.Begin;
					}
				}

				if (animation.State == ActorAnimation.ExecutionState.Executed)
					return;

				// The flow below is a little confusing, but basically we need to ensure that the animation is reset and then we can play our custom animation.
				// First we need to set TargetAnimation to 0 and wait for NextAnimation to tick over to 0 as well.
				// Then we make sure the animation mode is what we want a frame before we set our animation.
				// Once that's done, we set TargetAnimation to the actual animation we want, and again wait for the engine to tick that into NextAnimation.
				// Finally we set TargetAnimation to 0 again which will cause the engine to fire the now queued animation in NextAnimation.
				// This takes a couple of frames to work through, which is why this requires a tick thread to drive all that through.
				animation.State = ActorAnimation.ExecutionState.Executing;

				if (animation.Actor.TargetAnimation != animation.AnimationId && animation.Actor.TargetAnimation != 0)
				{
					animation.Actor.TargetAnimation = 0;
					return;
				}

				if(animation.AnimationMode != animation.Actor.AnimationMode)
				{
					animation.Actor.AnimationMode = animation.AnimationMode;
					return;
				}

				if (animation.Actor.TargetAnimation == 0 && animation.Actor.NextAnimation == 0)
				{
					animation.Actor.TargetAnimation = animation.AnimationId;
					return;
				}

				if (animation.Actor.TargetAnimation == animation.AnimationId && animation.Actor.NextAnimation == animation.AnimationId)
				{
					animation.Actor.TargetAnimation = 0;
					animation.LastPlayed = DateTime.Now;
					animation.State = ActorAnimation.ExecutionState.Executed;
					return;
				}
			}
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
				this.animationNopHook?.SetEnabled(true);
				this.slowMotionNopHook?.SetEnabled(true);
			}
			else
			{
				this.ClearAll();
				this.animationNopHook?.SetEnabled(false);
				this.slowMotionNopHook?.SetEnabled(false);
			}
		}

		private class ActorAnimation
		{
			public enum ExecutionState
			{
				Begin,
				Executing,
				Executed,
			}

			public ActorMemory? Actor { get; init; }
			public uint AnimationId { get; init; }
			public ActorMemory.AnimationModes AnimationMode { get; init; }
			public int RepeatAfter { get; init; }
			public DateTime LastPlayed { get; set; } = new();
			public ExecutionState State { get; set; } = ExecutionState.Begin;
		}
	}
}
