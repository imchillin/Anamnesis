// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Refresh;
using Anamnesis.Core.Extensions;
using Anamnesis.Services;
using Anamnesis.Utils;
using PropertyChanged;
using RemoteController.Interop.Delegates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class ActorMemory : GameObjectMemory, IDisposable
{
	private const int REFRESH_DEBOUNCE_TIMEOUT = 200;

	private static readonly List<IActorRefresher> s_actorRefreshers =
	[
		new BrioActorRefresher(),
		new PenumbraActorRefresher(),
		new AnamnesisActorRefresher(),
	];

	private static readonly Lock s_hookLock = new();
	private static HookHandle? s_isWandererHook = null;

	private readonly System.Timers.Timer refreshDebounceTimer;
	private readonly FuncQueue backupQueue;

	private int isRefreshing = 0;

	public ActorMemory()
	{
		lock (s_hookLock)
		{
			s_isWandererHook ??= ControllerService.Instance.RegisterWrapper<Character.IsWanderer>();
		}

		this.backupQueue = new(this.BackupAsync, 250);

		this.PropertyChanged += this.HandlePropertyChanged;

		// Initialize the debounce timer
		this.refreshDebounceTimer = new(REFRESH_DEBOUNCE_TIMEOUT) { AutoReset = false };
		this.refreshDebounceTimer.Elapsed += async (s, e) => { await this.Refresh(); };
	}

	public event EventHandler? Refreshed;

	public enum CharacterModes : byte
	{
		None = 0,
		Normal = 1,
		EmoteLoop = 3,
		Mounted = 4,
		AnimLock = 8,
		Carrying = 9,
		InPositionLoop = 11,
		Performance = 16,
	}

	[Flags]
	public enum CharacterFlagDefs : byte
	{
		None = 0,
		WeaponsVisible = 1 << 0,
		WeaponsDrawn = 1 << 2,
		VisorToggled = 1 << 4,
		HeadgearEarsHidden = 1 << 5,
	}

	[Bind(0x0100, BindFlags.Pointer)] public new ActorModelMemory? ModelObject { get; set; }
	[Bind(0x01CA)] public byte ClassJob { get; set; } // Source: CharacterData; Calculated using GameObject size + ClassJob offset
	[Bind(0x0680, BindFlags.Pointer)] public ActorMemory? Mount { get; set; } // Targets object within MountContainer
	[Bind(0x0688)] public ushort MountId { get; set; }
	[Bind(0x06E8, BindFlags.Pointer)] public ActorMemory? Companion { get; set; } // Targets object within CompanionContainer
	[Bind(0x0708)] public WeaponMemory? MainHand { get; set; }
	[Bind(0x0778)] public WeaponMemory? OffHand { get; set; }
	[Bind(0x08C8)] public ActorEquipmentMemory? Equipment { get; set; }
	[Bind(0x0918)] public ActorCustomizeMemory? Customize { get; set; }
	[Bind(0x0936, BindFlags.ActorRefresh)] public bool HatHidden { get; set; }
	[Bind(0x0937, BindFlags.ActorRefresh)] public CharacterFlagDefs CharacterFlags { get; set; }
	[Bind(0x0938)] public GlassesMemory? Glasses { get; set; }
	[Bind(0x0970, BindFlags.Pointer)] public ActorMemory? Ornament { get; set; } // Targets object within OrnamentContainer
	[Bind(0x0978)] public ushort OrnamentId { get; set; }
	[Bind(0x0A30)] public AnimationMemory? Animation { get; set; }
	[Bind(0x1A58)] public byte Voice { get; set; }
	[Bind(0x1B38, BindFlags.ActorRefresh)] public int ModelType { get; set; } = -1; // Invalid by default to prevent false positive human checks
	[Bind(0x1BA4)] public bool IsMotionDisabled { get; set; }
	[Bind(0x22E8)] public float Transparency { get; set; }
	[Bind(0x2364)] public byte CharacterModeRaw { get; set; }
	[Bind(0x2365)] public byte CharacterModeInput { get; set; }
	[Bind(0x2384)] public byte AttachmentPoint { get; set; } // Part of Ornament

	public PinnedActor? Pinned { get; set; }

	public History History { get; private set; } = new();

	public bool AutomaticRefreshEnabled { get; set; } = true;
	public bool IsRefreshing
	{
		get => Interlocked.CompareExchange(ref this.isRefreshing, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.isRefreshing, value ? 1 : 0);
	}

	public bool IsWeaponDirty { get; set; } = false;

	[DependsOn(nameof(IsValid), nameof(IsOverworldActor), nameof(Name), nameof(RenderMode))]
	public bool CanRefresh => CanRefreshActor(this);

	public bool IsHuman => this.ModelObject != null && this.ModelObject.IsHuman;

	[DependsOn(nameof(ModelType))]
	public bool IsChocobo => this.ModelType == 1;

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

	[DependsOn(nameof(MountId), nameof(Mount))]
	public bool IsMounted => this.MountId != 0 && this.Mount != null;

	[DependsOn(nameof(OrnamentId), nameof(Ornament))]
	public bool IsUsingOrnament => this.Ornament != null && this.OrnamentId != 0;

	[DependsOn(nameof(Companion))]
	public bool HasCompanion => this.Companion != null;

	[DependsOn(nameof(CharacterFlags))]
	public bool VisorToggled
	{
		get => this.CharacterFlags.HasFlagUnsafe(CharacterFlagDefs.VisorToggled);
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

	[DependsOn(nameof(CharacterFlags))]
	public bool HeadgearEarsHidden
	{
		get => this.CharacterFlags.HasFlagUnsafe(CharacterFlagDefs.HeadgearEarsHidden);
		set
		{
			if (value)
			{
				this.CharacterFlags |= CharacterFlagDefs.HeadgearEarsHidden;
			}
			else
			{
				this.CharacterFlags &= ~CharacterFlagDefs.HeadgearEarsHidden;
			}
		}
	}

	[DependsOn(nameof(IsMotionDisabled))]
	public bool IsMotionEnabled
	{
		get => !this.IsMotionDisabled;
		set => this.IsMotionDisabled = !value;
	}

	[DependsOn(nameof(ObjectIndex), nameof(CharacterMode))]
	public bool CanAnimate => this.CharacterMode == CharacterModes.Normal || this.CharacterMode == CharacterModes.AnimLock || !ActorService.IsLocalOverworldPlayer(this.ObjectIndex);

	[DependsOn(nameof(CharacterMode))]
	public bool IsAnimationOverridden => this.CharacterMode == CharacterModes.AnimLock;

	/// <summary>Determines if the actor can be refreshed.</summary>
	/// <param name="actor">The actor to check.</param>
	/// <returns>True if the actor can be refreshed, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanRefreshActor(ActorMemory actor)
	{
		if (!actor.IsValid)
			return false;

		foreach (IActorRefresher actorRefresher in s_actorRefreshers)
		{
			if (actorRefresher.CanRefresh(actor))
				return true;
		}

		return false;
	}

	/// <summary>Refreshes the specified actor.</summary>
	/// <param name="actor">The actor to refresh.</param>
	/// <returns>True if the actor was refreshed, otherwise false.</returns>
	public static async Task<bool> RefreshActor(ActorMemory actor)
	{
		if (CanRefreshActor(actor))
		{
			foreach (IActorRefresher actorRefresher in s_actorRefreshers)
			{
				if (actorRefresher.CanRefresh(actor))
				{
					Log.Information($"Executing {actorRefresher.GetType().Name} refresh for actor address: {actor.Address}");
					await actorRefresher.RefreshActor(actor);
					return true;
				}
			}
		}

		return false;
	}

	public override void Dispose()
	{
		this.PropertyChanged -= this.HandlePropertyChanged;
		this.refreshDebounceTimer?.Dispose();
		base.Dispose();
		GC.SuppressFinalize(this);
	}

	public override void Synchronize()
	{
		this.History.Tick();

		// Don't synchronize the actor during a refresh.
		if (this.IsRefreshing)
			return;

		base.Synchronize();
	}

	/// <summary>
	/// Asynchronously refresh the actor to force the game to reflect appearance changes.
	/// </summary>
	public async Task Refresh()
	{
		if (this.IsRefreshing)
			return;

		if (!this.CanRefresh)
			return;

		if (this.Address == IntPtr.Zero)
			return;

		try
		{
			Log.Information($"Attempting actor refresh for actor address: {this.Address}");

			this.IsRefreshing = true;

			if (await RefreshActor(this))
			{
				Log.Information($"Completed actor refresh for actor address: {this.Address}");
			}
			else
			{
				Log.Information($"Could not refresh actor: {this.Address}");
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error refreshing actor: {this.Address}");
		}
		finally
		{
			this.IsRefreshing = false;
			this.IsWeaponDirty = false;
		}

		this.OnPropertyChanged(nameof(this.IsHuman));
		this.OnRefreshed();
	}

	public async Task BackupAsync()
	{
		while (this.IsRefreshing)
			await Task.Delay(10);

		this.Pinned?.CreateCharacterBackup(PinnedActor.BackupModes.Gpose);
	}

	public void RaiseRefreshChanged()
	{
		this.OnPropertyChanged(nameof(this.CanRefresh));
	}

	public bool IsWanderer()
	{
		try
		{
			return ControllerService.Instance.InvokeHook<bool>(s_isWandererHook!, args: this.Address) ?? false;
		}
		catch
		{
			return false;
		}
	}

	protected virtual void OnRefreshed()
	{
		this.Refreshed?.Invoke(this, EventArgs.Empty);
	}

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e is not MemObjPropertyChangedEventArgs memObjEventArgs)
			return;

		var change = memObjEventArgs.Context;

		// Do not not refresh the actor if the change originated from the game
		if (change.Origin == PropertyChange.Origins.Game)
			return;

		// Only record changes that originate from the user
		if (!change.OriginBind.Flags.HasFlagUnsafe(BindFlags.DontRecordHistory) && !HistoryService.InstanceOrNull?.IsRestoring == true)
		{
			if (change.Origin == PropertyChange.Origins.User)
			{
				// Big hack to keep bone change history names short.
				if (change.OriginBind.Memory.ParentBind?.Type == typeof(TransformMemory))
				{
					change.Name = (PoseService.SelectedBonesText == null) ?
						LocalizationService.GetStringFormatted("History_ChangeBone", LocalizationService.GetString("Pose_OtherUnknown")) :
						LocalizationService.GetStringFormatted("History_ChangeBone", PoseService.SelectedBonesText);
				}

				this.History.Record(change);
			}
		}

		// Create backup
		this.backupQueue.Invoke();

		// Refresh the actor
		if (this.AutomaticRefreshEnabled && change.OriginBind.Flags.HasFlagUnsafe(BindFlags.ActorRefresh))
		{
			// Don't refresh because of a refresh
			if (this.IsRefreshing && (change.OriginBind.Name == nameof(this.ObjectKind) || change.OriginBind.Name == nameof(this.RenderMode)))
				return;

			if (change.OriginBind.Flags.HasFlagUnsafe(BindFlags.WeaponRefresh))
				this.IsWeaponDirty = true;

			// Restart the debounce timer if it's already running, otherwise start it
			this.refreshDebounceTimer.Stop();
			this.refreshDebounceTimer.Start();
		}
	}
}
