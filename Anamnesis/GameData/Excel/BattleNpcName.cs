// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel
{
	using Anamnesis.GameData.Sheets;
	using Lumina.Data;
	using Lumina.Excel;

	using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

	[Sheet("BNpcName", 2007444896u)]
	public class BattleNpcName : ExcelRow
	{
		public string Singular { get; private set; } = string.Empty;

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);

			this.Singular = parser.ReadString(0) ?? string.Empty;
		}
	}
}
