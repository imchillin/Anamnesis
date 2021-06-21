// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Posing.Views;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;

#pragma warning disable SA1402, SA1649
	public class PoseFileInfo : JsonFileInfoBase<PoseFile>
	{
		public override string Extension => "pose";
		public override string Name => "Anamnesis Pose";
		public override Type? LoadOptionsViewType => typeof(LoadOptions);

		public override IFileSource[] FileSources => new[]
		{
			new LocalFileSource("Local Files", SettingsService.Current.DefaultPoseDirectory),
			new BuiltInFileSource("BuiltInPoses/"),
		};
	}

	public class PoseFile : FileBase
	{
		public Configuration Config { get; set; } = new Configuration();

		public Dictionary<string, Bone?>? Bones { get; set; }

		public static async Task<Configuration> Save(ActorViewModel? actor, SkeletonVisual3d? skeleton, Configuration? config, bool selectionOnly)
		{
			if (config == null)
				config = new Configuration();

			if (actor == null || skeleton == null)
				return config;

			SaveResult result = await FileService.Save<PoseFile>();

			if (string.IsNullOrEmpty(result.Path) || result.Info == null)
				return config;

			PoseFile file = new PoseFile();
			file.WriteToFile(actor, skeleton, config, selectionOnly);

			using FileStream stream = new FileStream(result.Path, FileMode.Create);
			result.Info.SerializeFile(file, stream);
			return config;
		}

		public void WriteToFile(ActorViewModel actor, SkeletonVisual3d skeleton, Configuration config, bool selectionOnly)
		{
			this.Config = config;

			////SkeletonViewModel? skeletonMem = actor?.ModelObject?.Skeleton?.Skeleton;

			if (skeleton == null || skeleton.Bones == null)
				throw new Exception("No skeleton in actor");

			this.Bones = new Dictionary<string, Bone?>();

			foreach (BoneVisual3d bone in skeleton.Bones)
			{
				if (selectionOnly && !skeleton.GetIsBoneSelected(bone))
					continue;

				Transform? trans = bone.ViewModel.Model;

				if (trans == null)
					throw new Exception("Bone is missing transform");

				this.Bones.Add(bone.BoneName, new Bone(trans.Value));
			}
		}

		public async Task Apply(ActorViewModel actor, SkeletonVisual3d skeleton, Configuration config, bool selectionOnly)
		{
			if (actor == null)
				throw new ArgumentNullException(nameof(actor));

			if (actor.ModelObject == null)
				throw new Exception("Actor has no model");

			if (actor.ModelObject.Skeleton == null)
				throw new Exception("Actor model has no skeleton wrapper");

			if (actor.ModelObject.Skeleton.Skeleton == null)
				throw new Exception("Actor skeleton wrapper has no skeleton");

			SkeletonViewModel skeletonMem = actor.ModelObject.Skeleton.Skeleton;

			skeletonMem.MemoryMode = MemoryModes.None;

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

						TransformPtrViewModel vm = bone.ViewModel;

						if (PoseService.Instance.FreezePositions && savedBone.Position != null && config.LoadPositions)
						{
							vm.Position = (Vector)savedBone.Position;
						}

						if (PoseService.Instance.FreezeRotation && savedBone.Rotation != null && config.LoadRotations)
						{
							vm.Rotation = (Quaternion)savedBone.Rotation;
						}

						if (PoseService.Instance.FreezeScale && savedBone.Scale != null && config.LoadScales)
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
			skeletonMem.MemoryMode = MemoryModes.ReadWrite;

			await skeletonMem.ReadFromMemoryAsync();

			PoseService.Instance.CanEdit = true;
		}

		[AddINotifyPropertyChangedInterface]
		public class Configuration
		{
			public bool LoadPositions { get; set; } = true;
			public bool LoadRotations { get; set; } = true;
			public bool LoadScales { get; set; } = true;
		}

		[Serializable]
		public class Bone
		{
			public Bone()
			{
			}

			public Bone(Transform trans)
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
