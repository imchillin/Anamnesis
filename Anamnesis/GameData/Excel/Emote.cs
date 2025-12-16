// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina.Excel;

/// <summary>Represents player emotes in the game data.</summary>
[Sheet("Emote", 0xAFF5D14E)]
public readonly unsafe struct Emote(ExcelPage page, uint offset, uint row)
	: IExcelRow<Emote>
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the singular name of the emote.</summary>
	public readonly string Name => page.ReadString(offset, offset).ToString();

	/// <summary>
	/// Gets the looping action timeline of the emote (if available).
	/// </summary>
	public ActionTimeline? LoopTimeline => this.ActionTimeline[0].Value;

	/// <summary>
	/// Gets the intro action timeline of the emote (if available).
	/// </summary>
	public ActionTimeline? IntroTimeline => this.ActionTimeline[1].Value;

	/// <summary>
	/// Gets the ground action timeline of the emote (if available).
	/// </summary>
	public ActionTimeline? GroundTimeline => this.ActionTimeline[2].Value;

	/// <summary>
	/// Gets the chair action timeline of the emote (if available).
	/// </summary>
	public ActionTimeline? ChairTimeline => this.ActionTimeline[3].Value;

	/// <summary>
	/// Gets the upper body action timeline of the emote (if available).
	/// </summary>
	public ActionTimeline? UpperBodyTimeline => this.ActionTimeline[4].Value;

	/// <summary>
	/// Gets the image reference of the emote icon.
	/// </summary>
	public ImgRef? Icon => new(page.ReadUInt32(offset + 4));

	/// <summary>
	/// Gets the action timelines collection of the emote.
	/// </summary>
	public readonly Collection<RowRef<ActionTimeline>> ActionTimeline => new(page, offset, offset, &ActionTimelineCtor, 7);

	/// <summary>
	/// Creates a new instance of the <see cref="Emote"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Emote"/> struct.</returns>
	static Emote IExcelRow<Emote>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	private static RowRef<ActionTimeline> ActionTimelineCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page.Module, (uint)page.ReadUInt16(offset + 16 + (i * 2)), page.Language);
}
