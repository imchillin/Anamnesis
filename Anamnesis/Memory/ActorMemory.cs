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

		[Flags]
		public enum CharacterFlagDefs : byte
		{
			None = 0,
			HatHidden = 1 << 0,
			VisorToggled = 1 << 4,
		}

		[Bind(0x008D)] public byte SubKind { get; set; }
		[Bind(0x0B4)] public float Scale { get; set; }
		[Bind(0x00F0, BindFlags.Pointer)] public ActorModelMemory? ModelObject { get; set; }
		[Bind(0x01B4, BindFlags.ActorRefresh)] public int ModelType { get; set; }
		[Bind(0x01E2)] public byte ClassJob { get; set; }
		[Bind(0x07C4)] public bool IsMotionEnabled { get; set; }
		[Bind(0x0C30, BindFlags.Pointer)] public ActorMemory? Mount { get; set; }
		[Bind(0x0C38)] public ushort MountId { get; set; }
		[Bind(0x0C58, BindFlags.Pointer)] public ActorMemory? Companion { get; set; }
		[Bind(0x0C78)] public WeaponMemory? MainHand { get; set; }
		[Bind(0x0CE0)] public WeaponMemory? OffHand { get; set; }
		[Bind(0x0DB0)] public ActorEquipmentMemory? Equipment { get; set; }
		[Bind(0x0DD8)] public ActorCustomizeMemory? Customize { get; set; }
		[Bind(0x0DF6, BindFlags.ActorRefresh)] public CharacterFlagDefs CharacterFlags { get; set; }
		[Bind(0x0E08, BindFlags.Pointer)] public ActorMemory? Ornament { get; set; }
		[Bind(0x0F30)] public ushort TargetAnimation { get; set; }
		[Bind(0x0FA4)] public float BaseAnimationSpeedInternal { get; set; }
		[Bind(0x0FA8)] public float AnimationSpeedTrigger { get; set; }
		[Bind(0x0FC0)] public float LipAnimationSpeedInternal { get; set; }
		[Bind(0x110C)] public ushort BaseAnimationOverride { get; set; }
		[Bind(0x110E)] public ushort LipAnimationOverride { get; set; }
		[Bind(0x18B8)] public float Transparency { get; set; }
		[Bind(0x19C0)] public byte CharacterModeRaw { get; set; }
		[Bind(0x19C1)] public byte CharacterModeInput { get; set; }
		[Bind(0x19F4)] public byte AttachmentPoint { get; set; }

		public bool AutomaticRefreshEnabled { get; set; } = true;
		public bool IsRefreshing { get; set; } = false;
		public bool PendingRefresh { get; set; } = false;

		public bool IsPlayer => this.ModelObject != null && this.ModelObject.IsPlayer;

		[DependsOn(nameof(CharacterModeRaw))]
		public CharacterModes CharacterMode
		{
			get
			{
				return (CharacterModes)this.CharacterModeRaw;
			}
			set
			{
				this.CharacterModeRaw = (byte)value;
			}
		}

		[DependsOn(nameof(CharacterMode), nameof(CharacterModeInput), nameof(MountId), nameof(Mount))]
		public bool IsMounted => this.CharacterMode == CharacterModes.HasAttachment && this.CharacterModeInput == 0 && this.MountId != 0 && this.Mount != null;

		[DependsOn(nameof(CharacterMode), nameof(CharacterModeInput), nameof(Ornament))]
		public bool IsUsingOrnament => this.CharacterMode == CharacterModes.HasAttachment && this.CharacterModeInput != 0 && this.Ornament != null;

		[DependsOn(nameof(Companion))]
		public bool HasCompanion => this.Companion != null;

		[DependsOn(nameof(CharacterFlags))]
		public bool HatHidden
		{
			get => this.CharacterFlags.HasFlag(CharacterFlagDefs.HatHidden);
			set
			{
				if (value)
				{
					this.CharacterFlags |= CharacterFlagDefs.HatHidden;
				}
				else
				{
					this.CharacterFlags &= ~CharacterFlagDefs.HatHidden;
				}
			}
		}

		[DependsOn(nameof(CharacterFlags))]
		public bool VisorToggled
		{
			get => this.CharacterFlags.HasFlag(CharacterFlagDefs.VisorToggled);
			set
			{
				if (value)
				{
					this.CharacterFlags |= CharacterFlagDefs.VisorToggled;
				}
				else
				{
					this.CharacterFlags &= ~CharacterFlagDefs.VisorToggled;
				}
			}
		}

		public int ObjectKindInt
		{
			get => (int)this.ObjectKind;
			set => this.ObjectKind = (ActorTypes)value;
		}

		[DependsOn(nameof(ObjectIndex), nameof(CharacterMode))]
		public bool CanAnimate => (this.CharacterMode == CharacterModes.Normal || this.CharacterMode == CharacterModes.AnimLock) || !ActorService.Instance.IsLocalOverworldPlayer(this.ObjectIndex);

		[DependsOn(nameof(CharacterMode))]
		public bool IsAnimationOverriden => this.CharacterMode == CharacterModes.AnimLock;

		[DependsOn(nameof(BaseAnimationSpeedInternal))]
		public float BaseAnimationSpeed
		{
			get => this.BaseAnimationSpeedInternal;
			set
			{
				this.BaseAnimationSpeedInternal = value;
				this.AnimationSpeedTrigger = this.AnimationSpeedTrigger == 0.0f ? 1.0f : 0.0f;
			}
		}

		[DependsOn(nameof(LipAnimationSpeedInternal))]
		public float LipAnimationSpeed
		{
			get => this.LipAnimationSpeedInternal;
			set
			{
				this.LipAnimationSpeedInternal = value;
				this.AnimationSpeedTrigger = this.AnimationSpeedTrigger == 0.0f ? 1.0f : 0.0f;
			}
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

				if (SettingsService.Current.UseExternalRefresh)
				{
					await Penumbra.Penumbra.Redraw(this.Name);
					return;
				}
				else
				{
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
				}

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

		protected override void HandlePropertyChanged(PropertyChange change)
		{
			if (!this.AutomaticRefreshEnabled)
				return;

			if (this.IsRefreshing)
			{
				// dont refresh because of a refresh!
				if (change.TerminalPropertyName == nameof(this.ObjectKind) || change.TerminalPropertyName == nameof(this.RenderMode))
				{
					return;
				}
			}

			if (change.Origin.Attribute.Flags.HasFlag(BindFlags.ActorRefresh))
			{
				this.Refresh();
			}
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
