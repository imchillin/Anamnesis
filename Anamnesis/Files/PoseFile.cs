// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;

	#pragma warning disable SA1402, SA1649
	public class PoseFileInfo : JsonFileInfoBase<PoseFile>
	{
		public override string Extension => "pose";
		public override string Name => "Anamnesis Pose File";
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

		public void WriteToFile(ActorViewModel actor, SkeletonVisual3d skeleton, Configuration config)
		{
			this.Config = config;

			////SkeletonViewModel? skeletonMem = actor?.ModelObject?.Skeleton?.Skeleton;

			if (skeleton == null || skeleton.Bones == null)
				throw new Exception("No skeleton in actor");

			this.Bones = new Dictionary<string, Bone?>();

			foreach ((string name, BoneVisual3d bone) in skeleton.Bones)
			{
				if (config.UseSelection && !skeleton.GetIsBoneSelected(bone))
					continue;

				Transform? trans = bone.ViewModel.Model;

				if (trans == null)
					throw new Exception("Bone is missing transform");

				this.Bones.Add(name, new Bone(trans.Value));
			}
		}

		public async Task Apply(ActorViewModel actor, SkeletonVisual3d skeleton, Configuration config)
		{
			SkeletonViewModel? skeletonMem = actor?.ModelObject?.Skeleton?.Skeleton;

			if (skeletonMem == null || skeleton.Bones == null)
				throw new Exception("No skeleton in actor");

			skeletonMem.MemoryMode = MemoryModes.None;

			PoseService.Instance.SetEnabled(true);
			PoseService.Instance.CanEdit = false;
			await Task.Delay(100);

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

						if (!skeleton.Bones.ContainsKey(name))
						{
							Log.Write($"Bone: \"{name}\" not found", "Pose", Log.Severity.Warning);
							continue;
						}

						BoneVisual3d? bone = skeleton.Bones[name];

						if (config.UseSelection && !skeleton.GetIsBoneSelected(bone))
							continue;

						TransformViewModel vm = bone.ViewModel;

						if (PoseService.Instance.FreezePositions && savedBone.Position != null)
						{
							vm.Position = (Vector)savedBone.Position;
						}

						if (PoseService.Instance.FreezeRotation && savedBone.Rotation != null)
						{
							vm.Rotation = (Quaternion)savedBone.Rotation;
						}

						if (PoseService.Instance.FreezeScale && savedBone.Scale != null)
						{
							vm.Scale = (Vector)savedBone.Scale;
						}

						bone.ReadTransform();
						bone.WriteTransform(skeleton, false);
					}

					await Task.Delay(1);
				}
			}

			await Task.Delay(100);
			skeletonMem.MemoryMode = MemoryModes.ReadWrite;

			await skeletonMem.ReadFromMemoryAsync();
			PoseService.Instance.CanEdit = true;
		}

		public class Configuration
		{
			public bool UseSelection { get; set; } = false;
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
