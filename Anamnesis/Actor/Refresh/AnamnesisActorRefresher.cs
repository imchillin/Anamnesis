// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Memory;
using Anamnesis.Services;
using RemoteController.Interop.Delegates;
using Serilog;
using System;
using System.Threading.Tasks;

public enum RedrawStage
{
	Before,
	After,
}

public class AnamnesisActorRefresher : IActorRefresher
{
	public RedrawService RedrawService { get; } = new RedrawService();

	public bool CanRefresh(ActorMemory actor)
	{
		if (PoseService.Instance.IsEnabled)
			return false;

		// Ana can't refresh world frozen actors
		if (PoseService.Instance.FreezeWorldPosition)
			return false;

		return true;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		var handle = ActorService.Instance.ObjectTable.Get<GameObjectMemory>(actor.Address);
		if (handle == null)
			return;

		var isInGpose = GposeService.IsInGpose() ?? GposeService.Instance.IsGpose;
		if (SettingsService.Current.EnableNpcHack && !isInGpose && actor.ObjectKind == ActorTypes.Player)
		{
			// NOTE: This workaround is necessary only when we're in the overworld
			actor.ObjectKind = ActorTypes.EventNpc;
			await this.RedrawService.Redraw(handle);
			actor.ObjectKind = ActorTypes.Player;
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

	public async Task Redraw(ObjectHandle<GameObjectMemory> obj)
	{
		if (!obj.IsValid)
			return;

		string name = obj.DoRef(a => a.DisplayName) ?? "Unknown";

		int objectIndex = ActorService.Instance.ObjectTable.GetIndexOf(obj.Address);
		if (objectIndex == -1)
		{
			Log.Error($"Could not find the object index for the actor \"{name}\"[Index: {objectIndex}, Address: 0x{obj.Address:X}].");
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

			this.OnRedraw?.Invoke(obj, RedrawStage.After);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to redraw object \"{name}\"[Index: {objectIndex}, Address: 0x{obj.Address:X}].");
		}
	}
}
