// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Windows.Media;

	public interface IDye : IRow
	{
		byte Id { get; }
		string Name { get; }
		string? Description { get; }
		ImageSource? Icon { get; }
		Brush? Color { get; }
	}
}
