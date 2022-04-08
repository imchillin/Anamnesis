// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel
{
	using Anamnesis.GameData.Interfaces;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Lumina.Data;
	using Lumina.Excel;

	using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

	[Sheet("Action", 0xfedb4d9a)]
	public class Action : ExcelRow, IAnimation
	{
		public string? Name { get; set; }
		public ImageReference? Icon { get; private set; }
		public uint ActionTimelineRowId { get; private set; }
		public ActionTimeline? ActionTimeline { get; private set; }

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);

			this.Name = parser.ReadString(0);
			this.Icon = parser.ReadImageReference<ushort>(2);

			short animationRow = parser.ReadColumn<short>(7);

			if(animationRow >= 0)
			{
				this.ActionTimeline = GameDataService.ActionTimelines.Get(this.ActionTimelineRowId);
				this.ActionTimelineRowId = (uint)animationRow;
			}
		}
	}
}
