// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	public interface IDye : IDataObject
	{
		byte Id { get; }
		string Name { get; }
		string Description { get; }
		IImage Icon { get; }
	}
}
