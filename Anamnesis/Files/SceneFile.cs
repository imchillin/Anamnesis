// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Core;
using Anamnesis.Core.Extensions;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

[Serializable]
public class SceneFile : JsonFileBase
{
	[Flags]
	public enum Mode
	{
		Pose = 1,
		RelativePosition = 2,
		WorldPosition = 4,
		Camera = 8,
		Weather = 16,
		Time = 32,

		All = Pose | RelativePosition | WorldPosition | Camera,
	}

	public override string FileExtension => ".scene";
	public override string TypeName => "Anamnesis Scene";

	public uint Territory { get; set; }
	public uint Weather { get; set; }
	public byte DayOfMonth { get; set; }
	public long TimeOfDay { get; set; }

	public CameraShotFile? CameraShot { get; set; }

	public string RootActorName { get; set; } = string.Empty;
	public string TargetActorName { get; set; } = string.Empty;
	public Dictionary<string, ActorEntry> ActorEntries { get; set; } = new();

	public async Task Apply(Mode mode)
	{
		await Task.Yield();

		if (TargetService.Instance.PinnedActors.Where(i => !i.IsValid || i.Memory?.Do(a => a.ModelObject) == null).Any())
			throw new Exception("All pinned actors must be valid and have a model");

		// First up, ensure we have the actors mapped
		Dictionary<string, ObjectHandle<ActorMemory>?> actors = new();
		foreach ((string name, ActorEntry entry) in this.ActorEntries)
		{
			actors.Add(name, GetPinnedActor(name));
		}

		// TODO: present a dialog allowing the user to set the nicknames of actors to
		// the remainder in the usedNames array.
		int missingCount = 0;
		foreach ((string name, ObjectHandle<ActorMemory>? memory) in actors)
		{
			if (memory == null)
			{
				missingCount++;
			}
		}

		if (missingCount > 0)
		{
			if (await GenericDialog.ShowAsync($"{missingCount} actors not accounted for", "Warning", System.Windows.MessageBoxButton.OKCancel) != true)
			{
				return;
			}
		}

		// Find the root actor
		ObjectHandle<ActorMemory>? rootActor = GetPinnedActor(this.RootActorName);
		ActorEntry rootActorEntry = this.ActorEntries[this.RootActorName];

		if (rootActor == null)
			throw new Exception($"Failed to locate root actor: {this.RootActorName} in scene");

		// Find the target actor
		ObjectHandle<ActorMemory>? targetActor = GetPinnedActor(this.TargetActorName);
		ActorEntry taretActorEntry = this.ActorEntries[this.TargetActorName];

		if (targetActor == null)
			throw new Exception($"Failed to locate target actor: {this.TargetActorName} in scene");

		var pinnedTarget = TargetService.GetPinned(targetActor);
		if (pinnedTarget == null)
			throw new Exception($"Failed to locate pinned target actor: {this.TargetActorName} in scene");

		TargetService.SetPlayerTarget(pinnedTarget);

		if (mode.HasFlagUnsafe(Mode.Weather))
			TerritoryService.Instance.CurrentWeatherId = this.Weather;

		if (mode.HasFlagUnsafe(Mode.Time))
		{
			TimeService.Instance.Freeze = true;
			TimeService.Instance.DayOfMonth = this.DayOfMonth;
			TimeService.Instance.TimeOfDay = this.TimeOfDay;
		}

		if (mode.HasFlagUnsafe(Mode.WorldPosition))
		{
			if (TerritoryService.Instance.CurrentTerritoryId != this.Territory)
				throw new Exception("Could not restore world positions as you are not in the correct territory");

			rootActor.Do(a => a.ModelObject!.Transform!.Position = rootActorEntry.Position);
			rootActor.Do(a => a.ModelObject!.Transform!.Rotation = rootActorEntry.Rotation);
		}

		Vector3 rootPosition = rootActor!.Do(a => a.ModelObject!.Transform!.Position);
		Quaternion rootRotation = new Quaternion(0, 0, 0, 1) * rootActorEntry.Rotation;
		rootActor.Do(a => a.ModelObject!.Transform!.Rotation = rootRotation);

		// Adjust for waist
		var rootSkeleton = new Skeleton(rootActor);
		Vector3 rootOriginalWaist = rootActorEntry.Pose?.Bones?["n_hara"]?.Position ?? Vector3.Zero;
		Vector3 rootCurrentWaist = rootSkeleton.GetBone("n_hara")?.Position ?? Vector3.Zero;
		Vector3 rootAdjustedWaist = Vector3.Transform(rootCurrentWaist - rootOriginalWaist, rootRotation);

		if (mode.HasFlagUnsafe(Mode.WorldPosition))
		{
			rootActor!.Do(a => a.ModelObject!.Transform!.Position -= rootAdjustedWaist);
		}

		foreach ((string name, ObjectHandle<ActorMemory>? actor) in actors)
		{
			if (actor == null)
				continue;

			var skeleton = new Skeleton(actor);
			ActorEntry entry = this.ActorEntries[name];

			if (actor != rootActor && mode.HasFlagUnsafe(Mode.RelativePosition))
			{
				Quaternion rotatedRotation = rootRotation * entry.Rotation;

				Vector3 originalWaist = entry.Pose?.Bones?["n_hara"]?.Position ?? Vector3.Zero;
				Vector3 currentWaist = rootSkeleton.GetBone("n_hara")?.Position ?? Vector3.Zero;
				Vector3 adjustedWaist = Vector3.Transform(currentWaist - originalWaist, rotatedRotation);

				Vector3 rotatedRelativePosition = Vector3.Transform(entry.Position, rootRotation);
				actor.Do(a => a.ModelObject!.Transform!.Position = rootPosition + rotatedRelativePosition - adjustedWaist);
				actor.Do(a => a.ModelObject!.Transform!.Rotation = rotatedRotation);

				if (!mode.HasFlagUnsafe(Mode.WorldPosition))
				{
					actor.Do(a => a.ModelObject!.Transform!.Position += rootAdjustedWaist);
				}
			}

			if (mode.HasFlagUnsafe(Mode.Pose))
			{
				// TODO: This should follow the same approach as the pose page imports
				entry.Pose!.Apply(actor, skeleton, null, PoseFile.Mode.Rotation, true);
			}
		}

		if (mode.HasFlagUnsafe(Mode.Camera))
		{
			var basicHandle = ActorService.InstanceOrNull?.ActorTable.Get<ActorBasicMemory>(targetActor.Address);
			if (basicHandle != null)
			{
				this.CameraShot!.Apply(CameraService.Instance, basicHandle);
			}
		}
	}

