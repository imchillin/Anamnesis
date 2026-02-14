// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Actor.Pages;
using Anamnesis.Actor.Posing;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using RemoteController.Interop.Delegates;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public enum RedrawStage
{
	Before,
	After,
}

public class AnamnesisActorRefresher : IActorRefresher
{
	public RedrawService RedrawService { get; } = new RedrawService();

	public RefreshBlockedReason GetRefreshAvailability(ActorMemory actor)
	{
		if (PoseService.Instance.IsEnabled)
			return RefreshBlockedReason.PoseEnabled;

		// Ana can't refresh world frozen actors
		if (PoseService.Instance.FreezeWorldState)
			return RefreshBlockedReason.WorldFrozen;

		return RefreshBlockedReason.None;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		var handle = ActorService.Instance.ObjectTable.Get<GameObjectMemory>(actor.Address);
		if (handle == null)
			return;

		var actorHandle = ActorService.Instance.ObjectTable.Get<ActorMemory>(handle.Address);
		if (actorHandle == null)
			return;

		// Capture current state before redraw
		var currentSnapshot = this.CaptureSnapshot(actorHandle);

		// Compute what changed since last snapshot
		var changeset = this.ComputeChangeset(actor, currentSnapshot);
		Log.Information($"Computed changeset: {changeset}");

		if (!changeset.HasChanges)
		{
			Log.Information("No appearance changes detected, skipping redraw");
			this.UpdateSnapshot(actor, currentSnapshot);
			return;
		}

		// Try optimized redraw for non-structural changes
		if (await this.TryOptimizedRedrawAsync(actor, changeset))
		{
			this.UpdateSnapshot(actor, currentSnapshot);
			return;
		}

		// Fall back to full redraw
		Log.Information($"Performing full redraw (Structural: {changeset.HasStructuralChanges}, IsHuman: {actor.IsHuman})");
		await this.PerformFullRedrawAsync(actor, handle);

		// Update snapshot after redraw
		this.UpdateSnapshot(actor, currentSnapshot);
	}

	private CharacterFile CaptureSnapshot(ObjectHandle<ActorMemory> actor)
	{
		var snapshot = new CharacterFile();
		snapshot.WriteToFile(actor, CharacterFile.SaveModes.All);
		Log.Verbose($"Captured appearance snapshot for actor at 0x{actor.Address:X}");
		return snapshot;
	}

	private CharacterFile.CharChangeSet ComputeChangeset(ActorMemory actor, CharacterFile currentSnapshot)
	{
		if (actor.LastAppearanceSnapshot == null)
			return CharacterFile.CharChangeSet.FullRedraw;

		return actor.LastAppearanceSnapshot.CompareTo(currentSnapshot, CharacterFile.SaveModes.All);
	}

	private void UpdateSnapshot(ActorMemory actor, CharacterFile snapshot)
	{
		actor.LastAppearanceSnapshot = snapshot;
		Log.Verbose($"Updated appearance snapshot for actor at 0x{actor.Address:X}");
	}

	private async Task<bool> TryOptimizedRedrawAsync(ActorMemory actor, CharacterFile.CharChangeSet changeset)
	{
		if (!changeset.CanUseOptimizedRedraw || !actor.IsHuman)
			return false;

		try
		{
			// TODO: Brio uses LoadWeapon and SetGlasses (DrawDataContainer) for updating facewear and weapons.

			var drawData = actor.BuildDrawData();
			bool skipEquipment = !changeset.HasEquipmentChanges;

			if (actor.UpdateDrawData(in drawData, skipEquipment))
			{
				Log.Information($"Applied optimized redraw (skipped equipment: {skipEquipment})");
				return true;
			}

			Log.Information("Optimized UpdateDrawData failed, falling back to full redraw");
		}
		catch (Exception ex)
		{
			Log.Warning(ex, $"Optimized redraw failed for actor at 0x{actor.Address:X}");
		}

		return false;
	}

	private async Task PerformFullRedrawAsync(ActorMemory actor, ObjectHandle<GameObjectMemory> handle)
	{
		var isInGpose = GposeService.IsInGpose() ?? GposeService.Instance.IsGpose;
		if (SettingsService.Current.EnableNpcHack && !isInGpose && actor.ObjectKind == ObjectTypes.Player)
		{
			// NOTE: This workaround is necessary only when we're in the overworld
			actor.ObjectKind = ObjectTypes.EventNpc;
			await this.RedrawService.Redraw(handle);
			actor.ObjectKind = ObjectTypes.Player;
		}
		else
		{
			await this.RedrawService.Redraw(handle);
		}
	}
}

public class RedrawService
{
	private static HookHandle? s_enableDrawHook = null;
	private static HookHandle? s_disableDrawHook = null;

	public RedrawService()
	{
		s_enableDrawHook = ControllerService.Instance.RegisterWrapper<Character.EnableDraw>();
		s_disableDrawHook = ControllerService.Instance.RegisterWrapper<Character.DisableDraw>();
	}

	public delegate void RedrawEvent(ObjectHandle<GameObjectMemory> obj, RedrawStage stage);
	public event RedrawEvent? OnRedraw;

	public static async Task WaitForDrawing(int objectIndex)
	{
		if (ControllerService.Instance == null || ControllerService.Instance.Framework?.Active != true)
			return;

		IntPtr currentPtr = ActorService.Instance.ObjectTable.GetAddress(objectIndex);
		using var obj = new ObjectHandle<GameObjectMemory>(currentPtr, ActorService.Instance.ObjectTable);

		const int maxAttempts = 100;
		const int delayMs = 16;
		int attempts = 0;

		while (attempts < maxAttempts)
		{
			if (obj.Do(o => o.ModelObject?.IsVisible == true) == true)
				return;

			await Task.Delay(delayMs);
			attempts++;
		}
	}

	public async Task Redraw(ObjectHandle<GameObjectMemory> obj)
	{
		if (!obj.IsValid)
			return;

		string id = obj.DoRef(a => a.IdNoAddress) ?? "Unknown";

		int objectIndex = ActorService.Instance.ObjectTable.GetIndexOf(obj.Address);
		if (objectIndex == -1)
		{
			Log.Error($"Could not find the object index for the actor ID \"{id}\"[Index: {objectIndex}, Address: 0x{obj.Address:X}].");
			return;
		}

		try
		{
			this.OnRedraw?.Invoke(obj, RedrawStage.Before);

			using (var batch = ControllerService.Instance.CreateBatchInvoke())
			{
				batch.Mode = RemoteController.Interop.DispatchMode.FrameworkTick;

				IntPtr objPtr = ActorService.Instance.ObjectTable.GetAddress(objectIndex);
				batch.AddCall<long>(s_disableDrawHook!, args: objPtr);
				batch.AddCall<byte>(s_enableDrawHook!, args: objPtr);
			}

			await WaitForDrawing(objectIndex);

			this.OnRedraw?.Invoke(obj, RedrawStage.After);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to redraw object ID \"{id}\"[Index: {objectIndex}, Address: 0x{obj.Address:X}].");
		}
	}
}
