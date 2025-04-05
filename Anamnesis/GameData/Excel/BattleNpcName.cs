// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;
using XivToolsWpf.Selectors;

/// <summary>
/// Represents the name component of a battle non-playable entity in the game data.
/// </summary>
[Sheet("BNpcName", 0x77A72DA0)]
public readonly struct BattleNpcName(ExcelPage page, uint offset, uint row)
	: IExcelRow<BattleNpcName>, ISelectable
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the singular name of the battle non-playable entity.</summary>
	public readonly string Name => page.ReadString(offset, offset).ToString() ?? string.Empty;

	/// <summary>Gets the description of the battle non-player name entity.</summary>
	public readonly string? Description => $"N:{this.RowId:D7}";

	/// <summary>
	/// Creates a new instance of the <see cref="BattleNpcName"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="BattleNpcName"/> struct.</returns>
	static BattleNpcName IExcelRow<BattleNpcName>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
