// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Sheets;
using System.Windows.Media;

public interface IDye : IRow
{
	byte Id { get; }
	ImgRef? Icon { get; }
	Brush? Color { get; }
	bool IsFavorite { get; set; }
}
