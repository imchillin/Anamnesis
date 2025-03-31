// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Excel;

[Sheet("Action", 0xF87A2103)]
public readonly struct Action(ExcelPage page, uint offset, uint row)
	: IExcelRow<Action>, IAnimation
{
	public uint RowId => row;

	public readonly string Name => page.ReadString(offset, offset).ToString();
	public ImgRef? Icon => new(page.ReadUInt16(offset + 8));
	public ActionTimeline? Timeline => this.ActionTimelineHit.RowId >= 0 ? GameDataService.ActionTimelines.GetRow(this.ActionTimelineHit.RowId) : null;
	public readonly RowRef<ActionTimeline> ActionTimelineHit => new(page.Module, page.ReadUInt16(offset + 12), page.Language);
	public IAnimation.AnimationPurpose Purpose => IAnimation.AnimationPurpose.Action;

	static Action IExcelRow<Action>.Create(ExcelPage page, uint offset, uint row) =>
	   new(page, offset, row);
}
