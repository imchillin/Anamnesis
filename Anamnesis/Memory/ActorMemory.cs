// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Refresh;
using Anamnesis.Core.Extensions;
using Anamnesis.Files;
using Anamnesis.Services;
using Anamnesis.Utils;
using PropertyChanged;
using RemoteController.Interop.Delegates;
using RemoteController.Interop.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class ActorMemory : GameObjectMemory, IDisposable
{
	private const double REFRESH_DEBOUNCE_TIMEOUT_MS = 16;

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
	private readonly SemaphoreSlim snapshotSemaphore = new(1, 1);

	private int isRefreshing = 0;
	private bool needsRefresh = false;
	private bool forceReloadOnRefresh = false;

	public ActorMemory()
	{
		lock (s_hookLock)
		{
			s_isWandererHook ??= ControllerService.Instance.RegisterWrapper<Character.IsWanderer>();
		}

		this.backupQueue = new(this.BackupAsync, 250);

		this.PropertyChanged += this.HandlePropertyChanged;

		// Initialize the debounce timer
		this.refreshDebounceTimer = new(REFRESH_DEBOUNCE_TIMEOUT_MS) { AutoReset = false };
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

	[Bind(0x0100, BindFlags.Pointer)]
	public new ActorModelMemory? ModelObject
	{
		get => base.ModelObject as ActorModelMemory;
		set
		{
			if (base.ModelObject == value)
				return;

			base.ModelObject = value;
			this.OnPropertyChanged(nameof(base.ModelObject));
			this.OnPropertyChanged(nameof(this.ModelObject));
		}
	}

	[Bind(0x01CA)] public byte ClassJob { get; set; } // Source: CharacterData; Calculated using GameObject size + ClassJob offset
	[Bind(0x0680, BindFlags.Pointer)] public ActorMemory? Mount { get; set; } // Targets object within MountContainer
	[Bind(0x0688)] public ushort MountId { get; set; }
	[Bind(0x06E8, BindFlags.Pointer)] public CompanionMemory? Companion { get; set; } // Targets object within CompanionContainer
	[Bind(Actor.DRAW_DATA_OFFSET)] public DrawDataMemory DrawData { get; set; } = new();
	[Bind(0x0970, BindFlags.Pointer)] public OrnamentMemory? Ornament { get; set; } // Targets object within OrnamentContainer
	[Bind(0x0978)] public ushort OrnamentId { get; set; }
	[Bind(0x0A30)] public AnimationMemory? Animation { get; set; }
	[Bind(0x1A58)] public byte Voice { get; set; }
	[Bind(0x1B38, BindFlags.ActorRefresh)] public int ModelType { get; set; } = -1; // Invalid by default to prevent false positive human checks
	[Bind(0x1BA4)] public bool IsMotionDisabled { get; set; }
	[Bind(0x22E8)] public float Transparency { get; set; }
	[Bind(0x2364)] public byte CharacterModeRaw { get; set; }
	[Bind(0x2365)] public byte CharacterModeInput { get; set; }

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

	public RefreshBlockedReason RefreshBlockReason => GetRefreshBlockedReason(this);

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

	[DoNotNotify]
	public CharacterFile? LastAppearanceSnapshot { get; set; }

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
	/// <param name="forceReload">If set, a full redraw will be forced, ignoring any optimizations that would normally be applied.</param>
	/// <returns>True if the actor was refreshed, otherwise false.</returns>
	public static async Task<bool> RefreshActor(ActorMemory actor, bool forceReload = false)
	{
		if (CanRefreshActor(actor))
		{
			foreach (IActorRefresher actorRefresher in s_actorRefreshers)
			{
				if (actorRefresher.CanRefresh(actor))
				{
					Log.Information($"Executing {actorRefresher.GetType().Name} refresh for actor address: 0x{actor.Address:X}");
					await actorRefresher.RefreshActor(actor, forceReload);
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Determines if the actor object can be refreshed or not.
	/// Returns <see cref="RefreshBlockedReason.None"/> if any refresher can handle the actor.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RefreshBlockedReason GetRefreshBlockedReason(ActorMemory actor)
	{
		if (!actor.IsValid)
			return RefreshBlockedReason.IntegrationDisabled;

		RefreshBlockedReason lastReason = RefreshBlockedReason.IntegrationDisabled;

		foreach (IActorRefresher actorRefresher in s_actorRefreshers)
		{
			var reason = actorRefresher.GetRefreshAvailability(actor);
			if (reason == RefreshBlockedReason.None)
				return RefreshBlockedReason.None;

			// Prefer more specific reasons over IntegrationDisabled
			if (reason != RefreshBlockedReason.IntegrationDisabled)
				lastReason = reason;
		}

		return lastReason;
	}

	public override void Dispose()
	{
		this.PropertyChanged -= this.HandlePropertyChanged;
		this.refreshDebounceTimer?.Dispose();
		base.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	public override void Synchronize(IReadOnlySet<string>? inclGroups = null, IReadOnlySet<string>? exclGroups = null)
	{
		this.History.Tick();

		// Don't synchronize the actor during a refresh.
		if (this.IsRefreshing)
			return;

		base.Synchronize(inclGroups, exclGroups);

		// Take the initial snapshot after the first synchronization
		if (this.LastAppearanceSnapshot == null)
		{
			_ = this.TakeInitialSnapshotAsync();
		}
	}

	/// <summary>
	/// Asynchronously refresh the actor to force the game to reflect appearance changes.
	/// </summary>
	public async Task Refresh(bool forceReload = false)
	{
		if (this.IsRefreshing)
		{
			Log.Verbose("Refresh requested while busy. Marking as pending.");
			this.needsRefresh = true;
			this.forceReloadOnRefresh |= forceReload;
			return;
		}

		if (!this.CanRefresh || this.Address == IntPtr.Zero)
			return;

		try
		{
			Log.Verbose($"Attempting actor refresh for actor address: 0x{this.Address:X}");

			this.IsRefreshing = true;

			do
			{
				this.needsRefresh = false;

				if (await RefreshActor(this, this.forceReloadOnRefresh | forceReload))
				{
					Log.Verbose($"Completed actor refresh cycle for: 0x{this.Address:X}");
				}
				else
				{
					Log.Warning($"Could not refresh actor: 0x{this.Address:X}");
				}

				this.forceReloadOnRefresh = false;
			}
			while (this.needsRefresh);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error refreshing actor: 0x{this.Address:X}");
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
		this.OnPropertyChanged(nameof(this.RefreshBlockReason));
	}

	public bool IsWanderer()
	{
		try
		{
			return ControllerService.Instance.InvokeHook<bool>(s_isWandererHook!, args: this.Address) ?? false;
		}
		catch
		{
			Log.Verbose($"Failed to invoke 'IsWanderer' hook for actor at address 0x{this.Address:X}");
			return false;
		}
	}

	internal HumanDrawData BuildDrawData()
	{
		HumanDrawData drawData = default;
		this.DrawData.Customize?.WriteTo(ref drawData);
		this.DrawData.Equipment?.WriteTo(ref drawData);
		return drawData;
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

	private async Task TakeInitialSnapshotAsync()
	{
		await this.snapshotSemaphore.WaitAsync();
		try
		{
			if (this.LastAppearanceSnapshot != null)
				return;

			var snapshot = await Task.Run(() =>
			{
				var newSnapshot = new CharacterFile();
				newSnapshot.WriteToFile(this, CharacterFile.SaveModes.All);
				return newSnapshot;
			});

			this.LastAppearanceSnapshot = snapshot;
		}
		finally
		{
			this.snapshotSemaphore.Release();
		}
	}
}
