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
			ISelectionService selectionService = Services.Get<ISelectionService>();

			while (this.applyCountdown > 0)
			{
				while (this.applyCountdown > 0)
				{
					this.applyCountdown -= 50;
					await Task.Delay(50);
				}

				this.IsRefreshing = true;
				Log.Write("Refresh Begin", "Actor Refresh");

				using (IMemory<ActorTypes> actorTypeMem = actorOffset.GetMemory(Offsets.ActorType))
				{
					using (IMemory<byte> actorRenderMem = actorOffset.GetMemory(Offsets.ActorRender))
					{
						if (actorTypeMem.Value == ActorTypes.Player)
						{
							actorTypeMem.Value = ActorTypes.BattleNpc;
							actorRenderMem.Value = 2;
							await Task.Delay(50);
							actorRenderMem.Value = 0;
							await Task.Delay(50);
							actorTypeMem.Value = ActorTypes.Player;
						}
						else
						{
							actorRenderMem.Value = 2;
							await Task.Delay(50);
							actorRenderMem.Value = 0;
						}
					}
				}

				await Services.Get<ISelectionService>().ResetSelectionAsync();

				Log.Write("Refresh Complete", "Actor Refresh");
				this.IsRefreshing = false;
			}
		}
	}
}
