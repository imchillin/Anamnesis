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
			if (this.actor == null)
				return;

			IsRefreshing = true;
			Log.Write("Refresh Begin", "Actor Refresh");

			this.actor.ObjectKind = ActorTypes.Player;

			if (this.actor.ObjectKind == ActorTypes.Player)
			{
				this.actor.ObjectKind = ActorTypes.BattleNpc;
				this.actor.RenderMode = RenderModes.Unload;
				await Task.Delay(150);
				this.actor.RenderMode = RenderModes.Draw;
				await Task.Delay(150);
				this.actor.ObjectKind = ActorTypes.Player;
				await Task.Delay(150);
			}
			else
			{
				this.actor.RenderMode = RenderModes.Unload;
				await Task.Delay(150);
				this.actor.RenderMode = RenderModes.Draw;
				await Task.Delay(150);
			}

			await Task.Delay(50);

			await MemoryService.WaitForMemoryTick();
			await Task.Delay(50);

			Log.Write("Refresh Complete", "Actor Refresh");
			IsRefreshing = false;
		}
	}
}
