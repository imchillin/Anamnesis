// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class NpcBaseWrapper : ObjectWrapper<ENpcBase>, INpcBase
	{
		public NpcBaseWrapper(ENpcBase row)
			: base(row)
		{
		}
	}
}
