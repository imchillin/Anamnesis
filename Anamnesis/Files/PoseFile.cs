// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Posing;
	using Serilog;

	public class PoseFile : JsonFileBase
	{
		[Flags]
		public enum Mode
		{
			Rotation = 1,
			Scale = 2,
			Position = 4,

			All = Rotation | Scale | Position,
		}

		public override string FileExtension => ".pose";
		public override string TypeName => "Anamnesis Pose";

		public Vector? Position { get; set; }
		public Quaternion? Rotation { get; set; }
		public Vector? Scale { get; set; }

		public Dictionary<string, Bone?>? Bones { get; set; }

		public static async Task<DirectoryInfo?> Save(DirectoryInfo? dir, ActorMemory? actor, SkeletonVisual3d? skeleton, HashSet<string>? bones = null)
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
			return result.Directory;
		}

		public void WriteToFile(ActorMemory actor, SkeletonVisual3d skeleton, HashSet<string>? bones)
		{
			if (actor.ModelObject == null || actor.ModelObject.Transform == null)
				throw new Exception("No model in actor");

			if (skeleton == null || skeleton.Bones == null)
				throw new Exception("No skeleton in actor");

			this.Rotation = actor.ModelObject.Transform.Rotation;
			this.Position = actor.ModelObject.Transform.Position;
			this.Scale = actor.ModelObject.Transform.Scale;

			this.Bones = new Dictionary<string, Bone?>();

			foreach (BoneVisual3d bone in skeleton.Bones.Values)
			{
				if (bones != null && !bones.Contains(bone.BoneName))
					continue;

				this.Bones.Add(bone.BoneName, new Bone(bone));
			}
		}

		public async Task Apply(ActorMemory actor, SkeletonVisual3d skeleton, HashSet<string>? bones, Mode mode)
		{
			if (actor == null)
				throw new ArgumentNullException(nameof(actor));

			if (actor.ModelObject == null || actor.ModelObject.Transform == null)
				throw new Exception("Actor has no model");

			if (actor.ModelObject.Skeleton == null)
				throw new Exception("Actor model has no skeleton");

			if (this.Bones == null)
				return;

			/*
			// Positions would be nice... but they are per-map!
			if (this.Position != null)
				actor.ModelObject.Transform.Position = this.Position;
			*/

			if (this.Scale != null)
				actor.ModelObject.Transform.Scale = (Vector)this.Scale;

			if (this.Rotation != null)
				actor.ModelObject.Transform.Rotation = (Quaternion)this.Rotation;

			SkeletonMemory? skeletonMem = actor.ModelObject.Skeleton;

			PoseService.Instance.SetEnabled(true);
			PoseService.Instance.CanEdit = false;
			await Task.Delay(100);

			// Create a back up of the relative rotations of every bones
			Dictionary<string, Quaternion> unPosedBoneRotations = new Dictionary<string, Quaternion>();
			foreach ((string name, BoneVisual3d bone) in skeleton.Bones)
			{
				if (name == "n_root")
					continue;

				unPosedBoneRotations.Add(name, bone.Rotation);
			}

			// Facial expressions hack:
			// Since all facial bones are parented to the head, if we load the head rotation from
			// the pose that matches the expression, it wont break.
			// We then just set the head back to where it should be afterwards.
			BoneVisual3d? headBone = skeleton.GetBone("j_kao");
			Quaternion? originalHeadRotation = null;
			if (bones != null && bones.Contains("j_kao"))
			{
				if (headBone == null)
					throw new Exception("Unable to find head (j_kao) bone.");

				headBone.Tick();
				originalHeadRotation = headBone?.TransformMemory.Rotation;
			}

			// Apply all transforms a few times to ensure parent-inherited values are caluclated correctly, and to ensure
			// we dont end up with some values read during a ffxiv frame update.
			for (int i = 0; i < 3; i++)
			{
				foreach ((string name, Bone? savedBone) in this.Bones)
				{
					if (savedBone == null)
						continue;

					string boneName = name;
					string? modernName = LegacyBoneNameConverter.GetModernName(name);
					if (modernName != null)
						boneName = modernName;

					BoneVisual3d? bone = skeleton.GetBone(boneName);

					if (bone == null)
					{
						Log.Warning($"Bone: \"{boneName}\" not found");
						continue;
					}

					if (bones != null && !bones.Contains(boneName))
						continue;

					// Remove this bone from the relative rotations backup
					unPosedBoneRotations.Remove(boneName);

					foreach (TransformMemory transformMemory in bone.TransformMemories)
					{
						if (savedBone.Position != null && mode.HasFlag(Mode.Position) && bone.CanTranslate)
						{
							transformMemory.Position = (Vector)savedBone.Position;
						}

						if (savedBone.Rotation != null && mode.HasFlag(Mode.Rotation) && bone.CanRotate)
						{
							transformMemory.Rotation = (Quaternion)savedBone.Rotation;
						}

						if (savedBone.Scale != null && mode.HasFlag(Mode.Scale) && bone.CanScale)
						{
							transformMemory.Scale = (Vector)savedBone.Scale;
						}
					}

					bone.ReadTransform();
					bone.WriteTransform(skeleton, false);
				}

				await Task.Delay(1);
			}

			// Restore the head bone rotation if we were only loading an expression
			if (headBone != null && originalHeadRotation != null)
			{
				foreach (TransformMemory vm in headBone.TransformMemories)
				{
					vm.Rotation = (Quaternion)originalHeadRotation;
				}

				headBone.ReadTransform();
				headBone.WriteTransform(skeleton, true);
			}

			// Restore the relative rotations of any bones that we did not explicitly write to.
			foreach((string name, Quaternion rotation) in unPosedBoneRotations)
			{
				BoneVisual3d? bone = skeleton.GetBone(name);

				if (bone == null)
					continue;

				bone.Rotation = rotation;
				bone.WriteTransform(skeleton, false);
			}

			await Task.Delay(100);

			skeletonMem.Tick();

			PoseService.Instance.CanEdit = true;
		}

		[Serializable]
		public class Bone
		{
			public Bone()
			{
			}

			public Bone(BoneVisual3d boneVisual)
			{
				this.Position = boneVisual.TransformMemory.Position;
				this.Rotation = boneVisual.TransformMemory.Rotation;
				this.Scale = boneVisual.TransformMemory.Scale;
			}

			public Vector? Position { get; set; }
			public Quaternion? Rotation { get; set; }
			public Vector? Scale { get; set; }
		}
	}
}
