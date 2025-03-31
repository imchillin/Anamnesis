// © Anamnesis.
// Licensed under the MIT license.

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Excel;

/// <summary>Represents a row in the "Glasses" Excel sheet.</summary>
[Sheet("Glasses", 0x2FAAC2C1)]
public readonly struct Glasses(ExcelPage page, uint offset, uint row)
	: IExcelRow<Glasses>
{
	/// <summary>Gets the row ID.</summary>
	public uint RowId => row;

	/// <summary>Gets the name of the glasses.</summary>
	public readonly string Name => this.RowId != 0
		? page.ReadString(offset + 12, offset).ToString() ?? $"Glasses #{this.RowId}"
		: LocalizationService.GetString("Facewear_None_Name");

	/// <summary>Gets the description of the glasses.</summary>
	public readonly string Description => this.RowId != 0
		? page.ReadString(offset + 8, offset).ToString() ?? string.Empty
		: LocalizationService.GetString("Facewear_None_Desc");

	/// <summary>Gets the icon reference for the glasses.</summary>
	public ImgRef Icon => new(page.ReadInt32(offset + 28));

	/// <summary>
	/// Gets or sets a value indicating whether the glasses are marked as favorite.
	/// </summary>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, nameof(FavoritesService.Favorites.Glasses), value);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Glasses"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Glasses"/> struct.</returns>
	static Glasses IExcelRow<Glasses>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
