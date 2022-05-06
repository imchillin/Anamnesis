// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina.Data;
using Lumina.Excel;
using XivToolsWpf.Selectors;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("BNpcName", 0x77a72da0)]
public class BattleNpcName : ExcelRow, ISelectable
{
	public string Name { get; private set; } = string.Empty;
	public string? Description => $"N:{this.RowId.ToString("D7")}";

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0) ?? string.Empty;
	}
}
