// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using ConceptMatrix;

	public class AllFile : FileBase
	{
		public static readonly FileType FileType = new FileType("cm3aq", "All Appearance File", typeof(AllFile));

		public AppearanceSetFile Appearance { get; set; } = new AppearanceSetFile();
		public EquipmentSetFile Equipment { get; set; } = new EquipmentSetFile();

		public override FileType GetFileType()
		{
			return FileType;
		}
	}
}
