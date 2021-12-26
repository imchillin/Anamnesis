// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Anamnesis.Core.Memory;
    using Anamnesis.Memory;

    public class AnimationService : ServiceBase<AnimationService>
	{
		private readonly List<ActorBasicMemory> animatingActors = new();
		private NopHookViewModel? nopHook;

		private bool isEnabled = false;

		public bool Enabled
		{
			get => this.isEnabled;
			set
			{
				if(this.isEnabled != value)
				{
					this.SetOverride(value);
				}
			}
		}

		public void AnimateActor(ActorBasicMemory actor, uint desiredAnimation)
		{
			if (!this.animatingActors.Contains(actor))
				this.animatingActors.Add(actor);

			actor.DesiredAnimation = desiredAnimation;
		}

		public override Task Start()
		{
			this.nopHook = new NopHookViewModel(AddressService.AnimationPatch, 0x7);
			Task.Run(this.CheckThread);
			return base.Start();
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
				if(this.isEnabled)
				{
					if (!GposeService.Instance.IsGpose)
						this.Enabled = false; // Should only run in gpose

					foreach(var actor in this.animatingActors)
					{
						this.TickActor(actor);
					}
				}

				await Task.Delay(100);
			}
		}

		private void TickActor(ActorBasicMemory actor)
		{
			// This allows you to stop animations
			if(actor.DesiredAnimation == 0 && actor.TargetAnimation != 0)
			{
				actor.TargetAnimation = actor.DesiredAnimation;
				return;
			}

			// Determine if we need to reset the state
			if(actor.TargetAnimation != actor.DesiredAnimation && actor.TargetAnimation != 0 && actor.TargetAnimation != 3)
			{
				actor.TargetAnimation = 0;
				return;
			}

			// Once reset, we can go into idle
			if(actor.TargetAnimation == 0 && actor.NextAnimation == 0)
			{
				actor.TargetAnimation = 3;
				return;
			}

			// We wait until we are idling
			if (actor.TargetAnimation == 3 && actor.NextAnimation != 3)
				return;

			// Set the target animation
			if(actor.TargetAnimation != actor.DesiredAnimation)
			{
				actor.TargetAnimation = actor.DesiredAnimation;
				return;
			}

			// Once the target animation is queued, queue idle pose so our anim starts
			if(actor.NextAnimation == actor.DesiredAnimation)
			{
				actor.TargetAnimation = 3;
				return;
			}
		}

		private void SetOverride(bool enabled)
		{
			if (this.isEnabled == enabled)
				return;

			this.isEnabled = enabled;

			if(enabled)
			{
				this.nopHook?.SetEnabled(true);
			}
			else
			{
				this.nopHook?.SetEnabled(false);
				this.animatingActors.Clear();
			}
		}
	}
}
