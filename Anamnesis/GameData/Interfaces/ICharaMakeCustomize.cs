// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	public interface ICharaMakeCustomize : IDataObject
	{
		IImageSource Icon { get; }
		byte FeatureId { get; }
	}
}
