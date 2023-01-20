// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using System.Threading.Tasks;
using Anamnesis.Memory;
using Anamnesis.Services;
using static Anamnesis.Memory.ActorBasicMemory;

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
		if (SettingsService.Current.EnableNpcHack && actor.ObjectKind == ActorTypes.Player)
		{
			actor.ObjectKind = ActorTypes.BattleNpc;
			try
			{
				await Penumbra.Penumbra.Redraw(actor.ObjectIndex);
				await Task.Delay(200);
			}
			finally
			{
				actor.ObjectKind = ActorTypes.Player;
			}
		}
		else
		{
			await Penumbra.Penumbra.Redraw(actor.ObjectIndex);
		}
	}
}
