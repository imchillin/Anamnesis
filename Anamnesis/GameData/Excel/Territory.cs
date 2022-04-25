// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using System.Collections.Generic;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("TerritoryType", 0x5baa595e)]
public class Territory : ExcelRow
{
	private static readonly HashSet<uint> HousingTerritories = new()
	{
		282,
		283,
		284,
		342,
		343,
		344,
		345,
		346,
		347,
		384,
		385,
		386,
		608,
		609,
		610,
		649,
		650,
		651,
		652,
	};

	public string Name { get; private set; } = "Unknown";
	public string Place { get; private set; } = "Unknown";
	public string Region { get; private set; } = "Unknown";
	public string Zone { get; private set; } = "Unknown";

	public bool IsHouse { get; private set; } = false;

	public List<Weather> Weathers { get; private set; } = new List<Weather>();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0) ?? "Unknown";
		this.Region = parser.ReadRowReference<ushort, PlaceName>(3)?.Name ?? "Unknown";
		this.Zone = parser.ReadRowReference<ushort, PlaceName>(4)?.Name ?? "Unknown";
		this.Place = parser.ReadRowReference<ushort, PlaceName>(5)?.Name ?? "Unknown";

		WeatherRate? weatherRate = parser.ReadRowReference<byte, WeatherRate>(12);

		this.Weathers.Clear();
		if (weatherRate != null && weatherRate.UnkStruct0 != null)
		{
			foreach (WeatherRate.UnkStruct0Struct wr in weatherRate.UnkStruct0)
			{
				if (wr.Weather == 0)
					continue;

				this.Weathers.Add(GameDataService.Weathers.Get((uint)wr.Weather));
			}
		}

		this.IsHouse = HousingTerritories.Contains(this.RowId);
	}
}