	public void WriteToFile()
	{
		this.Territory = TerritoryService.Instance.CurrentTerritoryId;
		this.Weather = TerritoryService.Instance.CurrentWeatherId;
		this.DayOfMonth = TimeService.Instance.DayOfMonth;
		this.TimeOfDay = TimeService.Instance.TimeOfDay;

		if (TargetService.Instance.PinnedActors.Where(i => !i.IsValid || i.Memory?.Do(a => a.ModelObject) == null).Any())
			throw new Exception("All pinned actors must be valid and have a model");

		var actorHandles = TargetService.Instance.PinnedActors.Select(i => i.Memory!).ToList();

		if (actorHandles.Count == 0)
			throw new Exception("No pinned actors found");

		ObjectHandle<ActorMemory> rootActor = actorHandles[0];
		this.RootActorName = rootActor.Do(a => a.DisplayName) ?? "Unknown";

		PinnedActor? targetActor = TargetService.GetPlayerTarget();
		if (targetActor == null || !targetActor.IsValid || targetActor.Memory == null)
			throw new Exception("Targeted actor must be pinned");

		this.TargetActorName = targetActor.DisplayName ?? "Unknown";

		this.CameraShot = new();
		var basicHandle = ActorService.InstanceOrNull?.ActorTable.Get<ActorBasicMemory>(targetActor.Memory.Address);
		if (basicHandle != null)
		{
			this.CameraShot.WriteToFile(CameraService.Instance, basicHandle);
		}

		this.ActorEntries = new();

		Vector3 rootPosition = rootActor!.Do(a => a.ModelObject!.Transform!.Position);
		Quaternion rootRotation = rootActor!.Do(a => a.ModelObject!.Transform!.Rotation);
		Quaternion invertedRootRotation = Quaternion.Inverse(rootRotation);

		foreach (ObjectHandle<ActorMemory> actor in actorHandles)
		{
			Vector3 actorPosition = actor.Do(a => a.ModelObject!.Transform!.Position);
			Vector3 actorScale = actor.Do(a => a.ModelObject!.Transform!.Scale);
			Quaternion actorRotation = actor.Do(a => a.ModelObject!.Transform!.Rotation);

			Vector3 relativePosition = actorPosition - rootPosition;

			Vector3 rotatedRelativePosition = Vector3.Transform(relativePosition, invertedRootRotation);
			Quaternion rotatedRelativeRotation = invertedRootRotation * actorRotation;

			CharacterFile characterFile = new();
			characterFile.WriteToFile(actor, CharacterFile.SaveModes.All);

			PoseFile poseFile = new();
			var skeleton = new Skeleton(actor);
			poseFile.WriteToFile(actor, skeleton, null);

			ActorEntry entry = new();

			if (actor == rootActor)
			{
				entry.Position = rootPosition;
				entry.Rotation = rootRotation;
			}
			else
			{
				entry.Position = rotatedRelativePosition;
				entry.Rotation = rotatedRelativeRotation;
			}

			entry.Scale = actorScale;
			entry.Appearance = characterFile;
			entry.Pose = poseFile;

			var displayName = actor.Do(a => a.DisplayName) ?? "Unknown";
			if (this.ActorEntries.ContainsKey(displayName))
				throw new Exception($"Duplicate actor name: {displayName} in scene.");

			this.ActorEntries.Add(displayName, entry);
		}
	}

	private static ObjectHandle<ActorMemory>? GetPinnedActor(string name)
	{
		foreach (PinnedActor pinnedActor in TargetService.Instance.PinnedActors.ToList())
		{
			var actorHandle = pinnedActor.Memory;
			if (actorHandle == null)
				continue;

			if (actorHandle.Do(a => a.DisplayName == name))
				return actorHandle;
		}

		return null;
	}

	public class ActorEntry
	{
		public Vector3 Position { get; set; }
		public Vector3 Scale { get; set; }
		public Quaternion Rotation { get; set; }
		public CharacterFile? Appearance { get; set; }
		public PoseFile? Pose { get; set; }
	}
}
