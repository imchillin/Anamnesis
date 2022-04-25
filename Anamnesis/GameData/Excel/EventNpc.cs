// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ENpcBase", 0x927347d8)]
public class EventNpc : ExcelRow, INpcBase
{
	private string? name;

	public string Name => this.name ?? $"{this.TypeName} #{this.RowId}";
	public string Description { get; private set; } = string.Empty;
	public uint ModelCharaRow { get; private set; }

	public ImageReference? Icon => null;
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => this.name != null;
	public string TypeName => "Event NPC";

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.name = GameDataService.GetNpcName(this);

		////Scale = parser.ReadColumn<float>(34);
		this.ModelCharaRow = (uint)parser.ReadColumn<ushort>(35);
	}

	public INpcAppearance? GetAppearance()
	{
		Sheets.ExcelSheet<EventNpcAppearance> sheet = GameDataService.GetSheet<EventNpcAppearance>();
		return sheet.Get(this.RowId);
	}
}
