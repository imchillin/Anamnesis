// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Windows.Media;

	public interface ICharaMakeCustomize : IDataObject
	{
		ImageSource? Icon { get; }
		byte FeatureId { get; }
	}
}
