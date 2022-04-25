// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ENpcResident", 0xf74fa88c)]
public class ResidentNpc : ExcelRow, INpcBase
{
	private string? name;
	private EventNpc? eventNpc;

	public string Name => this.name ?? $"{this.TypeName} #{this.RowId}";
	public string Description { get; private set; } = string.Empty;
	public uint ModelCharaRow { get; private set; }

	public ImageReference? Icon => null;
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => this.name != null;
	public string TypeName => "Resident NPC";

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.name = parser.ReadString(0);
		this.Description = parser.ReadString(8) ?? string.Empty;

		this.eventNpc = GameDataService.EventNPCs.Get(this.RowId);
		this.ModelCharaRow = this.eventNpc.ModelCharaRow;
	}

	public INpcAppearance? GetAppearance()
	{
		if (this.eventNpc == null)
			return null;

		return this.eventNpc.GetAppearance();
	}
}
