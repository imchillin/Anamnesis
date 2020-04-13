// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class TerritoryTypeWrapper : ObjectWrapper<TerritoryType>, ITerritoryType
	{
		public TerritoryTypeWrapper(TerritoryType row)
			: base(row)
		{
		}
	}
}
