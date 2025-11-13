// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Memory;
using Anamnesis.Services;
using System.Threading.Tasks;

public class PenumbraActorRefresher : IActorRefresher
{
	public bool CanRefresh(ActorMemory actor)
	{
		// Only if Penumbra integration is really enabled
		if (!SettingsService.Current.UseExternalRefresh)
			return false;

		if (PoseService.Instance.IsEnabled)
			return false;

		// Penumbra can't refresh world frozen actors
		if (PoseService.Instance.FreezeWorldPosition)
			return false;

		// Penumbra can't refresh overworld actors in gpose
		if (GposeService.Instance.IsGpose && actor.IsOverworldActor)
			return false;

		return true;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		bool doNpcHack = SettingsService.Current.EnableNpcHack && actor.ObjectKind == ActorTypes.Player;

		if (doNpcHack)
			actor.ObjectKind = ActorTypes.EventNpc;

		try
		{
			await Penumbra.Penumbra.Redraw(actor.Name, actor.ObjectIndex);
		}
		finally
		{
			if (doNpcHack)
				actor.ObjectKind = ActorTypes.Player;
		}
	}
}
