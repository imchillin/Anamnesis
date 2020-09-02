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
		private ActorViewModel? actor;

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

			Task.Run(this.Refresh);
		}

		private async Task Refresh()
		{
			if (this.actor == null || this.actor.Pointer == null)
				return;

			IsRefreshing = true;
			this.actor.Locked = true;

			// we create a new actor view model here so that we can write the values we need to update
			// for the refresh without the actual actorview model being able to write its own values.
			// if the other view model attempts to write a value during a refresh, the game will crash.
			ActorViewModel newVm = new ActorViewModel((IntPtr)this.actor.Pointer);

			Log.Write("Refresh Begin", "Actor Refresh");

			if (newVm.ObjectKind == ActorTypes.Player)
			{
				newVm.ObjectKind = ActorTypes.BattleNpc;
				newVm.RenderMode = RenderModes.Unload;
				await Task.Delay(150);
				newVm.RenderMode = RenderModes.Draw;
				await Task.Delay(150);
				newVm.ObjectKind = ActorTypes.Player;
				await Task.Delay(150);
			}
			else
			{
				newVm.RenderMode = RenderModes.Unload;
				await Task.Delay(150);
				newVm.RenderMode = RenderModes.Draw;
				await Task.Delay(150);
			}

			await Task.Delay(50);

			await MemoryService.WaitForMemoryTick();
			await Task.Delay(50);

			Log.Write("Refresh Complete", "Actor Refresh");
			IsRefreshing = false;
			this.actor.Locked = false;
		}
	}
}
