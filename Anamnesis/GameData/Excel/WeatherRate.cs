// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;

/// <summary>Represents the weather rate data in the game.</summary>
[Sheet("WeatherRate", 0x474ABCE2)]
public readonly unsafe struct WeatherRate(ExcelPage page, uint offset, uint row)
	: IExcelRow<WeatherRate>
{
	/// <summary>Gets the row ID.</summary>
	public uint RowId => row;

	/// <summary>
	/// Gets all weather conditions as as a collection of references.
	/// </summary>
	public readonly Collection<RowRef<Weather>> Weather => new(page, offset, offset, &WeatherCtor, 8);

	/// <summary>Gets the collection of weather rates.</summary>
	public readonly Collection<byte> Rate => new(page, offset, offset, &RateCtor, 8);

	/// <summary>
	/// Creates a new instance of the <see cref="WeatherRate"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="WeatherRate"/> struct.</returns>
	static WeatherRate IExcelRow<WeatherRate>.Create(ExcelPage page, uint offset, uint row) =>
	new(page, offset, row);

	private static RowRef<Weather> WeatherCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page.Module, (uint)page.ReadInt32(offset + (i * 4)), page.Language);
	private static byte RateCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadUInt8(offset + 32 + i);
}
