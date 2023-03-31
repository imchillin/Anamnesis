// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using System.Threading.Tasks;
using Anamnesis.Brio;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using XivToolsWpf;

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
		await Dispatch.MainThread();
		bool isPosing = PoseService.Instance.IsEnabled;

		RedrawType redrawType = RedrawType.AllowFull | RedrawType.PreservePosition | RedrawType.AllowOptimized | RedrawType.ForceAllowNPCAppearance;

		if(actor.IsWeaponDirty)
		{
			redrawType |= RedrawType.ForceRedrawWeaponsOnOptimized;
		}

		if (actor.IsOverworldActor || (actor.IsWeaponDirty && isPosing))
		{
			redrawType &= ~RedrawType.AllowOptimized;
		}

		if (isPosing)
		{
			// Save the current pose
			PoseFile poseFile = new PoseFile();
			SkeletonVisual3d skeletonVisual3D = new SkeletonVisual3d();
			await skeletonVisual3D.SetActor(actor);
			poseFile.WriteToFile(actor, skeletonVisual3D, null);

			// Redraw
			var result = await Brio.Redraw(actor.ObjectIndex, redrawType);
			if(result == "\"Full\"")
			{
				// TODO: It's probably best to find some way to detect when it's safe
				// this is a good first attempt though.
				new Task(async () =>
				{
					await Task.Delay(500);
					await Dispatch.MainThread();

					// Restore current pose
					skeletonVisual3D = new SkeletonVisual3d();
					await skeletonVisual3D.SetActor(actor);
					await poseFile.Apply(actor, skeletonVisual3D, null, PoseFile.Mode.All);
				}).Start();
			}
		}
		else
		{
			// Outside of pose mode we can just refresh
			await Brio.Redraw(actor.ObjectIndex, redrawType);
		}
	}
}
