// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System.Threading.Tasks;

	public class ActorRefreshService : IActorRefreshService
	{
		// how long to wait after a change before calling Apply()
		private const int ApplyDelay = 500;

		private int applyCountdown = 0;
		private Task applyTask;

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

		private async Task ApplyAfterDelay(IBaseMemoryOffset actorOffset)
		{
			while (this.applyCountdown > 0)
			{
				while (this.applyCountdown > 0)
				{
					this.applyCountdown -= 100;
					await Task.Delay(100);
				}

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
			}
		}
	}
}
