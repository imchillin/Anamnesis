// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Glasses")]
public class Glasses : ExcelRow, IGlasses
{
	public ushort GlassesId => (ushort)this.RowId;
	public string Name { get; private set; } = string.Empty;
	public string Description { get; private set; } = string.Empty;
	public ImageReference? Icon { get; private set; }

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		if(this.RowId == 0)
		{
			this.Name = "None";
			this.Description = "Just your beautiful face!";
		}
		else
		{
			this.Name = parser.ReadString(13) ?? $"Glasses #{this.RowId}";
			this.Description = parser.ReadString(12) ?? string.Empty;
			this.Icon = parser.ReadImageReference<int>(2);
		}
	}
}
