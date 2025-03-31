// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina.Excel;

[Sheet("Weather", 0x02CF2541)]
public readonly struct Weather(ExcelPage page, uint offset, uint row)
	: IExcelRow<Weather>
{
	/// <summary>Gets the row ID.</summary>
	public uint RowId => row;

	/// <summary>Gets the name of the weather.</summary>
	public readonly string Name => page.ReadString(offset, offset).ToString() ?? string.Empty;

	/// <summary>Gets the description of the weather.</summary>
	public readonly string Description => page.ReadString(offset + 4, offset).ToString() ?? string.Empty;

	/// <summary>Gets the icon reference for the weather.</summary>
	public ImgRef Icon => new(page.ReadInt32(offset + 24));

	/// <summary>
	/// Creates a new instance of the <see cref="Weather"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Weather"/> struct.</returns>
	static Weather IExcelRow<Weather>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
