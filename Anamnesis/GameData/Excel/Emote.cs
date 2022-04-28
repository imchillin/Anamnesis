// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Emote", 0xf3afded2)]
public class Emote : ExcelRow
{
	public string? DisplayName { get; private set; }
	public ActionTimeline? LoopTimeline { get; private set; }
	public ActionTimeline? IntroTimeline { get; private set; }
	public ActionTimeline? GroundTimeline { get; private set; }
	public ActionTimeline? ChairTimeline { get; private set; }
	public ActionTimeline? UpperBodyTimeline { get; private set; }

	public ImageReference? Icon { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.DisplayName = parser.ReadString(0);

		this.LoopTimeline = parser.ReadRowReference<ushort, ActionTimeline>(1);
		this.IntroTimeline = parser.ReadRowReference<ushort, ActionTimeline>(2);
		this.GroundTimeline = parser.ReadRowReference<ushort, ActionTimeline>(3);
		this.ChairTimeline = parser.ReadRowReference<ushort, ActionTimeline>(4);
		this.UpperBodyTimeline = parser.ReadRowReference<ushort, ActionTimeline>(5);
		ActionTimeline? f = parser.ReadRowReference<ushort, ActionTimeline>(6);
		ActionTimeline? g = parser.ReadRowReference<ushort, ActionTimeline>(7);

		this.Icon = parser.ReadImageReference<ushort>(20);
	}
}
