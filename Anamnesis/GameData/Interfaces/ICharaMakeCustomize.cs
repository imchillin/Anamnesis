// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Windows.Media;

	public interface ICharaMakeCustomize : IRow
	{
		ImageSource? Icon { get; }
		byte FeatureId { get; }
	}
}
