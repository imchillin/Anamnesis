// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	public interface ICharaMakeCustomize : IDataObject
	{
		IImage Icon { get; }
		byte FeatureId { get; }
	}
}
