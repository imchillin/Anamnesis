// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;
using XivToolsWpf.Selectors;

[Sheet("BNpcName", 0x77A72DA0)]
public readonly struct BattleNpcName(ExcelPage page, uint offset, uint row)
	: IExcelRow<BattleNpcName>, ISelectable
{
	public uint RowId => row;

	public string Name => page.ReadString(offset, offset).ToString() ?? string.Empty;
	public string? Description => $"N:{this.RowId:D7}";

	static BattleNpcName IExcelRow<BattleNpcName>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
