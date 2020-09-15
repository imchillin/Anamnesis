// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
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

		public static bool AwaitingRefresh
		{
			get;
			private set;
		}

		public static bool AutomaticRefreshEnabled
		{
			get;
			set;
		}

		public static void Refresh()
		{
			AwaitingRefresh = true;
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			AutomaticRefreshEnabled = true;

			TargetService.ActorSelected += this.OnActorSelected;
			this.OnActorSelected(TargetService.SelectedActor);

			_ = Task.Run(this.RefreshTask);
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
			if (!AutomaticRefreshEnabled)
				return;

			AwaitingRefresh = true;
		}

		private async Task RefreshTask()
		{
			while (this.IsAlive)
			{
				await Task.Delay(10);

				if (AwaitingRefresh)
				{
					AwaitingRefresh = false;
					await this.DoRefresh();
				}
			}
		}

		private async Task DoRefresh()
		{
			if (this.actor == null || this.actor.Pointer == null)
				return;

			IntPtr actorPointer = (IntPtr)this.actor.Pointer;

			// if the target actor changed while this refresh was pending, abort.
			if (actorPointer != (IntPtr)this.actor.Pointer)
				return;

			IsRefreshing = true;
			////this.actor.Enabled = false;

			MemoryService.EnableMemoryViewModelTick = false;

			// we use direct pointers here so that we can write the values we need to update
			// for the refresh without the actual actorview model being able to write its own values.
			// if the actor view model attempts to write a value during a refresh, the game will crash.
			IntPtr objectKindPointer = actorPointer + 0x008c;
			IntPtr renderModePointer = actorPointer + 0x0104;

			if (this.actor.ObjectKind == ActorTypes.Player)
			{
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.BattleNpc);

				MemoryService.Write(renderModePointer, (int)RenderModes.Unload);
				await Task.Delay(50);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw);
				await Task.Delay(50);
				MemoryService.Write(objectKindPointer, (byte)ActorTypes.Player);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw);
			}
			else
			{
				MemoryService.Write(renderModePointer, (int)RenderModes.Unload);
				await Task.Delay(50);
				MemoryService.Write(renderModePointer, (int)RenderModes.Draw);
			}

			MemoryService.EnableMemoryViewModelTick = true;
			await MemoryService.WaitForMemoryTick();

			await Task.Delay(50);

			Log.Write("Refresh Complete", "Actor Refresh");
			IsRefreshing = false;
			////this.actor.Enabled = true;
		}
	}
}
