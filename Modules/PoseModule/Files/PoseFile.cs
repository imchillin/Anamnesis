// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	public class PoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType(@"pose", "Anamnesis Pose File", typeof(PoseFile), true, "Poses");

		[Flags]
		public enum Groups
		{
			None = 0,

			Hair = 1,
			Face = 2,
			Torso = 4,
			LeftArm = 8,
			RightArm = 16,
			LeftHand = 32,
			RightHand = 64,
			LeftLeg = 128,
			RightLeg = 256,
			Clothes = 512,
			Equipment = 1024,
			Tail = 2048,

			All = Hair | Face | Torso | LeftArm | RightArm | LeftHand | RightHand | LeftLeg | RightLeg | Clothes | Equipment | Tail,
		}

		public override FileType Type => FileType;

		public Configuration Config { get; set; } = new Configuration();
		public Dictionary<string, Transform> Bones { get; set; } = new Dictionary<string, Transform>();

		public void Read(IEnumerable<BoneVisual3d> bones, Configuration config)
		{
			this.Config = config;

			/*foreach (Bone bone in bones)
			{
				if (!bone.IsEnabled)
					continue;

				Groups group = Enum.Parse<Groups>(bone.Definition.Group);
				if (!config.Groups.HasFlag(group))
					continue;

				this.Bones.Add(bone.BoneName, bone.LiveTransform);
			}*/

			throw new NotImplementedException();
		}

		public Task Write(SkeletonViewModel skeleton, Configuration config)
		{
			/*PoseService.SetEnabled(true);
			await Task.Delay(100);

			// don't freeze positions if we aren't writing any
			poseService.FreezePositions = this.Config.IncludePosition;

			foreach (Bone bone in skeleton.Bones)
			{
				if (!bone.IsEnabled)
					continue;

				Groups group = Enum.Parse<Groups>(bone.Definition.Group);
				if (!config.Groups.HasFlag(group))
					continue;

				if (this.Bones.ContainsKey(bone.BoneName))
				{
					try
					{
						this.Bones[bone.BoneName].Rotation.Normalize();

						Transform trans = bone.LiveTransform;
						Transform newTrans = this.Bones[bone.BoneName];

						if (config.IncludeScale && this.Config.IncludeScale && newTrans.Scale != Vector.Zero)
							trans.Scale = newTrans.Scale;

						if (config.IncludePosition && this.Config.IncludePosition && newTrans.Position != Vector.Zero)
							trans.Position = newTrans.Position;

						if (config.IncludeRotation && this.Config.IncludeRotation)
							trans.Rotation = newTrans.Rotation;

						bone.LiveTransform = trans;
						bone.ReadTransform();
					}
					catch (Exception ex)
					{
						throw new Exception("Failed to apply pose transform to bone: " + bone.BoneName, ex);
					}
				}
			}

			await Task.Delay(100);
			poseService.FreezePositions = true;

			skeleton.RefreshBones();*/
			throw new NotImplementedException();
		}

		public class Configuration
		{
			public PoseFile.Groups Groups { get; set; } = Groups.All;
			public bool IncludeRotation { get; set; } = true;
			public bool IncludePosition { get; set; } = true;
			public bool IncludeScale { get; set; } = true;
		}
	}
}
