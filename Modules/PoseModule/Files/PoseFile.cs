// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;

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

				this.Bones.Add(bone.BoneName, bone.Transform);
			}
		}

		public void Write(IEnumerable<Bone> bones, Groups groups)
		{
			foreach (Bone bone in bones)
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
						bone.Transform = this.Bones[bone.BoneName];
					}
					catch (Exception ex)
					{
						throw new Exception("Failed to apply pose transform to bone: " + bone.BoneName, ex);
					}
				}
			}
		}
	}
}
