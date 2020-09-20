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
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class ActorRefreshService : ServiceBase<ActorRefreshService>
	{
		private const int AutomaticRefreshDelay = 50;

		private ActorViewModel? actor;
		private int currentRefreshDelay = 0;

		public bool IsRefreshing
		{
			get;
			private set;
		}

		public bool AutomaticRefreshEnabled
		{
			get;
			set;
		}

		public bool PendingRefresh
		{
			get;
			set;
		}

		public static void Refresh()
		{
			Instance.currentRefreshDelay = 1;
			Instance.PendingRefresh = true;
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.AutomaticRefreshEnabled = true;

			TargetService.ActorSelected += this.OnActorSelected;
			this.OnActorSelected(TargetService.SelectedActor);

			_ = Task.Run(this.RefreshTask);
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.OnActorSelected(null);
			this.IsRefreshing = false;
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
			if (!this.AutomaticRefreshEnabled)
				return;

			this.currentRefreshDelay = 250;
			Instance.PendingRefresh = true;
		}

		private async Task RefreshTask()
		{
			while (this.IsAlive)
			{
				if (this.currentRefreshDelay > 0)
				{
					await Task.Delay(10);
					this.currentRefreshDelay -= 10;

					if (this.currentRefreshDelay <= 0)
					{
						Instance.PendingRefresh = false;
						await this.DoRefresh();
					}
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

			this.IsRefreshing = true;
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
			this.IsRefreshing = false;
			////this.actor.Enabled = true;
		}
	}
}
