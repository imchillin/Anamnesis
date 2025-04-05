// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina.Excel;

/// <summary>Represents a character customization option in the game data.</summary>
[Sheet("CharaMakeCustomize", 0xC30E9B73)]
public readonly struct CharaMakeCustomize(ExcelPage page, uint offset, uint row)
	: IExcelRow<CharaMakeCustomize>
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>Gets the name of the customization option.</summary>
	public string Name => this.HintItem.Value.RowId != 0 ? this.HintItem.Value.Name.ToString() : $"Feature #{this.RowId}";

	/// <summary>Gets the hint item associated with the customization option.</summary>
	public readonly RowRef<Item> HintItem => new(page.Module, page.ReadUInt32(offset + 8), page.Language);

	/// <summary>Gets the icon of the customization option.</summary>
	public ImgRef? Icon => new(page.ReadUInt32(offset));

	/// <summary>Gets the icon of the hint item if available.</summary>
	public ImgRef? ItemIcon => this.HintItem.Value.RowId != 0 ? this.HintItem.Value.Icon : null;

	/// <summary>Gets the feature ID of the customization option.</summary>
	public readonly byte FeatureId => page.ReadUInt8(offset + 14);

	/// <summary>Gets the face type of the customization option.</summary>
	public readonly byte FaceType => page.ReadUInt8(offset + 15);

	/// <summary>Gets a value indicating whether the customization option is purchasable.</summary>
	public readonly bool IsPurchasable => page.ReadPackedBool(offset + 16, 0);

	/// <summary>
	/// Creates a new instance of the <see cref="CharaMakeCustomize"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="CharaMakeCustomize"/> struct.</returns>
	static CharaMakeCustomize IExcelRow<CharaMakeCustomize>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
