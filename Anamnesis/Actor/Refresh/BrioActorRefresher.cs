// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Brio;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
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

		if (PoseService.Instance.IsEnabled)
		{
			// Save the current pose
			PoseFile poseFile = new PoseFile();
			SkeletonVisual3d skeletonVisual3D = new SkeletonVisual3d();
			await skeletonVisual3D.SetActor(actor);
			poseFile.WriteToFile(actor, skeletonVisual3D, null);

			// Redraw
			var result = await Brio.Redraw(actor.ObjectIndex);

			if (result == "\"Full\"")
			{
				new Task(async () =>
				{
					await Task.Delay(500);
					await Dispatch.MainThread();

					// Restore current pose
					skeletonVisual3D = new SkeletonVisual3d();
					await skeletonVisual3D.SetActor(actor);
					poseFile.Apply(actor, skeletonVisual3D, null, PoseFile.Mode.All, true);
				}).Start();
			}
		}
		else
		{
			// Outside of pose mode we can just refresh
			await Brio.Redraw(actor.ObjectIndex);
		}
	}
}
