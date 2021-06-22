// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	public class BuiltInFileSource : LocalFileSource
	{
		public BuiltInFileSource(string baseDir)
			: base("Built In files", "Data/" + baseDir)
		{
		}

		public override bool CanWrite => false;
	}
}
