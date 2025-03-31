// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Excel;

[Sheet("ActionTimeline", 0xD803699F)]
public readonly struct ActionTimeline(ExcelPage page, uint offset, uint row)
	: IExcelRow<ActionTimeline>, IAnimation
{
	public uint RowId => row;

	public readonly string Key => page.ReadString(offset, offset).ToString();
	public readonly byte Type => page.ReadUInt8(offset + 9);
	public readonly AnimationMemory.AnimationSlots Slot => (AnimationMemory.AnimationSlots)page.ReadUInt8(offset + 12);
	public readonly bool IsLoop => page.ReadPackedBool(offset + 20, 5);

	public string? Name => this.Key;
	public ActionTimeline? Timeline => this;
	public ImgRef? Icon => null;

	public IAnimation.AnimationPurpose Purpose => IAnimation.AnimationPurpose.Raw;

	static ActionTimeline IExcelRow<ActionTimeline>.Create(ExcelPage page, uint offset, uint row) =>
	   new(page, offset, row);
}
