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

		public Dictionary<string, Bone?>? Bones { get; set; }

		public static async Task<DirectoryInfo?> Save(DirectoryInfo? dir, ActorMemory? actor, SkeletonVisual3d? skeleton, bool selectionOnly)
		{
			if (actor == null || skeleton == null)
				return null;

			SaveResult result = await FileService.Save<PoseFile>(dir, FileService.DefaultPoseDirectory);

			if (result.Path == null)
				return null;

			PoseFile file = new PoseFile();
			file.WriteToFile(actor, skeleton, selectionOnly);

			using FileStream stream = new FileStream(result.Path.FullName, FileMode.Create);
			file.Serialize(stream);
			return result.Directory;
		}

		public void WriteToFile(ActorMemory actor, SkeletonVisual3d skeleton, bool selectionOnly)
		{
			////SkeletonViewModel? skeletonMem = actor?.ModelObject?.Skeleton?.Skeleton;

			if (skeleton == null || skeleton.Bones == null)
				throw new Exception("No skeleton in actor");

			this.Bones = new Dictionary<string, Bone?>();

			foreach (BoneVisual3d bone in skeleton.Bones)
			{
				if (selectionOnly && !skeleton.GetIsBoneSelected(bone))
					continue;

				this.Bones.Add(bone.BoneName, new Bone(bone.ViewModel));
			}
		}

		public async Task Apply(ActorMemory actor, SkeletonVisual3d skeleton, bool selectionOnly, Mode mode)
		{
			if (actor == null)
				throw new ArgumentNullException(nameof(actor));

			if (actor.ModelObject == null)
				throw new Exception("Actor has no model");

			if (actor.ModelObject.Skeleton == null)
				throw new Exception("Actor model has no skeleton wrapper");

			if (actor.ModelObject.Skeleton.Skeleton == null)
				throw new Exception("Actor skeleton wrapper has no skeleton");

			SkeletonMemory skeletonMem = actor.ModelObject.Skeleton.Skeleton;

			PoseService.Instance.SetEnabled(true);
			PoseService.Instance.CanEdit = false;
			await Task.Delay(100);

			// Facial expressions hack:
			// Since all facial bones are parented to the head, if we load the head rotation from
			// the pose that matches the expression, it wont break.
			// We then just set the head back to where it should be afterwards.
			BoneVisual3d? headBone = skeleton.GetIsHeadSelection() ? skeleton.GetBone("Head") : null;
			headBone?.ReadTransform(true);
			Quaternion? originalHeadRotation = headBone?.ViewModel.Rotation;

			if (this.Bones != null)
			{
				// Apply all transforms a few times to ensure parent-inherited values are caluclated correctly, and to ensure
				// we dont end up with some values read during a ffxiv frame update.
				for (int i = 0; i < 3; i++)
				{
					foreach ((string name, Bone? savedBone) in this.Bones)
					{
						if (savedBone == null)
							continue;

						BoneVisual3d? bone = skeleton.GetBone(name);

						if (bone == null)
						{
							Log.Warning($"Bone: \"{name}\" not found");
							continue;
						}

						if (selectionOnly && !skeleton.GetIsBoneSelected(bone))
							continue;

						TransformMemory vm = bone.ViewModel;

						if (savedBone.Position != null && mode.HasFlag(Mode.Position))
						{
							vm.Position = (Vector)savedBone.Position;
						}

						if (savedBone.Rotation != null && mode.HasFlag(Mode.Rotation))
						{
							vm.Rotation = (Quaternion)savedBone.Rotation;
						}

						if (savedBone.Scale != null && mode.HasFlag(Mode.Scale))
						{
							vm.Scale = (Vector)savedBone.Scale;
						}

						bone.ReadTransform();
						bone.WriteTransform(skeleton, false);
					}

					await Task.Delay(1);
				}
			}

			// Restore the head bone rotation if we were only loading an expression
			if (headBone != null && originalHeadRotation != null)
			{
				headBone.ViewModel.Rotation = (Quaternion)originalHeadRotation;
				headBone.ReadTransform();
				headBone.WriteTransform(skeleton, true);
			}

			await Task.Delay(100);

			skeletonMem.Tick();

			PoseService.Instance.CanEdit = true;
		}

		/*[AddINotifyPropertyChangedInterface]
		public class Configuration
		{
			public bool LoadPositions { get; set; } = true;
			public bool LoadRotations { get; set; } = true;
			public bool LoadScales { get; set; } = true;
		}*/

		[Serializable]
		public class Bone
		{
			public Bone()
			{
			}

			public Bone(TransformMemory trans)
			{
				this.Position = trans.Position;
				this.Rotation = trans.Rotation;
				this.Scale = trans.Scale;
			}

			public Vector? Position { get; set; }
			public Quaternion? Rotation { get; set; }
			public Vector? Scale { get; set; }
		}
	}
}
