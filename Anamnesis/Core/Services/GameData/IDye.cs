// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	public interface IDye : IDataObject
	{
		byte Id { get; }
		string Name { get; }
		string Description { get; }
		IImageSource Icon { get; }
	}
}
