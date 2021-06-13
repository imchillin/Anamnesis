// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Windows.Media;

	public interface IDye : IRow
	{
		byte Id { get; }
		ImageSource? Icon { get; }
		Brush? Color { get; }

		bool IsFavorite { get; set; }
	}
}
