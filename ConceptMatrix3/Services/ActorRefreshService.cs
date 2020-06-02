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

		public event RefreshEvent OnRefreshStarting;
		public event RefreshEvent OnRefreshComplete;

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
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public void Refresh(IBaseMemoryOffset offset)
		{
			this.applyCountdown = ApplyDelay;

			if (this.applyTask == null || this.applyTask.IsCompleted)
			{
				this.applyTask = this.ApplyAfterDelay(offset);
			}
		}

		public async Task RefreshAsync(IBaseMemoryOffset offset)
		{
			while (this.IsRefreshing)
				await Task.Delay(100);

			this.Refresh(offset);
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

		private async Task ApplyAfterDelay(IBaseMemoryOffset actorOffset)
		{
			while (this.applyCountdown > 0)
			{
				while (this.applyCountdown > 0)
				{
					this.applyCountdown -= 50;
					await Task.Delay(50);
				}

				this.IsRefreshing = true;
				this.OnRefreshStarting?.Invoke(actorOffset);
				Log.Write("Refresh Begin", "Actor Refresh");

				using IMemory<ActorTypes> actorTypeMem = actorOffset.GetMemory(Offsets.Main.ActorType);
				actorTypeMem.Name = "Actor Type";
				using IMemory<byte> actorRenderMem = actorOffset.GetMemory(Offsets.Main.ActorRender);
				actorRenderMem.Name = "Actor Render";

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

				this.OnRefreshComplete?.Invoke(actorOffset);
				Log.Write("Refresh Complete", "Actor Refresh");
				this.IsRefreshing = false;
			}
		}
	}
}
