// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using System.Threading.Tasks;
using Anamnesis.Memory;
using Anamnesis.Services;
using static Anamnesis.Memory.ActorBasicMemory;

public class BrioActorRefresher : IActorRefresher
{
	public bool CanRefresh(ActorMemory actor)
	{
		// Only if Brio integration is really enabled
		if (!SettingsService.Current.UseExternalRefreshBrio)
			return false;

		return true;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		await Brio.Brio.Redraw(actor.ObjectIndex);
	}
}
