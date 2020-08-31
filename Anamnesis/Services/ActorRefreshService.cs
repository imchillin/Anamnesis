// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Memory;

	public class ActorRefreshService : IService
	{
		// how long to wait after a change before calling Apply()
		private const int ApplyDelay = 500;

		private int applyCountdown = 0;
		private Task? applyTask;

		public static bool IsRefreshing
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
			IsRefreshing = false;
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
			while (IsRefreshing)
				await Task.Delay(100);

			this.Refresh(actor);
			this.PendingRefreshImmediate();

			await Task.Delay(50);

			while (IsRefreshing)
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

				IsRefreshing = true;
				Log.Write("Refresh Begin", "Actor Refresh");

				throw new NotImplementedException();

				/*using IMarshaler<ActorTypes> actorTypeMem = actor.GetMemory(Offsets.Main.ActorType);
				using IMarshaler<byte> actorRenderMem = actor.GetMemory(Offsets.Main.ActorRender);

				if (actorTypeMem.Value == ActorTypes.Player)
				{
					actorTypeMem.SetValue(ActorTypes.BattleNpc, true);
					actorRenderMem.SetValue(2, true);
					await Task.Delay(150);
					actorRenderMem.SetValue(0, true);
					await Task.Delay(150);
					actorTypeMem.SetValue(ActorTypes.Player, true);
					await Task.Delay(150);
				}
				else
				{
					actorRenderMem.SetValue(2, true);
					await Task.Delay(150);
					actorRenderMem.SetValue(0, true);
					await Task.Delay(150);
				}

				await Task.Delay(50);

				await MemoryService.WaitForMemoryTick();
				await Task.Delay(50);

				Log.Write("Refresh Complete", "Actor Refresh");
				IsRefreshing = false;*/
			}
		}
	}
}
