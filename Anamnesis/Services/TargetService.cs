// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XivToolsWpf;

/// <summary>
/// A service that manages the selection and pinning of actors in the application.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class TargetService : ServiceBase<TargetService>
{
	private const int TASK_DELAY = 32; // ms (~30 fps)

	/// <summary>
	/// The delegate object for the <see cref="TargetService.ActorSelected"/> event.
	/// </summary>
	/// <param name="actor">The actor that was selected, or null if selection was cleared.</param>
	public delegate void SelectionEvent(ObjectHandle<ActorMemory>? actor);

	/// <summary>
	/// The delegate object for the <see cref="TargetService.ActorPinned"/>
	/// and <see cref="TargetService.ActorUnPinned"/> events.
	/// </summary>
	/// <param name="actor">The actor that was pinned or unpinned.</param>
	public delegate void PinnedEvent(PinnedActor actor);

	/// <summary>
	/// Event that is triggered when actor selection changes.
	/// </summary>
	public static event SelectionEvent? ActorSelected;

	/// <summary>
	/// Event that is triggered when an actor is pinned.
	/// </summary>
	public static event PinnedEvent? ActorPinned;

	/// <summary>
	/// Event that is triggered when an actor is unpinned.
	/// </summary>
	public static event PinnedEvent? ActorUnPinned;

	/// <summary>
	/// Gets a handle to player target actor.
	/// </summary>
	public ObjectHandle<GameObjectMemory>? PlayerTargetHandle { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the player target is pinnable.
	/// </summary>
	public bool IsPlayerTargetPinnable => this.PlayerTargetHandle != null && this.PlayerTargetHandle.Do(a => a.ObjectKind.IsSupportedType()) == true;

	/// <summary>
	/// Gets the currently pinned selected actor (if any).
	/// </summary>
	public PinnedActor? CurrentlyPinned { get; private set; }

	/// <summary>
	/// Gets or sets the currently pinned actors.
	/// </summary>
	public ObservableCollection<PinnedActor> PinnedActors { get; set; } = [];

	/// <summary>
	/// Gets the memory of the currently pinned selected actor (if any).
	/// </summary>
	[DependsOn(nameof(CurrentlyPinned))]
	public ObjectHandle<ActorMemory>? SelectedActor => this.CurrentlyPinned?.Memory;

	/// <summary>
	/// Gets the count of pinned actors.
	/// </summary>
	public int PinnedActorCount { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the pinned actor count is greater than 4.
	/// </summary>
	[DependsOn(nameof(PinnedActorCount))]
	public bool MoreThanFourPins => this.PinnedActorCount > 4;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies =>
	[
		AddressService.Instance,
		ActorService.Instance,
		GameService.Instance,
		GposeService.Instance
	];

	/// <summary>
	/// Pins the targeted actor to the list of pinned actors.
	/// </summary>
	/// <param name="handle">An object handle to the target actor to pin.</param>
	/// <param name="select">A flag indicating whether to select the actor after pinning.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public static async Task PinActor(ObjectHandle<GameObjectMemory> handle, bool select = false)
	{
		if (!handle.IsValid)
			return;

		var result = handle.Do(a =>
		{
			if (!a.ObjectKind.IsSupportedType())
			{
				Log.Warning($"You cannot pin actor of type: {a.ObjectKind}");
				return false;
			}

			return true;
		});

		if (result != true)
			return;

		try
		{
			foreach (PinnedActor otherActor in Instance.PinnedActors)
			{
				if (handle.Address == otherActor.Pointer)
				{
					Log.Information($"Actor already pinned: {otherActor}");
					Instance.SelectActor(otherActor);
					return;
				}
			}

			var actorMemHandle = ActorService.Instance.ObjectTable.Get<ActorMemory>(handle.Address);
			if (actorMemHandle != null)
			{
				var pinned = new PinnedActor(actorMemHandle);

				Log.Information($"Pinning actor: {pinned}");

				await Dispatch.MainThread();
				Instance.PinnedActors.Add(pinned);
				Instance.PinnedActorCount = Instance.PinnedActors.Count;

				if (select)
					Instance.SelectActor(pinned);

				ActorPinned?.Invoke(pinned);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to pin actor");
		}
	}

	/// <summary>
	/// Gets the memory of the player target.
	/// </summary>
	/// <returns>The memory of the player target.</returns>
	public static ObjectHandle<GameObjectMemory>? GetTargetedActor()
	{
		Instance.UpdatePlayerTarget();
		return Instance.PlayerTargetHandle;
	}

	/// <summary>
	/// Unpins the specified actor from the list of pinned actors.
	/// </summary>
	/// <param name="actor">The actor to unpin.</param>
	public static void UnpinActor(PinnedActor actor)
	{
		Instance.PinnedActors.Remove(actor);

		if (actor.Memory == Instance.SelectedActor)
		{
			if (Instance.PinnedActors.Count > 0)
			{
				Instance.SelectActor(Instance.PinnedActors[0]);
			}
			else
			{
				Instance.SelectActor(null);
			}
		}

		Instance.PinnedActorCount = Instance.PinnedActors.Count;
		ActorUnPinned?.Invoke(actor);
		actor.Dispose();
	}

	/// <summary>
	/// Gets the pinned actor from the list of pinned actors.
	/// </summary>
	/// <param name="actor">The actor to search for.</param>
	/// <returns>The pinned actor if found, otherwise null.</returns>
	public static PinnedActor? GetPinned<T>(ObjectHandle<T> actor)
		where T : GameObjectMemory, new()
	{
		var targetId = actor.DoRef(a => a.Id);
		foreach (PinnedActor pinned in Instance.PinnedActors.ToList())
		{
			if (pinned.Memory == null)
				continue;

			if (pinned.Memory.Do(a => a.Id == targetId) == true)
				return pinned;
		}

		return null;
	}

	/// <summary>
	/// Checks if the specified actor is pinned.
	/// </summary>
	/// <param name="actor">The actor to check.</param>
	/// <returns>True if the actor is pinned, otherwise false.</returns>
	public static bool IsPinned<T>(ObjectHandle<T> actor)
		where T : GameObjectMemory, new() => GetPinned(actor) != null;

	/// <summary>
	/// Sets the player target to the specified pinned actor.
	/// </summary>
	/// <param name="actor">The actor to set as the new player target.</param>
	public static void SetPlayerTarget(PinnedActor actor)
	{
		if (actor.IsValid)
		{
			SetPlayerTarget(actor.Pointer);
		}
	}

	/// <summary>
	/// Sets the player target to the specified actor.
	/// </summary>
	/// <param name="actor">The actor to set as the new player target.</param>
	public static void SetPlayerTarget(GameObjectMemory actor)
	{
		if (actor.IsValid)
		{
			SetPlayerTarget(actor.Address);
		}
	}

	/// <summary>
	/// Gets the current player target actor from the list of pinned actors.
	/// </summary>
	/// <returns>The pinned actor if found, otherwise null.</returns>
	public static PinnedActor? GetPlayerTarget()
	{
		foreach (var pinned in Instance.PinnedActors)
		{
			if (Instance.PlayerTargetHandle != null && pinned.Pointer == Instance.PlayerTargetHandle.Address)
				return pinned;
		}

		return null;
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		// Register hotkeys only once
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned1", () => this.SelectActor(0));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned2", () => this.SelectActor(1));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned3", () => this.SelectActor(2));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned4", () => this.SelectActor(3));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned5", () => this.SelectActor(4));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned6", () => this.SelectActor(5));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned7", () => this.SelectActor(6));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned8", () => this.SelectActor(7));
		HotkeyService.RegisterHotkeyHandler("TargetService.NextPinned", this.NextPinned);
		HotkeyService.RegisterHotkeyHandler("TargetService.PrevPinned", this.PrevPinned);

		await base.Initialize();
	}

	/// <inheritdoc/>
	public override Task Shutdown()
	{
		GposeService.GposeStateChanged -= this.GposeService_GposeStateChanged;
		PoseService.EnabledChanged -= this.PoseService_EnabledChanged;
		PoseService.FreezeWorldPositionsEnabledChanged -= this.PoseService_EnabledChanged;
		return base.Shutdown();
	}

	/// <summary>Clears the actor selection.</summary>
	public void ClearSelection()
	{
		if (this.CurrentlyPinned == null)
			return;

		if (App.Current == null)
			return;

		App.Current.Dispatcher.Invoke(() =>
		{
			this.CurrentlyPinned = null;

			foreach (PinnedActor actor in this.PinnedActors)
			{
				actor.SelectionChanged();
			}
		});
	}

	/// <summary>
	/// Selects the first pinned actor if no actor is currently selected.
	/// </summary>
	public void EnsureSelection()
	{
		if (App.Current == null)
			return;

		if (this.SelectedActor != null)
			return;

		if (this.PinnedActors == null || this.PinnedActors.Count <= 0)
			return;

		this.SelectActor(this.PinnedActors[0]);
	}

	/// <summary>
	/// Selects the specified pinned actor.
	/// </summary>
	/// <param name="actor">The actor to select.</param>
	public void SelectActor(PinnedActor? actor)
	{
		App.Current.Dispatcher.Invoke(() =>
		{
			if (this.CurrentlyPinned == actor)
			{
				// Raise the event in case the underlying memory changed
				this.RaisePropertyChanged(nameof(TargetService.CurrentlyPinned));
				this.RaisePropertyChanged(nameof(TargetService.SelectedActor));
			}
			else
			{
				this.CurrentlyPinned = actor;
			}

			ActorSelected?.Invoke(actor?.Memory);

			foreach (PinnedActor ac in this.PinnedActors)
			{
				ac.SelectionChanged();
			}
		});
	}

	/// <summary>
	/// Selects the specified actor by its pinned index.
	/// </summary>
	/// <param name="index">The index of the actor to select.</param>
	/// <returns>True if the actor was selected, otherwise false.</returns>
	public bool SelectActor(int index)
	{
		if (index >= this.PinnedActors.Count || index < 0)
			return false;

		if (this.PinnedActors[index].IsSelected)
		{
			SetPlayerTarget(this.PinnedActors[index]);
		}
		else
		{
			this.SelectActor(this.PinnedActors[index]);
		}

		return true;
	}

	/// <summary>
	/// Gets the pinned index of the currently selected actor.
	/// </summary>
	/// <returns>The index of the selected actor, or 0 if none is selected.</returns>
	public int GetSelectedIndex()
	{
		for (int i = 0; i < this.PinnedActors.Count; i++)
		{
			if (this.PinnedActors[i].IsSelected)
			{
				return i;
			}
		}

		return 0;
	}

	/// <summary>
	/// Changes actor selection to the next pinned actor.
	/// It loops back to the first actor if the end of the list is reached.
	/// </summary>
	public void NextPinned()
	{
		int selectedIndex = this.GetSelectedIndex();
		selectedIndex++;

		if (selectedIndex >= this.PinnedActors.Count)
			selectedIndex = 0;

		this.SelectActor(selectedIndex);
	}

	/// <summary>
	/// Changes actor selection to the previous pinned actor.
	/// It loops back to the last actor if the beginning of the list is reached.
	/// </summary>
	public void PrevPinned()
	{
		int selectedIndex = this.GetSelectedIndex();
		selectedIndex--;

		if (selectedIndex < 0)
			selectedIndex = this.PinnedActors.Count - 1;

		this.SelectActor(selectedIndex);
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		GposeService.GposeStateChanged += this.GposeService_GposeStateChanged;
		PoseService.EnabledChanged += this.PoseService_EnabledChanged;
		PoseService.FreezeWorldPositionsEnabledChanged += this.PoseService_EnabledChanged;

		if (GameService.GetIsSignedIn())
		{
			try
			{
				bool isGpose = GposeService.IsInGpose();

				var actorHandles = ActorService.Instance.ObjectTable.GetAll();

				// We want the first non-hidden actor with a name in the same mode as the game
				foreach (var handle in actorHandles)
				{
					var result = handle.Do(a =>
					{
						if (a.IsDisposed)
							return false;

						// Ensure the actor data is up to date
						a.Synchronize();

						if (a.IsHidden)
							return false;

						if (string.IsNullOrEmpty(a.Name))
							return false;

						if (a.IsGPoseActor != isGpose)
							return false;

						return true;
					});

					if (result == true)
					{
						await PinActor(handle);
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to pin default actor");
			}
		}

		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.TickPinnedActors(this.CancellationToken));
		await base.OnStart();
	}

	private static void SetPlayerTarget(IntPtr? ptr)
	{
		if (ptr != null && ptr != IntPtr.Zero)
		{
			if (ActorService.Instance.ObjectTable.Contains((IntPtr)ptr))
			{
				if (GposeService.Instance.IsGpose)
				{
					MemoryService.Write(IntPtr.Add(AddressService.TargetSystem, AddressService.GPOSE_PLAYER_TARGET_OFFSET), (IntPtr)ptr);
				}
				else
				{
					MemoryService.Write(IntPtr.Add(AddressService.TargetSystem, AddressService.OVERWORLD_PLAYER_TARGET_OFFSET), (IntPtr)ptr);
				}
			}
		}
	}

	private void UpdatePlayerTarget()
	{
		IntPtr currentPlayerTargetPtr = IntPtr.Zero;

		try
		{
			currentPlayerTargetPtr = GposeService.Instance.IsGpose
				? AddressService.GPosePlayerTarget
				: AddressService.OverworldPlayerTarget;
		}
		catch
		{
			// If the memory read fails the target will be 0x0
		}

		try
		{
			bool wasNull = this.PlayerTargetHandle == null;
			bool willBeNull = currentPlayerTargetPtr == IntPtr.Zero;

			if (wasNull != willBeNull || (!willBeNull && (this.PlayerTargetHandle?.Address != currentPlayerTargetPtr)))
			{
				// Only raise property changed if transitioning between null and non-null
				if (willBeNull)
				{
					this.PlayerTargetHandle?.Dispose();
					this.PlayerTargetHandle = null;
				}
				else
				{
					this.PlayerTargetHandle = ActorService.Instance.ObjectTable.Get<GameObjectMemory>(currentPlayerTargetPtr);
				}

				this.RaisePropertyChanged(nameof(TargetService.PlayerTargetHandle));
				this.RaisePropertyChanged(nameof(TargetService.IsPlayerTargetPinnable));
			}
		}
		catch
		{
			// This section can only fail when FFXIV isn't running (fail to set address) so it should be safe to ignore
		}

		// Tick the actor if it still exists
		var handle = this.PlayerTargetHandle;
		if (handle?.IsValid == true)
		{
			try
			{
				PinnedActor? pinnedActor = null;
				for (int i = 0, count = this.PinnedActors.Count; i < count; i++)
				{
					var pinned = this.PinnedActors[i];
					if (pinned.Memory?.Address == handle.Address)
					{
						pinnedActor = pinned;
						break;
					}
				}

				// If the player target is pinned, synchronize through the pinned actor class. Otherwise synchronize directly.
				if (pinnedActor != null)
					pinnedActor.Tick();
				else
					handle.Do(a => a.Synchronize());
			}
			catch
			{
				// Should only fail to tick if the game isn't running
			}
		}
	}

	private async Task TickPinnedActors(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				this.UpdatePlayerTarget();

				for (int i = this.PinnedActors.Count - 1; i >= 0; i--)
				{
					// Skip the player target as it is already updated in the preceding function call
					if (this.PlayerTargetHandle != null && this.PinnedActors[i].Memory?.Address == this.PlayerTargetHandle.Address)
						continue;

					this.PinnedActors[i].Tick();
				}

				await Task.Delay(TASK_DELAY, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
			}
		}
	}

	private void RefreshActorRefreshStatus()
	{
		foreach (var pin in this.PinnedActors)
		{
			pin.Memory?.Do(a => a.RaiseRefreshChanged());
		}
	}

	private void GposeService_GposeStateChanged(bool newState) => this.RefreshActorRefreshStatus();
	private void PoseService_EnabledChanged(bool value) => this.RefreshActorRefreshStatus();
}