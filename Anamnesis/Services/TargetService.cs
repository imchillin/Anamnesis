// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Anamnesis.Actor;
using Anamnesis.Core.Memory;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using PropertyChanged;
using XivToolsWpf;

public delegate void SelectionEvent(ActorMemory? actor);
public delegate void PinnedEvent(PinnedActor actor);

[AddINotifyPropertyChangedInterface]
public class TargetService : ServiceBase<TargetService>
{
	public static readonly int OverworldPlayerTargetOffset = 0x80;
	public static readonly int GPosePlayerTargetOffset = 0x98;

	public static event SelectionEvent? ActorSelected;
	public static event PinnedEvent? ActorPinned;
	public static event PinnedEvent? ActorUnPinned;

	public ActorBasicMemory PlayerTarget { get; private set; } = new();
	public bool IsPlayerTargetPinnable => this.PlayerTarget.Address != IntPtr.Zero && this.PlayerTarget.ObjectKind.IsSupportedType();

	public PinnedActor? CurrentlyPinned { get; private set; }
	public ObservableCollection<PinnedActor> PinnedActors { get; set; } = new ObservableCollection<PinnedActor>();

	[DependsOn(nameof(CurrentlyPinned))]
	public ActorMemory? SelectedActor => this.CurrentlyPinned?.Memory;

	public int PinnedActorCount { get; private set; }

	[DependsOn(nameof(PinnedActorCount))]
	public bool MoreThanFourPins => this.PinnedActorCount > 4;

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

	public static ActorBasicMemory GetTargetedActor()
	{
		Instance.UpdatePlayerTarget();
		return Instance.PlayerTarget;
	}

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

	public static PinnedActor? GetPinned(ActorBasicMemory actor)
	{
		foreach (PinnedActor pinned in TargetService.Instance.PinnedActors)
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

	public static bool IsPinned(ActorBasicMemory actor)
	{
		return GetPinned(actor) != null;
	}

	public static void SetPlayerTarget(PinnedActor actor)
	{
		if (actor.IsValid)
		{
			SetPlayerTarget(actor.Pointer);
		}
	}

	public static void SetPlayerTarget(ActorBasicMemory actor)
	{
		if (actor.IsValid)
		{
			SetPlayerTarget(actor.Address);
		}
	}

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

	public void UpdatePlayerTarget()
	{
		IntPtr currentPlayerTargetPtr = IntPtr.Zero;

		try
		{
			if (GposeService.Instance.IsGpose)
			{
				currentPlayerTargetPtr = MemoryService.Read<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset));
			}
			else
			{
				currentPlayerTargetPtr = MemoryService.Read<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset));
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
		if(this.PlayerTarget != null && this.PlayerTarget.Address != IntPtr.Zero)
		{
			try
			{
				this.PlayerTarget.Tick();
			}
			catch
			{
				// Should only fail to tick if the game isn't running
			}
		}
	}

	public override async Task Start()
	{
		await base.Start();

		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned1", () => this.SelectActor(0));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned2", () => this.SelectActor(1));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned3", () => this.SelectActor(2));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned4", () => this.SelectActor(3));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned5", () => this.SelectActor(4));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned6", () => this.SelectActor(5));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned7", () => this.SelectActor(6));
		HotkeyService.RegisterHotkeyHandler("TargetService.SelectPinned8", () => this.SelectActor(7));
		HotkeyService.RegisterHotkeyHandler("TargetService.NextPinned", () => this.NextPinned());
		HotkeyService.RegisterHotkeyHandler("TargetService.PrevPinned", () => this.PrevPinned());

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

				List<ActorBasicMemory> allActors = ActorService.Instance.GetAllActors();

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

		_ = Task.Run(this.TickPinnedActors);
	}

	public override Task Shutdown()
	{
		GposeService.GposeStateChanged -= this.GposeService_GposeStateChanged;
		PoseService.EnabledChanged -= this.PoseService_EnabledChanged;
		PoseService.FreezeWorldPositionsEnabledChanged -= this.PoseService_EnabledChanged;
		return base.Shutdown();
	}

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

	public void NextPinned()
	{
		int selectedIndex = this.GetSelectedIndex();
		selectedIndex++;

		if (selectedIndex >= this.PinnedActors.Count)
			selectedIndex = 0;

		this.SelectActor(selectedIndex);
	}

	public void PrevPinned()
	{
		int selectedIndex = this.GetSelectedIndex();
		selectedIndex--;

		if (selectedIndex < 0)
			selectedIndex = this.PinnedActors.Count - 1;

		this.SelectActor(selectedIndex);
	}

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

	private static void SetPlayerTarget(IntPtr? ptr)
	{
		if (ptr != null && ptr != IntPtr.Zero)
		{
			if (ActorService.Instance.IsActorInTable((IntPtr)ptr))
			{
				if (GposeService.Instance.IsGpose)
				{
					MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset), (IntPtr)ptr, "Update player target");
				}
				else
				{
					MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset), (IntPtr)ptr, "Update player target");
				}
			}
		}
	}

	private async Task TickPinnedActors()
	{
		while (this.IsAlive)
		{
			await Task.Delay(33);

			for (int i = this.PinnedActors.Count - 1; i >= 0; i--)
			{
				this.PinnedActors[i].Tick();
			}

			this.UpdatePlayerTarget();
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