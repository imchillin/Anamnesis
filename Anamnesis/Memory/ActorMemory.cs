// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Services;
	using PropertyChanged;

	public class ActorMemory : ActorBasicMemory
	{
		private const short RefreshDelay = 250;

		private short refreshDelay;
		private Task? refreshTask;

		private IntPtr? previousObjectKindAddressBeforeGPose;

		public enum RenderModes : int
		{
			Draw = 0,
			Unload = 2,
		}

		public enum CharacterModes : byte
		{
			None = 0,
			Normal = 1,
			EmoteLoop = 3,
			HasAttachment = 4,
			AnimLock = 8,
			Carrying = 9,
			InPositionLoop = 11,
			Performance = 16,
		}

		[Bind(0x008D)] public byte SubKind { get; set; }
		[Bind(0x00F0, BindFlags.Pointer)] public ActorModelMemory? ModelObject { get; set; }
		[Bind(0x0104)] public RenderModes RenderMode { get; set; }
		[Bind(0x01B4, BindFlags.ActorRefresh)] public int ModelType { get; set; }
		[Bind(0x01E2)] public byte ClassJob { get; set; }
		[Bind(0x07C4)] public bool IsAnimating { get; set; }
		[Bind(0x0C30, BindFlags.Pointer)] public ActorMemory? Mount { get; set; }
		[Bind(0x0C38)] public ushort MountId { get; set; }
		[Bind(0x0C58, BindFlags.Pointer)] public ActorMemory? Companion { get; set; }
		[Bind(0x0C78)] public WeaponMemory? MainHand { get; set; }
		[Bind(0x0CE0)] public WeaponMemory? OffHand { get; set; }
		[Bind(0x0DB0)] public ActorEquipmentMemory? Equipment { get; set; }
		[Bind(0x0DD8)] public ActorCustomizeMemory? Customize { get; set; }
		[Bind(0x0E08, BindFlags.Pointer)] public ActorMemory? Ornament { get; set; }
		[Bind(0x0F30)] public uint TargetAnimation { get; set; }
		[Bind(0x0FA4)] public float AnimationSpeed { get; set; }
		[Bind(0x110C)] public ushort AnimationOverride { get; set; }
		[Bind(0x18B8)] public float Transparency { get; set; }
		[Bind(0x19C0)] public CharacterModes CharacterMode { get; set; }
		[Bind(0x19C1)] public byte CharacterModeInput { get; set; }
		[Bind(0x19F4)] public byte AttachmentPoint { get; set; }

		public bool AutomaticRefreshEnabled { get; set; } = true;
		public bool IsRefreshing { get; set; } = false;
		public bool PendingRefresh { get; set; } = false;

		public bool IsPlayer => this.ModelObject != null && this.ModelObject.IsPlayer;

		[DependsOn(nameof(CharacterMode), nameof(CharacterModeInput), nameof(MountId), nameof(Mount))]
		public bool IsMounted => this.CharacterMode == CharacterModes.HasAttachment && this.CharacterModeInput == 0 && this.MountId != 0 && this.Mount != null;

		[DependsOn(nameof(CharacterMode), nameof(CharacterModeInput), nameof(Ornament))]
		public bool IsUsingOrnament => this.CharacterMode == CharacterModes.HasAttachment && this.CharacterModeInput != 0 && this.Ornament != null;

		[DependsOn(nameof(Companion))]
		public bool HasCompanion => this.Companion != null;

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
			if (this.IsRefreshing)
				return;

			if (!this.CanRefresh)
				return;

			if (this.Address == IntPtr.Zero)
				return;

			try
			{
				Log.Information($"Begining actor refresh for actor address: {this.Address}");

				this.IsRefreshing = true;

				await Task.Delay(16);

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
				this.WriteDelayedBinds();
			}

			this.RaisePropertyChanged(nameof(this.IsPlayer));
			await Task.Delay(150);
			this.RaisePropertyChanged(nameof(this.IsPlayer));
		}

		public bool CanHasNpcFace()
		{
			int index = ActorService.Instance.GetActorTableIndex(this.Address);

			// only the local player should get npc faces!
			if (index != 0)
				return false;

			if (this.Customize?.Head > 10)
				return true;

			return false;
		}

		public void OnRetargeted()
		{
			GposeService gpose = GposeService.Instance;

			// dont apply the npc face hack to actors that dont need it, since it breaks weirdly sometimes.
			if (this.CanHasNpcFace() || this.previousObjectKindAddressBeforeGPose != null)
			{
				if (gpose.IsGpose && gpose.IsChangingState)
				{
					// Entering gpose
					if (this.ObjectKind == ActorTypes.Player)
					{
						this.previousObjectKindAddressBeforeGPose = this.GetAddressOfProperty(nameof(this.ObjectKind));
						this.ObjectKind = ActorTypes.BattleNpc;

						// Sanity check that we do get turned back into a player
						Task.Run(async () =>
						{
							await Task.Delay(3000);
							MemoryService.Write((IntPtr)this.previousObjectKindAddressBeforeGPose, ActorTypes.Player, "NPC face fix");
						});
					}
				}
				else if (gpose.IsGpose && !gpose.IsChangingState)
				{
					// Entered gpose
					if (this.previousObjectKindAddressBeforeGPose != null)
					{
						MemoryService.Write((IntPtr)this.previousObjectKindAddressBeforeGPose, ActorTypes.Player, "NPC face fix");
						this.ObjectKind = ActorTypes.Player;
					}
				}
			}
		}

		protected override void ActorRefresh(string propertyName)
		{
			if (!this.AutomaticRefreshEnabled)
				return;

			if (this.IsRefreshing)
			{
				// dont refresh because of a refresh!
				if (propertyName == nameof(this.ObjectKind) || propertyName == nameof(this.RenderMode))
				{
					return;
				}
			}

			this.Refresh();
		}

		protected override bool CanWrite(BindInfo bind)
		{
			if (this.IsRefreshing)
			{
				if (bind.Memory != this)
				{
					Log.Warning("Skipping Bind " + bind);

					// Do not allow writing of any properties form sub-memory while we are refreshing
					return false;
				}
				else
				{
					// do not allow writing of any properties except the ones needed for refresh during a refresh.
					return bind.Property.Name == nameof(this.ObjectKind) || bind.Property.Name == nameof(this.RenderMode);
				}
			}

			return base.CanWrite(bind);
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
