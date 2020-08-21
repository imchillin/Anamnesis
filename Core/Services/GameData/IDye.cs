// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	public interface IDye : IDataObject
	{
		byte Id { get; }
		string Name { get; }
		string Description { get; }
		IImageSource Icon { get; }
	}
}
