// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using Anamnesis;
	using Anamnesis.Services;

	public class LocalFileSource : FileSourceBase
	{
		public LocalFileSource()
			: base("Local Files")
		{
			this.startdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Anamnesis/";
		}

		public override bool CanOpen(FileType filetype)
		{
			if (filetype.Type.Name.StartsWith("Legacy"))
				return false;

			return true;
		}
	}
}
