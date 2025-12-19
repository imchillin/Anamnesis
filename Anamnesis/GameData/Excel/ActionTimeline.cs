// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Excel;

/// <summary>Represents an action timeline object in the game data.</summary>
[Sheet("ActionTimeline", 0xD803699F)]
public readonly struct ActionTimeline(ExcelPage page, uint offset, uint row)
	: IExcelRow<ActionTimeline>, IAnimation
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>
	/// An unsigned 16-bit integer representing the row ID of the action timeline.
	/// </summary>
	/// <remarks>
	/// This is necessary as we directly use this value to set
	/// the game memory variable, which is of type ushort.
	/// </remarks>
	public readonly ushort AnimationId => (ushort)row;

	/// <summary>Gets the in-game path of the action timeline.</summary>
	public readonly string Key => page.ReadString(offset, offset).ToString();

	/// <summary>Gets the type of the action timeline.</summary>
	/// <remarks>As reported in EXDSchema#84, the offset in the schema, which is used by Lumina.Excel, is incorrect.</remarks>
	public readonly byte Type => page.ReadUInt8(offset + 8);

	/// <summary>Gets the animation slot of the action timeline.</summary>
	/// <remarks>As reported in EXDSchema#84, the offset in the schema, which is used by Lumina.Excel, is incorrect.</remarks>
	public readonly AnimationMemory.AnimationSlots Slot => (AnimationMemory.AnimationSlots)page.ReadUInt8(offset + 11);

	/// <summary>Gets a value indicating whether the action timeline is a looping animation.</summary>
	/// <remarks>As reported in EXDSchema#84, the offset in the schema, which is used by Lumina.Excel, is incorrect.</remarks>
	public readonly bool IsLoop => page.ReadPackedBool(offset + 20, 0);

	/// <inheritdoc/>
	public readonly string? Name => this.Key;

	/// <inheritdoc/>
	public ActionTimeline? Timeline => this;

	/// <inheritdoc/>
	public readonly ImgRef? Icon => null;

	/// <inheritdoc/>
	public IAnimation.AnimationPurpose Purpose => IAnimation.AnimationPurpose.Raw;

	/// <summary>
	/// Creates a new instance of the <see cref="ActionTimeline"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="ActionTimeline"/> struct.</returns>
	static ActionTimeline IExcelRow<ActionTimeline>.Create(ExcelPage page, uint offset, uint row) =>
	   new(page, offset, row);
}
