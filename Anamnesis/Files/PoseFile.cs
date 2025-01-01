// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Actor;
using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Posing;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public class PoseFile : JsonFileBase
{
	[Flags]
	public enum Mode
	{
		None = 0,
		Rotation = 1,
		Scale = 2,
		Position = 4,
		WorldRotation = 8,
		WorldScale = 16,

		All = Rotation | Scale | Position | WorldRotation | WorldScale,
	}

	public enum BoneProcessingModes
	{
		Ignore,
		KeepRelative,
		FullLoad,
	}

	public override string FileExtension => ".pose";
	public override string TypeName => "Anamnesis Pose";

	public Vector3? Position { get; set; }
	public Quaternion? Rotation { get; set; }
	public Vector3? Scale { get; set; }

	public Dictionary<string, Bone?>? Bones { get; set; }

	public static BoneProcessingModes GetBoneMode(ActorMemory? actor, Skeleton? skeleton, string boneName)
	{
		return boneName != "n_root" ? BoneProcessingModes.FullLoad : BoneProcessingModes.Ignore;
	}

	public static async Task<DirectoryInfo?> Save(DirectoryInfo? dir, ActorMemory? actor, Skeleton? skeleton, HashSet<string>? bones = null, bool editMeta = false)
	{
		if (actor == null || skeleton == null)
			return null;

		SaveResult result = await FileService.Save<PoseFile>(dir, FileService.DefaultPoseDirectory);

		if (result.Path == null)
			return null;

		PoseFile file = new PoseFile();
		file.WriteToFile(actor, skeleton, bones);

		using FileStream stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);

		if (editMeta)
			FileMetaEditor.Show(result.Path, file);

		return result.Directory;
	}

	public void WriteToFile(ActorMemory actor, Skeleton skeleton, HashSet<string>? bones)
	{
		if (actor.ModelObject == null || actor.ModelObject.Transform == null)
			throw new Exception("No model in actor");

		if (skeleton == null || skeleton.Bones == null)
			throw new Exception("No skeleton in actor");

		this.Rotation = actor.ModelObject.Transform.Rotation;
		this.Position = actor.ModelObject.Transform.Position;
		this.Scale = actor.ModelObject.Transform.Scale;

		this.Bones = new Dictionary<string, Bone?>();

		foreach (Core.Bone bone in skeleton.Bones.Values)
		{
			if (bones != null && !bones.Contains(bone.Name))
				continue;

			this.Bones.Add(bone.Name, new Bone(bone));
		}
	}

	public void Apply(ActorMemory actor, Skeleton skeleton, HashSet<string>? bones, Mode mode, bool doFacialExpressionHack)
	{
		if (actor == null)
			throw new ArgumentNullException(nameof(actor));

		if (actor.ModelObject == null || actor.ModelObject.Transform == null)
			throw new Exception("Actor has no model");

		if (actor.ModelObject.Skeleton == null)
			throw new Exception("Actor model has no skeleton. Are you trying to load a pose outside of GPose?");

		if (this.Bones == null)
			return;

		/*
		// Positions would be nice... but they are per-map!
		if (this.Position != null)
			actor.ModelObject.Transform.Position = this.Position;
		*/

		if (bones == null)
		{
			if (mode.HasFlag(Mode.WorldScale) && this.Scale.HasValue)
				actor.ModelObject.Transform.Scale = this.Scale.Value;

			if (mode.HasFlag(Mode.WorldRotation) && this.Rotation.HasValue)
				actor.ModelObject.Transform.Rotation = this.Rotation.Value;
		}

		SkeletonMemory skeletonMem = actor.ModelObject.Skeleton;

		PoseService.Instance.SetEnabled(true);
		PoseService.Instance.CanEdit = false;
		skeletonMem.PauseSynchronization = true;

		// Create a backup of all bone transforms.
		// Note: Unposed bone transforms are parent-relative, while the restore bone position list is character-relative.
		Dictionary<Core.Bone, Transform> unposedBoneTransforms = new();
		Dictionary<Core.Bone, Vector3> posedBonePos = new();
		List<Core.Bone> bonePosRestore = new();
		foreach (var bone in skeleton.Bones.Values)
		{
			if (GetBoneMode(actor, skeleton, bone.Name) == BoneProcessingModes.Ignore)
				continue;

			unposedBoneTransforms[bone] = new Transform
			{
				Position = bone.Position,
				Rotation = bone.Rotation,
				Scale = bone.Scale,
			};
		}

		// Facial expressions hack:
		// Since all facial bones are parented to the head, if we load the head rotation from
		// the pose that matches the expression, it wont break.
		// We then just set the head back to where it should be afterwards.
		// We can skip this if we actually intend to pose the head
		// in the case of posing by selected bones or by body.
		Core.Bone? headBone = skeleton.GetBone("j_kao");
		Quaternion? originalHeadRotation = null;
		Vector3? originalHeadPosition = null;
		if (doFacialExpressionHack && bones != null && bones.Contains("j_kao"))
		{
			headBone = skeleton.GetBone("j_kao") ?? throw new Exception("Unable to find head bone (j_kao).");
			originalHeadRotation = headBone?.Rotation;
			originalHeadPosition = headBone?.Position;
		}

		// Collect the parent-relative positions of all posed bones
		foreach ((string name, Bone? savedBone) in this.Bones)
		{
			if (savedBone == null)
				continue;

			string boneName = LegacyBoneNameConverter.GetModernName(name) ?? name;

			// Don't apply bones that cant be serialized.
			if (GetBoneMode(actor, skeleton, boneName) != BoneProcessingModes.FullLoad)
				continue;

			Core.Bone? bone = skeleton.GetBone(boneName);

			if (bone == null)
			{
				Log.Warning($"Bone: \"{boneName}\" not found");
				continue;
			}

			if (bones != null && !bones.Contains(boneName))
				continue;

			unposedBoneTransforms.Remove(bone);
			posedBonePos.TryAdd(bone, bone.Position);

			if (savedBone.Position != null && !mode.HasFlag(Mode.Position))
			{
				bonePosRestore.Add(bone);
			}
		}

		// Add unposed bones to the restore position bones list.
		// This is necessary to recover the positions of bones that are not explicitly
		// written to while "Freeze Position" is enabled.
		bonePosRestore.AddRange(unposedBoneTransforms.Keys.Where(unposedBone => !bonePosRestore.Contains(unposedBone)));

		// Record position changes if bones are posed without position to preserve positions.
		foreach (var bone in bonePosRestore)
		{
			if (bone.TransformMemory == null)
			{
				Log.Error($"Bone {bone.Name} has no transform memory");
			}

			if (!bone.TransformMemory!.Binds.TryGetValue("Position", out PropertyBindInfo? bindInfo))
			{
				Log.Error($"Failed to find position bind for bone: {bone.Name}");
				continue;
			}

			PropertyChange change = new(bindInfo, bone.TransformMemory.Position, bone.TransformMemory.Position, PropertyChange.Origins.User);
			change.ConfigureBindPath();
			actor.History.Record(change);
		}

		// Apply all transforms a few times to ensure parent-inherited values are calculated correctly, and to ensure
		// we dont end up with some values read during a ffxiv frame update.
		for (int i = 0; i < 3; i++)
		{
			foreach ((string name, Bone? savedBone) in this.Bones)
			{
				if (savedBone == null)
					continue;

				string boneName = LegacyBoneNameConverter.GetModernName(name) ?? name;

				// Don't apply bones that cant be serialized.
				if (GetBoneMode(actor, skeleton, boneName) != BoneProcessingModes.FullLoad)
					continue;

				Core.Bone? bone = skeleton.GetBone(boneName);

				if (bone == null)
				{
					Log.Warning($"Bone: \"{boneName}\" not found");
					continue;
				}

				if (bones != null && !bones.Contains(boneName))
					continue;

				foreach (TransformMemory transformMemory in bone.TransformMemories)
				{
					if (savedBone.Position != null && mode.HasFlag(Mode.Position) && bone.CanTranslate)
					{
						transformMemory.Position = (Vector3)savedBone.Position;
					}

					if (savedBone.Rotation != null && mode.HasFlag(Mode.Rotation) && bone.CanRotate)
					{
						transformMemory.Rotation = (Quaternion)savedBone.Rotation;
					}

					if (savedBone.Scale != null && mode.HasFlag(Mode.Scale) && bone.CanScale)
					{
						transformMemory.Scale = (Vector3)savedBone.Scale;
					}
				}

				bone.ReadTransform();
			}
		}

		// Restore the head bone rotation if we were only loading an expression
		if (headBone != null && originalHeadRotation != null && originalHeadPosition != null)
		{
			headBone.Rotation = (Quaternion)originalHeadRotation;
			headBone.Position = (Vector3)originalHeadPosition;
			headBone.WriteTransform(true);
		}

		// If we are not loading the position of bones, restore the positions of all bones that were not explicitly written to.
		if (!mode.HasFlag(Mode.Position))
		{
			var sortedBones = Core.Bone.SortBonesByHierarchy(posedBonePos.Keys);

			foreach (var bone in sortedBones)
			{
				bone.Position = posedBonePos[bone];
				bone.WriteTransform(true);
			}
		}

		// Restore the transforms of any bones that we did not explicitly write to.
		if (unposedBoneTransforms.Count > 0)
		{
			var sortedBones = Core.Bone.SortBonesByHierarchy(unposedBoneTransforms.Keys);

			foreach (var bone in sortedBones)
			{
				bone.Rotation = unposedBoneTransforms[bone].Rotation;
				bone.Position = unposedBoneTransforms[bone].Position;
				bone.Scale = unposedBoneTransforms[bone].Scale;
				bone.WriteTransform(false);
			}
		}

		skeletonMem.PauseSynchronization = false;
		skeletonMem.WriteDelayedBinds();

		PoseService.Instance.CanEdit = true;
	}

	public bool IsPreDTPoseFile()
	{
		if (this.Bones == null)
			return false;

		// Check to see if we have *any* face bones at all.
		// If not, then the file is backwards compatibile.
		bool hasFaceBones = false;
		foreach ((string name, Bone? bone) in this.Bones)
		{
			if (name.StartsWith("j_f_"))
			{
				hasFaceBones = true;
				break;
			}
		}

		// Looking for the tongue-A bone, a new bone common to all races and genders added in DT.
		// If we dont have it, we are assumed to be a pre-DT pose file.
		// This doesn't account for users manually editing the JSON.
		if (!hasFaceBones || !this.Bones.ContainsKey("j_f_bero_01"))
			return true;

		return false;
	}

	[Serializable]
	public class Bone
	{
		public Bone()
		{
		}

		public Bone(Core.Bone bone)
		{
			if (bone.TransformMemory != null)
			{
				this.Position = bone.TransformMemory.Position;
				this.Rotation = bone.TransformMemory.Rotation;
				this.Scale = bone.TransformMemory.Scale;
			}
		}

		public Vector3? Position { get; set; }
		public Quaternion? Rotation { get; set; }
		public Vector3? Scale { get; set; }
	}
}
