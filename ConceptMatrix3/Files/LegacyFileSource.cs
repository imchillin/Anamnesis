// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Files
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using ConceptMatrix;

	using Directories = System.IO.Directory;
	using Files = System.IO.File;
	using Paths = System.IO.Path;

	public class LegacyFileSource : FileSourceBase
	{
		public LegacyFileSource()
			: base("Local Files (CM2)")
		{
			this.startdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CmTool/";
		}

		public override bool CanOpen(FileType filetype)
		{
			if (filetype.Type.Name.StartsWith("Legacy"))
				return true;

			return false;
		}
	}
}
