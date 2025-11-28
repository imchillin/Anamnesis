// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Brio;
using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using Serilog;
using System.Threading.Tasks;
using XivToolsWpf;

public class BrioActorRefresher : IActorRefresher
{
	public bool CanRefresh(ActorMemory actor)
	{
		// Only if Brio integration is really enabled
		if (!SettingsService.Current.UseExternalRefreshBrio)
			return false;

		// Brio doesn't support refresh on world-frozen actors.
		// Trying to refresh a world-frozen actor will cause the actor to be
		// sent to world origin (0, 0, 0).
		if (PoseService.Instance.FreezeWorldPosition)
			return false;

		return true;
	}

	public async Task RefreshActor(ActorMemory actor)
	{
		await Dispatch.MainThread();

		bool doNpcHack = SettingsService.Current.EnableNpcHack && actor.ObjectKind == ActorTypes.Player;

		if (PoseService.Instance.IsEnabled)
		{
			// Save the current pose
			var poseFile = new PoseFile();
			var actorHandle = ActorService.Instance.ObjectTable.Get<ActorMemory>(actor.Address);
			if (actorHandle == null)
			{
				Log.Warning($"Failed to refresh actor {actor.Id} as they are not part of the object table");
				return;
			}

			var skeleton = new Skeleton(actorHandle);
			poseFile.WriteToFile(actorHandle, skeleton, null);

			// Redraw
			if (doNpcHack)
				actor.ObjectKind = ActorTypes.EventNpc;

			try
			{
				var result = await Brio.Redraw(actor.ObjectIndex);

				if (result == "\"Full\"")
				{
					new Task(async () =>
					{
						await Task.Delay(500);
						await Dispatch.MainThread();

						// Restore current pose
						skeleton = new Skeleton(actorHandle);
						poseFile.Apply(actorHandle, skeleton, null, PoseFile.Mode.All, true);
					}).Start();
				}
			}
			finally
			{
				if (doNpcHack)
					actor.ObjectKind = ActorTypes.Player;
			}
		}
		else
		{
			// Outside of pose mode we can just refresh
			if (doNpcHack)
				actor.ObjectKind = ActorTypes.EventNpc;

			try
			{
				await Brio.Redraw(actor.ObjectIndex);
			}
			finally
			{
				if (doNpcHack)
					actor.ObjectKind = ActorTypes.Player;
			}
		}
	}
}
