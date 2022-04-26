// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System.Windows.Media;
using Anamnesis.GameData.Sheets;

public interface IDye : IRow
{
	byte Id { get; }
	ImageReference? Icon { get; }
	Brush? Color { get; }

	bool IsFavorite { get; set; }
}
