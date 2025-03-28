// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Data;
using Lumina.Excel;
using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Glasses")]
public class Glasses : ExcelRow
{
	public ushort GlassesId => (ushort)this.RowId;
	public string Name { get; private set; } = string.Empty;
	public string Description { get; private set; } = string.Empty;
	public ImageReference? Icon { get; private set; }

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<Glasses>(this);
		set => FavoritesService.SetFavorite<Glasses>(this, nameof(FavoritesService.Favorites.Glasses), value);
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		if(this.RowId == 0)
		{
			this.Name = LocalizationService.GetString("Facewear_None_Name");
			this.Description = LocalizationService.GetString("Facewear_None_Desc");
		}
		else
		{
			this.Name = parser.ReadString(13) ?? string.Empty;
			this.Description = parser.ReadString(12) ?? string.Empty;
			this.Icon = parser.ReadImageReference<int>(2);
		}
	}
}
