// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using Anamnesis.Files.Types;

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
