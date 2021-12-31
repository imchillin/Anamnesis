// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class AnimationService : ServiceBase<AnimationService>
	{
		private readonly ConcurrentDictionary<ActorMemory, ActorAnimation> animatingActors = new();
		private NopHookViewModel? animationNopHook;
		private NopHookViewModel? slowMotionNopHook;

		private bool isEnabled = false;

		public int TickDelay { get; set; } = 50;

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
			Task.Run(this.TickThread);
			return base.Start();
		}

		public void AnimateActor(ActorMemory actor, uint desiredAnimation, ActorMemory.AnimationModes animationMode = ActorMemory.AnimationModes.Normal, float repeatAfter = 0)
		{
			if (desiredAnimation >= GameDataService.ActionTimelines.RowCount)
				return;

			ActorAnimation animationEntry = new()
			{
				Actor = actor,
				AnimationId = desiredAnimation,
				AnimationMode = animationMode,
				RepeatAfter = repeatAfter,
			};

			this.animatingActors.AddOrUpdate(actor, (_) => animationEntry, (_, _) => animationEntry);
		}

		public ActorAnimation? GetAnimation(ActorMemory actor)
		{
			ActorAnimation? animation = null;

			if (this.animatingActors.TryGetValue(actor, out animation))
				return animation;

			return null;
		}

		public void ClearAnimation(ActorMemory actor)
		{
			this.animatingActors.TryRemove(actor, out _);
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

		private async Task TickThread()
		{
			while (this.IsAlive)
			{
				if (this.isEnabled)
				{
					if (!GposeService.Instance.IsGpose)
						this.Enabled = false; // Should only run in gpose

					foreach ((_, ActorAnimation actor) in this.animatingActors)
					{
						this.TickActor(actor);
					}
				}

				await Task.Delay(this.TickDelay);
			}
		}

		private void TickActor(ActorAnimation animation)
		{
			if (animation.Actor != null)
			{
				if (!animation.Actor.IsAnimating)
				{
					if (animation.State != ActorAnimation.ExecutionState.Begin)
						animation.LastPlayed = DateTime.Now;

					return;
				}

				// The flow below is a little confusing, but basically we need to ensure that the animation is reset and then we can play our custom animation.
				// First we need to set TargetAnimation to 0 and wait for NextAnimation to tick over to 0 as well.
				// Then we make sure the animation mode is what we want a frame before we set our animation.
				// Once that's done, we set TargetAnimation to the actual animation we want, and again wait for the engine to tick that into NextAnimation.
				// Finally we set TargetAnimation to 0 again which will cause the engine to fire the now queued animation in NextAnimation.
				// This takes a couple of frames to work through, which is why this requires a tick thread to drive all that through.
				switch (animation.State)
				{
					case ActorAnimation.ExecutionState.Idle:
						if(animation.RepeatAfter != 0)
						{
							if (DateTime.Now > animation.LastPlayed.Add(TimeSpan.FromSeconds(animation.RepeatAfter)))
							{
								animation.State = ActorAnimation.ExecutionState.Begin;
							}
						}

						return;

					case ActorAnimation.ExecutionState.Begin:
						{
							animation.Actor.TargetAnimation = 0;
							animation.State = ActorAnimation.ExecutionState.Resetting;
						}

						return;

					case ActorAnimation.ExecutionState.Resetting:
						{
							if (animation.Actor.TargetAnimation == 0 && animation.Actor.NextAnimation == 0)
							{
								animation.State = ActorAnimation.ExecutionState.SetMode;
							}
						}

						return;

					case ActorAnimation.ExecutionState.SetMode:
						{
							if (animation.AnimationMode != animation.Actor.AnimationMode)
							{
								animation.Actor.AnimationMode = animation.AnimationMode;
							}

							animation.State = ActorAnimation.ExecutionState.Prepare;
						}

						return;

					case ActorAnimation.ExecutionState.Prepare:
						{
							animation.Actor.TargetAnimation = animation.AnimationId;
							animation.State = ActorAnimation.ExecutionState.Fire;
						}

						return;

					case ActorAnimation.ExecutionState.Fire:
						{
							if (animation.Actor.TargetAnimation == animation.AnimationId && animation.Actor.NextAnimation == animation.AnimationId)
							{
								animation.Actor.TargetAnimation = 0;
								animation.State = ActorAnimation.ExecutionState.Firing;
							}
						}

						return;

					case ActorAnimation.ExecutionState.Firing:
						{
							if (animation.Actor.TargetAnimation == 0 && animation.Actor.NextAnimation == 0)
							{
								animation.LastPlayed = DateTime.Now;
								animation.State = ActorAnimation.ExecutionState.Idle;
							}
						}

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

		public class ActorAnimation
		{
			public enum ExecutionState
			{
				Idle,
				Begin,
				Resetting,
				Prepare,
				SetMode,
				Fire,
				Firing,
			}

			public ActorMemory? Actor { get; init; }
			public uint AnimationId { get; init; }
			public ActorMemory.AnimationModes AnimationMode { get; init; }
			public float RepeatAfter { get; init; }
			public DateTime LastPlayed { get; set; } = new();
			public ExecutionState State { get; set; } = ExecutionState.Begin;
		}
	}
}
