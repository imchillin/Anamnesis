// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis;

	public class PoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType(@"cm3p", "CM3 Pose File", typeof(PoseFile));

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

		public bool IncludePositions { get; set; } = true;
		public bool IncludeScale { get; set; } = true;
		public Dictionary<string, Transform> Bones { get; set; } = new Dictionary<string, Transform>();

		public override FileType GetFileType()
		{
			return FileType;
		}

		public void Read(IEnumerable<Bone> bones, Groups groups)
		{
			foreach (Bone bone in bones)
			{
				if (!bone.IsEnabled)
					continue;

				Groups group = Enum.Parse<Groups>(bone.Definition.Group);
				if (!groups.HasFlag(group))
					continue;

				this.Bones.Add(bone.BoneName, bone.LiveTransform);
			}
		}

		public async Task Write(SkeletonViewModel skeleton, Groups groups)
		{
			PoseService poseService = Services.Get<PoseService>();

			poseService.IsEnabled = true;

			// don't freeze positions if we aren't writing any
			poseService.FreezePositions = this.IncludePositions;

			foreach (Bone bone in skeleton.Bones)
			{
				if (!bone.IsEnabled)
					continue;

				Groups group = Enum.Parse<Groups>(bone.Definition.Group);
				if (!groups.HasFlag(group))
					continue;

				if (this.Bones.ContainsKey(bone.BoneName))
				{
					try
					{
						this.Bones[bone.BoneName].Rotation.Normalize();

						Transform trans = bone.LiveTransform;
						Transform newTrans = this.Bones[bone.BoneName];

						if (this.IncludeScale && newTrans.Scale != Vector.Zero)
							trans.Scale = newTrans.Scale;

						if (this.IncludePositions && newTrans.Position != Vector.Zero)
							trans.Position = newTrans.Position;

						////if (newTrans.Rotation != Quaternion.Identity)
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
		}
	}
}
