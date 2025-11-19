// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Memory;
using Anamnesis.Services;
using System.Threading.Tasks;
using static Anamnesis.Memory.ActorBasicMemory;

public class AnamnesisActorRefresher : IActorRefresher
{
	public bool CanRefresh(ActorMemory actor)
	{
		if (PoseService.Instance.IsEnabled)
			return false;

		// Ana can't refresh gpose actors
		if (actor.IsGPoseActor)
			return false;

		// Ana can't refresh world frozen actors
		if (PoseService.Instance.FreezeWorldPosition)
			return false;

		return true;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		if (SettingsService.Current.EnableNpcHack && actor.ObjectKind == ActorTypes.Player)
		{
			actor.ObjectKind = ActorTypes.EventNpc;
			actor.RenderMode = RenderModes.Unload;
			await Task.Delay(75);
			actor.RenderMode = RenderModes.Draw;
			await Task.Delay(75);
			actor.ObjectKind = ActorTypes.Player;
			actor.RenderMode = RenderModes.Draw;
		}
		else
		{
			actor.RenderMode = RenderModes.Unload;
			await Task.Delay(75);
			actor.RenderMode = RenderModes.Draw;
		}
	}
}
