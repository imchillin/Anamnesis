// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Excel;

/// <summary>Represents an executable actor action in the game data.</summary>
[Sheet("Action", 0xF87A2103)]
public readonly struct Action(ExcelPage page, uint offset, uint row)
	: IExcelRow<Action>, IAnimation
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <inheritdoc/>
	public readonly string Name => page.ReadString(offset, offset).ToString();

	/// <inheritdoc/>
	public readonly ImgRef? Icon => new(page.ReadUInt16(offset + 8));

	/// <inheritdoc/>
	public ActionTimeline? Timeline => this.AnimationEnd.IsValid && this.AnimationEnd.RowId >= 0 ? GameDataService.ActionTimelines.GetRow(this.AnimationEnd.RowId) : null;

	/// <summary>
	/// Gets a reference to the action timeline associated with the action.
	/// </summary>
	public readonly RowRef<ActionTimeline> AnimationEnd => new(page.Module, (uint)page.ReadInt16(offset + 32), page.Language);

	/// <inheritdoc/>
	public IAnimation.AnimationPurpose Purpose => IAnimation.AnimationPurpose.Action;

	/// <summary>
	/// Creates a new instance of the <see cref="Action"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Action"/> struct.</returns>
	static Action IExcelRow<Action>.Create(ExcelPage page, uint offset, uint row) =>
	   new(page, offset, row);
}
