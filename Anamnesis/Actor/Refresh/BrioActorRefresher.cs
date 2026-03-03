// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Brio;
using Anamnesis.Memory;
using Anamnesis.Services;
using System.Threading.Tasks;
using XivToolsWpf;

public class BrioActorRefresher : IActorRefresher
{
	public RefreshBlockedReason GetRefreshAvailability(ActorMemory actor)
	{
		// Only if Brio integration is really enabled
		if (!SettingsService.Current.UseExternalRefreshBrio)
			return RefreshBlockedReason.IntegrationDisabled;

		if (PoseService.Instance.IsEnabled)
			return RefreshBlockedReason.PoseEnabled;

		// Brio doesn't support refresh on world-frozen actors.
		// Trying to refresh a world-frozen actor will cause the actor to be
		// sent to world origin (0, 0, 0).
		if (PoseService.Instance.FreezeWorldState)
			return RefreshBlockedReason.WorldFrozen;

		return RefreshBlockedReason.None;
	}

	public async Task RefreshActor(ActorMemory actor, bool forceReload = false)
	{
		await Dispatch.MainThread();

		bool doNpcHack = SettingsService.Current.EnableNpcHack && actor.ObjectKind == ObjectTypes.Player;

		try
		{
			if (doNpcHack)
				actor.ObjectKind = ObjectTypes.EventNpc;

			await Brio.Redraw(actor.ObjectIndex);
		}
		finally
		{
			if (doNpcHack)
				actor.ObjectKind = ObjectTypes.Player;
		}
	}
}
