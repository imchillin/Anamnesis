// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel
{
	using Anamnesis.GameData.Interfaces;
	using Anamnesis.GameData.Sheets;
	using Lumina.Data;
	using Lumina.Excel;

	using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

	[Sheet("Emote", 0xf3afded2)]
	public class Emote : ExcelRow, IAnimation
	{
		public string? Name { get; private set; }
		public uint ActionTimelineRowId { get; private set; }
		public ActionTimeline? ActionTimeline { get; private set; }
		public ImageReference? Icon { get; private set; }

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);

			this.Name = parser.ReadString(0);

			ActionTimeline? a = parser.ReadRowReference<ushort, ActionTimeline>(1);
			ActionTimeline? b = parser.ReadRowReference<ushort, ActionTimeline>(2);
			ActionTimeline? c = parser.ReadRowReference<ushort, ActionTimeline>(3);
			ActionTimeline? d = parser.ReadRowReference<ushort, ActionTimeline>(4);
			ActionTimeline? e = parser.ReadRowReference<ushort, ActionTimeline>(5);
			ActionTimeline? f = parser.ReadRowReference<ushort, ActionTimeline>(6);
			ActionTimeline? g = parser.ReadRowReference<ushort, ActionTimeline>(7);

			this.ActionTimeline = a;
			this.ActionTimelineRowId = this.ActionTimeline?.RowId ?? 0;

			this.Icon = parser.ReadImageReference<ushort>(20);
		}
	}
}
