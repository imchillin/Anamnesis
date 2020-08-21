// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Files
{
	using System;
	using ConceptMatrix;

	public class LocalFileSource : FileSourceBase
	{
		public LocalFileSource()
			: base("Local Files")
		{
			this.startdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ConceptMatrix/";
		}

		public override bool CanOpen(FileType filetype)
		{
			if (filetype.Type.Name.StartsWith("Legacy"))
				return false;

			return true;
		}
	}
}
