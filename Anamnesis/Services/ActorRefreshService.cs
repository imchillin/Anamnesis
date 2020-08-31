// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Memory;

	public class ActorRefreshService : ServiceBase<ActorRefreshService>
	{
		// how long to wait after a change before calling Apply()
		private const int ApplyDelay = 500;

		private static int applyCountdown = 0;
		private static Task? applyTask;

		public static bool IsRefreshing
		{
			get;
			private set;
		}

		public static void Refresh(ActorViewModel actor)
		{
			applyCountdown = ApplyDelay;

			if (applyTask == null || applyTask.IsCompleted)
			{
				applyTask = ApplyAfterDelay(actor);
			}
		}

		public static async Task RefreshAsync(ActorViewModel actor)
		{
			while (IsRefreshing)
				await Task.Delay(100);

			Refresh(actor);
			PendingRefreshImmediate();

			await Task.Delay(50);

			while (IsRefreshing)
				await Task.Delay(100);

			await Task.Delay(50);
		}

		public static void PendingRefreshImmediate()
		{
			applyCountdown = 0;
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			IsRefreshing = false;
		}

		private static async Task ApplyAfterDelay(ActorViewModel actor)
		{
			while (applyCountdown > 0)
			{
				while (applyCountdown > 0)
				{
					applyCountdown -= 50;
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
