// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System.Threading.Tasks;
	using ConceptMatrix;

	public class ActorRefreshService : IActorRefreshService
	{
		// how long to wait after a change before calling Apply()
		private const int ApplyDelay = 500;

		private int applyCountdown = 0;
		private Task applyTask;
		private SelectionService selectionService;

		public bool IsRefreshing
		{
			get;
			private set;
		}

		public Task Initialize()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			this.selectionService = Services.Get<SelectionService>();
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.IsRefreshing = false;
			return Task.CompletedTask;
		}

		public void Refresh(Actor actor)
		{
			this.applyCountdown = ApplyDelay;

			if (this.applyTask == null || this.applyTask.IsCompleted)
			{
				this.applyTask = this.ApplyAfterDelay(actor);
			}
		}

		public async Task RefreshAsync(Actor actor)
		{
			while (this.IsRefreshing)
				await Task.Delay(100);

			this.Refresh(actor);
			this.PendingRefreshImmediate();

			await Task.Delay(50);

			while (this.IsRefreshing)
				await Task.Delay(100);

			await Task.Delay(50);
		}

		public void PendingRefreshImmediate()
		{
			this.applyCountdown = 0;
		}

		private async Task ApplyAfterDelay(Actor actor)
		{
			while (this.applyCountdown > 0)
			{
				while (this.applyCountdown > 0)
				{
					this.applyCountdown -= 50;
					await Task.Delay(50);
				}

				this.IsRefreshing = true;
				Log.Write("Refresh Begin", "Actor Refresh");

				using IMemory<ActorTypes> actorTypeMem = actor.GetMemory(Offsets.Main.ActorType);
				using IMemory<byte> actorRenderMem = actor.GetMemory(Offsets.Main.ActorRender);

				if (actorTypeMem.Value == ActorTypes.Player)
				{
					actorTypeMem.SetValue(ActorTypes.BattleNpc, true);
					actorRenderMem.SetValue(2, true);
					await Task.Delay(150);
					actorRenderMem.SetValue(0, true);
					await Task.Delay(150);
					actorTypeMem.SetValue(ActorTypes.Player, true);
				}
				else
				{
					actorRenderMem.SetValue(2, true);
					await Task.Delay(150);
					actorRenderMem.SetValue(0, true);
				}

				this.selectionService.RetargetActors();

				Log.Write("Refresh Complete", "Actor Refresh");
				this.IsRefreshing = false;
			}
		}
	}
}
