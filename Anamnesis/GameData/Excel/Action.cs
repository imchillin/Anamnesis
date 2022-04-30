// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Action", 0xfedb4d9a)]
public class Action : ExcelRow, IAnimation
{
	public string? DisplayName { get; set; }
	public ImageReference? Icon { get; private set; }
	public ActionTimeline? Timeline { get; private set; }
	public IAnimation.AnimationPurpose Purpose => IAnimation.AnimationPurpose.Action;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.DisplayName = parser.ReadString(0);
		this.Icon = parser.ReadImageReference<ushort>(2);

		short animationRow = parser.ReadColumn<short>(7);

		if (animationRow >= 0)
		{
			this.Timeline = GameDataService.ActionTimelines.Get((uint)animationRow);
		}
	}
}
