// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina.Excel;

[Sheet("Emote", 0xF3AFDED2)]
public readonly unsafe struct Emote(ExcelPage page, uint offset, uint row)
	: IExcelRow<Emote>
{
	public uint RowId => row;

	public readonly string Name => page.ReadString(offset, offset).ToString();
	public ActionTimeline? LoopTimeline => this.ActionTimeline[0].Value;
	public ActionTimeline? IntroTimeline => this.ActionTimeline[1].Value;
	public ActionTimeline? GroundTimeline => this.ActionTimeline[2].Value;
	public ActionTimeline? ChairTimeline => this.ActionTimeline[3].Value;
	public ActionTimeline? UpperBodyTimeline => this.ActionTimeline[4].Value;
	public ImgRef? Icon => new(page.ReadUInt16(offset + 28));

	public readonly Collection<RowRef<ActionTimeline>> ActionTimeline => new(page, offset, offset, &ActionTimelineCtor, 7);
	static Emote IExcelRow<Emote>.Create(ExcelPage page, uint offset, uint row) =>
new(page, offset, row);
	private static RowRef<ActionTimeline> ActionTimelineCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page.Module, (uint)page.ReadUInt16(offset + 12 + (i * 2)), page.Language);
}
