// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Actor;
using Anamnesis.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
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
	public ushort Weather { get; set; }
	public byte DayOfMonth { get; set; }
	public long TimeOfDay { get; set; }

	public Vector RootPosition { get; set; }
	public Quaternion RootRotation { get; set; }

	public CameraShotFile? CameraShot { get; set; }

	public List<ActorEntry>? ActorEntries { get; set; }

	public async Task Apply(Mode mode)
	{
		if (TargetService.Instance.PinnedActors.Where(i => !i.IsValid || i.Memory?.ModelObject == null).Any())
			throw new Exception("All pinned actors must be valid and have a model");

		List<ActorMemory> actors = TargetService.Instance.PinnedActors.Select(i => i.Memory!).ToList();

		if (actors.Count != this.ActorEntries!.Count)
			throw new Exception($"Must have the same number of actors pinned: {this.ActorEntries.Count}");

		if (mode.HasFlag(Mode.Weather))
		{
			TerritoryService.Instance.CurrentWeatherId = this.Weather;
		}

		if (mode.HasFlag(Mode.Time))
		{
			TimeService.Instance.Freeze = true;
			TimeService.Instance.DayOfMonth = this.DayOfMonth;
			TimeService.Instance.TimeOfDay = this.TimeOfDay;
		}

		if (mode.HasFlag(Mode.WorldPosition))
		{
			if (TerritoryService.Instance.CurrentTerritoryId == this.Territory)
			{
				actors[0]!.ModelObject!.Transform!.Position = this.RootPosition;
				actors[0]!.ModelObject!.Transform!.Rotation = this.RootRotation;
			}
			else
			{
				throw new Exception("Could not restore world positions as you are not in the correct territory");
			}
		}

		if (mode.HasFlag(Mode.Camera))
		{
			ActorBasicMemory? targetActor = TargetService.Instance.PlayerTarget;
			if (targetActor == null || !targetActor.IsValid)
				return;

			if(targetActor.Address == IntPtr.Zero || targetActor.Address != actors[0]!.Address)
				throw new Exception("First pinned actor must be targeted by the camera to load camera angles");

			this.CameraShot!.Apply(CameraService.Instance, actors[0]!);
		}

		Vector rootPosition = actors[0]!.ModelObject!.Transform!.Position;
		Quaternion rootRotation = actors[0]!.ModelObject!.Transform!.Rotation;
		Quaternion invertedRootRotation = rootRotation;
		invertedRootRotation.Invert();

		for (int i = 0; i < actors.Count; i++)
		{
			ActorMemory actor = actors[i];
			ActorEntry entry = this.ActorEntries[i];

			if (mode.HasFlag(Mode.RelativePosition))
			{
				Vector rotatedRelativePosition = rootRotation * entry.Position;
				Quaternion rotatedRotation = rootRotation * entry.Rotation;

				actor.ModelObject!.Transform!.Position = rootPosition + rotatedRelativePosition;
				actor.ModelObject!.Transform!.Rotation = rotatedRotation;
			}

			if(mode.HasFlag(Mode.Pose))
			{
				SkeletonVisual3d skeleton = new();
				await skeleton.SetActor(actor);
				await entry.Pose!.Apply(actor, skeleton, null, PoseFile.Mode.Rotation);
			}
		}
	}

	public async Task WriteToFile()
	{
		this.Territory = TerritoryService.Instance.CurrentTerritoryId;
		this.Weather = TerritoryService.Instance.CurrentWeatherId;
		this.DayOfMonth = TimeService.Instance.DayOfMonth;
		this.TimeOfDay = TimeService.Instance.TimeOfDay;

		if(TargetService.Instance.PinnedActors.Where(i => !i.IsValid || i.Memory?.ModelObject == null).Any())
			throw new Exception("All pinned actors must be valid and have a model");

		List<ActorMemory> actors = TargetService.Instance.PinnedActors.Select(i => i.Memory!).ToList();

		this.RootPosition = actors[0]!.ModelObject!.Transform!.Position;
		this.RootRotation = actors[0]!.ModelObject!.Transform!.Rotation;

		ActorBasicMemory? targetActor = TargetService.Instance.PlayerTarget;
		if (targetActor == null || !targetActor.IsValid)
			return;

		if (targetActor.Address == IntPtr.Zero || targetActor.Address != actors[0]!.Address)
			throw new Exception("First pinned actor must be targeted by the camera to save");

		this.CameraShot = new();
		this.CameraShot.WriteToFile(CameraService.Instance, actors[0]);

		this.ActorEntries = new();

		Quaternion invertedRootRotation = this.RootRotation;
		invertedRootRotation.Invert();

		foreach (ActorMemory actor in actors)
		{
			Vector actorPosition = actor.ModelObject!.Transform!.Position;
			Vector actorScale = actor.ModelObject!.Transform!.Scale;
			Quaternion actorRotation = actor.ModelObject!.Transform!.Rotation;

			Vector relativePosition = actorPosition - this.RootPosition;

			Vector rotatedRelativePosition = invertedRootRotation * relativePosition;
			Quaternion rotatedRelativeRotation = actorRotation * invertedRootRotation;

			CharacterFile characterFile = new();
			characterFile.WriteToFile(actor, CharacterFile.SaveModes.All);

			PoseFile poseFile = new();
			SkeletonVisual3d skeleton = new();
			await skeleton.SetActor(actor);
			poseFile.WriteToFile(actor, skeleton, null);

			ActorEntry entry = new ActorEntry
			{
				Position = rotatedRelativePosition,
				Scale = actorScale,
				Rotation = rotatedRelativeRotation,
				Appearance = characterFile,
				Pose = poseFile,
			};

			this.ActorEntries.Add(entry);
		}
	}

	public class ActorEntry
	{
		public Vector Position { get; init; }
		public Vector Scale { get; init; }
		public Quaternion Rotation { get; init; }
		public CharacterFile? Appearance { get; set; }
		public PoseFile? Pose { get; set; }
	}
}
