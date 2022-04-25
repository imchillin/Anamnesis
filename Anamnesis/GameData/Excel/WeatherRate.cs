// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("WeatherRate", 0x474abce2)]
public class WeatherRate : ExcelRow
{
	public UnkStruct0Struct[]? UnkStruct0 { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.UnkStruct0 = new UnkStruct0Struct[8];
		for (int i = 0; i < 8; i++)
		{
			this.UnkStruct0[i] = default(UnkStruct0Struct);
			this.UnkStruct0[i].Weather = parser.ReadColumn<int>(i * 2);
			this.UnkStruct0[i].Rate = parser.ReadColumn<byte>((i * 2) + 1);
		}
	}

	public struct UnkStruct0Struct
	{
		public int Weather;
		public byte Rate;
	}
}
