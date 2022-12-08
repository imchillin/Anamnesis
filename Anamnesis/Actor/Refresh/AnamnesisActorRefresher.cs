// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using System.Threading.Tasks;
using Anamnesis.Memory;
using Anamnesis.Services;
using static Anamnesis.Memory.ActorBasicMemory;

public class AnamnesisActorRefresher : IActorRefresher
{
	public bool CanRefresh(ActorMemory actor)
	{
		// Ana can't refresh gpose actors
		if (actor.IsGPoseActor)
			return false;

		return true;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		if (SettingsService.Current.EnableNpcHack)
		{
			await Task.Delay(16);

			if (actor.ObjectKind == ActorTypes.Player)
			{
				actor.ObjectKind = ActorTypes.BattleNpc;
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

			await Task.Delay(150);
		}
		else
		{
			await Task.Delay(16);

			actor.RenderMode = RenderModes.Unload;
			await Task.Delay(75);
			actor.RenderMode = RenderModes.Draw;

			await Task.Delay(150);
		}
	}
}
