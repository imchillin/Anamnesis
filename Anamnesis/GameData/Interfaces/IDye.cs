// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Sheets;
using System.Windows.Media;

public interface IDye : IRow
{
	/// <summary>The dye's byte value identifier.</summary>
	byte Id { get; }

	/// <summary>Gets the icon associated with the stain.</summary>
	ImgRef? Icon { get; }

	/// <summary>Gets the color of the stain as a <see cref="Brush"/>.</summary>
	Brush? Color { get; }

	/// <summary>
	/// Gets or sets a value indicating whether the dye object is favorited.
	/// </summary>
	bool IsFavorite { get; set; }
}
