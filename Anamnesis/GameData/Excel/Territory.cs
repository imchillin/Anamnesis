// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

/// <summary>Represents a territory type in the game data.</summary>
[Sheet("TerritoryType", 0x77B34BBB)]
public readonly struct Territory(ExcelPage page, uint offset, uint row)
	: IExcelRow<Territory>
{
	/// <summary>Collection of housing territories.</summary>
	private static readonly HashSet<uint> s_housingTerritories =
	[
		282, 283, 284, 342, 343, 344, 345, 346, 347, 384, 385, 386, 608, 609, 610, 649, 650, 651, 652,
	];

	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the name of the territory.</summary>
	public readonly string Name => page.ReadString(offset, offset).ToString() ?? string.Empty;

	/// <summary>Gets a reference to the current region.</summary>
	public readonly RowRef<PlaceName> Region => new(page.Module, page.ReadUInt16(offset + 28), page.Language);

	/// <summary>Gets a reference to the current zone.</summary>
	public readonly RowRef<PlaceName> Zone => new(page.Module, page.ReadUInt16(offset + 30), page.Language);

	/// <summary>Gets a reference to the current place.</summary>
	public readonly RowRef<PlaceName> Place => new(page.Module, page.ReadUInt16(offset + 32), page.Language);

	/// <summary>
	/// Gets a reference to weather rate applicable to this territory.
	/// </summary>
	public readonly RowRef<WeatherRate> WeatherRate => new(page.Module, page.ReadUInt8(offset + 50), page.Language);

	/// <summary>
	/// Gets a value indicating whether the territory is a housing territory.
	/// </summary>
	public readonly bool IsHouse => s_housingTerritories.Contains(row);

	/// <summary>
	/// Creates a new instance of the <see cref="Territory"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Territory"/> struct.</returns>
	static Territory IExcelRow<Territory>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
