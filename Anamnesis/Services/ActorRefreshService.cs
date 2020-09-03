// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Memory;

	public class ActorRefreshService : ServiceBase<ActorRefreshService>
	{
		private const int RefreshDelay = 250;

		private ActorViewModel? actor;
		private int refreshCountdown;

		public static bool IsRefreshing
		{
			get;
			private set;
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			TargetService.ActorSelected += this.OnActorSelected;
			this.OnActorSelected(TargetService.SelectedActor);
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.OnActorSelected(null);
			IsRefreshing = false;
		}

		private void OnActorSelected(ActorViewModel? actor)
		{
			if (this.actor != null)
				this.actor.ViewModelChanged -= this.OnSelectedActorChanged;

			this.actor = actor;

			if (this.actor != null)
			{
				this.actor.ViewModelChanged += this.OnSelectedActorChanged;
			}
		}

		private void OnSelectedActorChanged(object sender)
		{
			if (IsRefreshing)
				return;

			bool startTask = this.refreshCountdown <= 0;
			this.refreshCountdown = RefreshDelay;

			if (startTask)
			{
				Task.Run(this.Refresh);
			}
		}

		private async Task Refresh()
		{
			if (this.actor == null || this.actor.Pointer == null)
				return;

			IntPtr actorPointer = (IntPtr)this.actor.Pointer;

			while (this.refreshCountdown > 0)
			{
				await Task.Delay(10);
				this.refreshCountdown -= 10;
			}

			this.refreshCountdown = 0;

			// if the target actor changed while this refresh was pending, abort.
			if (actorPointer != (IntPtr)this.actor.Pointer)
				return;

			IsRefreshing = true;
			this.actor.Enabled = false;

			// we create a new actor view model here so that we can write the values we need to update
			// for the refresh without the actual actorview model being able to write its own values.
			// if the other view model attempts to write a value during a refresh, the game will crash.
			ActorViewModel newVm = new ActorViewModel(actorPointer);

			if (newVm.ObjectKind == ActorTypes.Player)
			{
				newVm.ObjectKind = ActorTypes.BattleNpc;
				newVm.RenderMode = RenderModes.Unload;
				await MemoryService.WaitForMemoryTick();
				await Task.Delay(50);
				newVm.RenderMode = RenderModes.Draw;
				await MemoryService.WaitForMemoryTick();
				await Task.Delay(50);
				newVm.ObjectKind = ActorTypes.Player;
			}
			else
			{
				newVm.RenderMode = RenderModes.Unload;
				await MemoryService.WaitForMemoryTick();
				await Task.Delay(50);
				newVm.RenderMode = RenderModes.Draw;
			}

			await MemoryService.WaitForMemoryTick();
			await Task.Delay(50);

			Log.Write("Refresh Complete", "Actor Refresh");
			IsRefreshing = false;
			this.actor.Enabled = true;
		}
	}
}
