// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	public interface IWeather : IDataObject
	{
		string Name { get; }
		string Description { get; }
		IImage Icon { get; }
	}
}
