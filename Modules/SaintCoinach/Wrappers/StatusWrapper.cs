// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class StatusWrapper : ObjectWrapper<Status>, IStatus
	{
		public StatusWrapper(Status row)
			: base(row)
		{
		}
	}
}
