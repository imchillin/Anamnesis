// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using ConceptMatrix.Services;

	public class LegacyPoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType("cmp", "CM2 Pose File", typeof(LegacyPoseFile));

		public override FileType GetFileType()
		{
			return FileType;
		}

		public PoseFile Upgrade()
		{
			throw new NotImplementedException();
		}
	}
}
