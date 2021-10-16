// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Services;

	public class ActorMemory : ActorBasicMemory
	{
		private const short RefreshDelay = 250;

		private short refreshDelay;
		private Task? refreshTask;

		public enum RenderModes : int
		{
			Draw = 0,
			Unload = 2,
		}

		[Bind(0x008D)] public byte SubKind { get; set; }
		[Bind(0x00F0, BindFlags.Pointer)] public ActorModelMemory? ModelObject { get; set; }
		[Bind(0x0104)] public RenderModes RenderMode { get; set; }
		[Bind(0x01B4, BindFlags.ActorRefresh)] public int ModelType { get; set; }
		[Bind(0x01E2)] public byte ClassJob { get; set; }
		[Bind(0x07C4)] public bool IsAnimating { get; set; }
		[Bind(0x0F08)] public WeaponMemory? MainHand { get; set; }
		[Bind(0x0F70)] public WeaponMemory? OffHand { get; set; }
		[Bind(0x1040)] public ActorEquipmentMemory? Equipment { get; set; }
		[Bind(0x182C)] public float Transparency { get; set; }
		[Bind(0x1898)] public ActorCustomizeMemory? Customize { get; set; }

		public bool AutomaticRefreshEnabled { get; set; } = true;
		public bool IsRefreshing { get; set; } = false;
		public bool PendingRefresh { get; set; } = false;

		public int ObjectKindInt
		{
			get => (int)this.ObjectKind;
			set => this.ObjectKind = (ActorTypes)value;
		}

		/// <summary>
		/// Refresh the actor to force the game to load any changed values for appearance.
		/// </summary>
		public void Refresh()
		{
			this.refreshDelay = RefreshDelay;

			if (this.refreshTask == null || this.refreshTask.IsCompleted)
			{
				this.refreshTask = Task.Run(this.RefreshTask);
			}
		}

		public override void Tick()
		{
			// Since writing is immadiate from poperties, we don't want to tick (read) anything
			// during a refresh.
			if (this.IsRefreshing || this.PendingRefresh)
				return;

			base.Tick();
		}

		/// <summary>
		/// Refresh the actor to force the game to load any changed values for appearance.
		/// </summary>
		public async Task RefreshAsync()
		{
			if (this.IsRefreshing || GposeService.Instance.IsGpose)
				return;

			try
			{
				Log.Information($"Begining actor refresh for actor address: {this.Address}");

				this.IsRefreshing = true;

				if (this.ObjectKind == ActorTypes.Player)
				{
					this.ObjectKind = ActorTypes.BattleNpc;
					this.RenderMode = RenderModes.Unload;
					await Task.Delay(75);
					this.RenderMode = RenderModes.Draw;
					await Task.Delay(75);
					this.ObjectKind = ActorTypes.Player;
					this.RenderMode = RenderModes.Draw;
				}
				else
				{
					this.RenderMode = RenderModes.Unload;
					await Task.Delay(75);
					this.RenderMode = RenderModes.Draw;
				}

				await Task.Delay(150);

				Log.Information($"Completed actor refresh for actor address: {this.Address}");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to refresh actor");
			}
			finally
			{
				this.IsRefreshing = false;
			}
		}

		protected override void ActorRefresh()
		{
			this.Refresh();
		}

		protected override bool ShouldBind(BindInfo bind)
		{
			// Only object kind and render mode can be changed while refreshing.
			if (this.IsRefreshing && bind.Name != nameof(this.ObjectKind) && bind.Name != nameof(this.RenderMode))
				return false;

			return base.ShouldBind(bind);
		}

		private async Task RefreshTask()
		{
			// Double loops to handle case where a refresh delay was added
			// while the refresh was running
			while (this.refreshDelay > 0)
			{
				lock (this)
					this.PendingRefresh = true;

				while (this.refreshDelay > 0)
				{
					await Task.Delay(10);
					this.refreshDelay -= 10;
				}

				lock (this)
					this.PendingRefresh = false;

				await this.RefreshAsync();
			}
		}
	}
}
