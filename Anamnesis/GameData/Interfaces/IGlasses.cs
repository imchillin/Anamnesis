// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;

public interface IGlasses : IRow
{
	ushort GlassesId => (ushort)this.RowId;
	ImageReference? Icon { get; }

	bool IsFavorite { get; set; }
}
