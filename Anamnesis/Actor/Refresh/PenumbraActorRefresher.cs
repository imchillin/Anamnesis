// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Memory;
using Anamnesis.Services;
using System.Threading.Tasks;
using XivToolsWpf;

public class PenumbraActorRefresher : IActorRefresher
{
	public RefreshBlockedReason GetRefreshAvailability(ActorMemory actor)
	{
		// Only if Penumbra integration is really enabled
		if (!SettingsService.Current.UseExternalRefresh)
			return RefreshBlockedReason.IntegrationDisabled;

		if (PoseService.Instance.IsEnabled)
			return RefreshBlockedReason.PoseEnabled;

		// Penumbra can't refresh world frozen actors
		if (PoseService.Instance.FreezeWorldState)
			return RefreshBlockedReason.WorldFrozen;

		// Penumbra can't refresh overworld actors in gpose
		if (GposeService.Instance.IsGpose && actor.IsOverworldActor)
			return RefreshBlockedReason.OverworldInGpose;

		return RefreshBlockedReason.None;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		await Dispatch.MainThread();

		bool doNpcHack = SettingsService.Current.EnableNpcHack && actor.ObjectKind == ActorTypes.Player;

		try
		{
			if (doNpcHack)
				actor.ObjectKind = ActorTypes.EventNpc;

			await Penumbra.Penumbra.Redraw(actor.Name, actor.ObjectIndex);
		}
		finally
		{
			if (doNpcHack)
				actor.ObjectKind = ActorTypes.Player;
		}
	}
}
