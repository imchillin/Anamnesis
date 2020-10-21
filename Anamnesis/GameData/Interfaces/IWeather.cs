// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Windows.Media;

	public interface IWeather : IDataObject
	{
		string Name { get; }
		string Description { get; }
		ImageSource? Icon { get; }
		ushort WeatherId { get; }
	}
}
