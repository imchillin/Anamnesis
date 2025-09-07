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
	public delegate void SelectionEvent(ActorMemory? actor);

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
	/// Gets the player target actor.
	/// </summary>
	public ActorBasicMemory PlayerTarget { get; private set; } = new();

	/// <summary>
	/// Gets a value indicating whether the player target is pinnable.
	/// </summary>
	public bool IsPlayerTargetPinnable => this.PlayerTarget.Address != IntPtr.Zero && this.PlayerTarget.ObjectKind.IsSupportedType();

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
	public ActorMemory? SelectedActor => this.CurrentlyPinned?.Memory;

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
	/// <param name="basicActor">The actor to pin.</param>
	/// <param name="select">A flag indicating whether to select the actor after pinning.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public static async Task PinActor(ActorBasicMemory basicActor, bool select = false)
	{
		if (basicActor.Address == IntPtr.Zero)
			return;

		if (!basicActor.ObjectKind.IsSupportedType())
		{
			Log.Warning($"You cannot pin actor of type: {basicActor.ObjectKind}");
			return;
		}

		try
		{
			foreach (PinnedActor otherActor in Instance.PinnedActors)
			{
				if (basicActor.Address == otherActor.Pointer)
				{
					Log.Information($"Actor already pinned: {otherActor}");
					Instance.SelectActor(otherActor);
					return;
				}
			}

			ActorMemory memory = new();

			if (basicActor is ActorMemory actorMemory)
				memory = actorMemory;

			memory.SetAddress(basicActor.Address);
			PinnedActor pined = new PinnedActor(memory);

			Log.Information($"Pinning actor: {pined}");

			await Dispatch.MainThread();
			Instance.PinnedActors.Add(pined);
			Instance.PinnedActorCount = Instance.PinnedActors.Count;

			if (select)
				Instance.SelectActor(pined);

			ActorPinned?.Invoke(pined);
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
	public static ActorBasicMemory GetTargetedActor()
	{
		Instance.UpdatePlayerTarget();
		return Instance.PlayerTarget;
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
	public static PinnedActor? GetPinned(ActorBasicMemory actor)
	{
		foreach (PinnedActor pinned in TargetService.Instance.PinnedActors.ToList())
		{
			if (pinned.Memory == null)
				continue;

			if (pinned.Memory.Id == actor.Id)
			{
				return pinned;
			}
		}

		return null;
	}

	/// <summary>
	/// Checks if the specified actor is pinned.
	/// </summary>
	/// <param name="actor">The actor to check.</param>
	/// <returns>True if the actor is pinned, otherwise false.</returns>
	public static bool IsPinned(ActorBasicMemory actor) => GetPinned(actor) != null;

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
	public static void SetPlayerTarget(ActorBasicMemory actor)
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
			if (pinned.Pointer == Instance.PlayerTarget.Address)
			{
				return pinned;
			}
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

#if DEBUG
		if (MemoryService.Process == null)
		{
			await TargetService.PinActor(new DummyActor(1));
			await TargetService.PinActor(new DummyActor(2));
			return;
		}
#endif

		if (GameService.GetIsSignedIn())
		{
			try
			{
				bool isGpose = GposeService.GetIsGPose();

				List<ActorBasicMemory> allActors = ActorService.Instance.GetAllActors(true);

				// We want the first non-hidden actor with a name in the same mode as the game
				foreach (ActorBasicMemory actor in allActors)
				{
					if (actor.IsHidden)
						continue;

					if (string.IsNullOrEmpty(actor.Name))
						continue;

					if (actor.IsGPoseActor != isGpose)
						continue;

					await PinActor(actor);
					break;
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
			if (ActorService.Instance.IsActorInTable((IntPtr)ptr))
			{
				if (GposeService.Instance.IsGpose)
				{
					MemoryService.Write(IntPtr.Add(AddressService.TargetSystem, AddressService.GPOSE_PLAYER_TARGET_OFFSET), (IntPtr)ptr, "Update player target");
				}
				else
				{
					MemoryService.Write(IntPtr.Add(AddressService.TargetSystem, AddressService.OVERWORLD_PLAYER_TARGET_OFFSET), (IntPtr)ptr, "Update player target");
				}
			}
		}
	}

	private void UpdatePlayerTarget()
	{
		IntPtr currentPlayerTargetPtr = IntPtr.Zero;

		try
		{
			if (GposeService.Instance.IsGpose)
			{
				currentPlayerTargetPtr = AddressService.GPosePlayerTarget;
			}
			else
			{
				currentPlayerTargetPtr = AddressService.OverworldPlayerTarget;
			}
		}
		catch
		{
			// If the memory read fails the target will be 0x0
		}

		try
		{
			if (currentPlayerTargetPtr != this.PlayerTarget.Address)
			{
				if (currentPlayerTargetPtr == IntPtr.Zero)
				{
					this.PlayerTarget.Dispose();
				}
				else
				{
					this.PlayerTarget.SetAddress(currentPlayerTargetPtr);
				}

				this.RaisePropertyChanged(nameof(TargetService.PlayerTarget));
				this.RaisePropertyChanged(nameof(TargetService.IsPlayerTargetPinnable));
			}
		}
		catch
		{
			// This section can only fail when FFXIV isn't running (fail to set address) so it should be safe to ignore
		}

		// Tick the actor if it still exists
		if (this.PlayerTarget != null && this.PlayerTarget.Address != IntPtr.Zero)
		{
			try
			{
				var pinnedActor = this.PinnedActors.FirstOrDefault(pinned => pinned.Memory?.Address == this.PlayerTarget.Address);

				// If the player target is pinned, synchronize through the pinned actor class. Otherwise synchronize directly.
				if (pinnedActor != null)
					pinnedActor.Tick();
				else
					this.PlayerTarget.Synchronize();
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
					if (this.PinnedActors[i].Memory?.Address == this.PlayerTarget.Address)
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
			if (pin.Memory?.IsValid == true)
			{
				pin.Memory.RaiseRefreshChanged();
			}
		}
	}

	private void GposeService_GposeStateChanged(bool newState) => this.RefreshActorRefreshStatus();
	private void PoseService_EnabledChanged(bool value) => this.RefreshActorRefreshStatus();
}