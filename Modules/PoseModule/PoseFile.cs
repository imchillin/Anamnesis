// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using ConceptMatrix.Services;

	public class PoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType("cm3p", "CM3 Pose File", typeof(PoseFile));

		public override FileType GetFileType()
		{
			return FileType;
		}
	}
}
