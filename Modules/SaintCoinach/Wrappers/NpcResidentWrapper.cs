// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class NpcResidentWrapper : ObjectWrapper<ENpcResident>, INpcResident
	{
		public NpcResidentWrapper(ENpcResident row)
			: base(row)
		{
		}
	}
}
