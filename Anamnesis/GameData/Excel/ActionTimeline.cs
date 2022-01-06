// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel
{
	using Anamnesis.GameData.Interfaces;
	using Anamnesis.GameData.Sheets;
	using Lumina.Data;
	using Lumina.Excel;

	using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

	[Sheet("ActionTimeline", 0x3ae4a5a0)]
	public class ActionTimeline : ExcelRow, IAnimation
	{
		public byte Type { get; set; }
		public string? Key { get; set; }
		public byte ActionTimelineIDMode { get; set; }
		public byte IsLoop { get; set; }

		public string? Name => this.Key;
		public uint ActionTimelineRowId => this.RowId;
		public ImageReference? Icon => null;

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);

			this.Type = parser.ReadColumn<byte>(0);
			this.Key = parser.ReadString(6);
			this.ActionTimelineIDMode = parser.ReadColumn<byte>(7);
			this.IsLoop = parser.ReadColumn<byte>(16);
		}
	}
}
