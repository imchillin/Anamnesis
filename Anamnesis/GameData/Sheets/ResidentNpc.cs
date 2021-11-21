// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System.Windows.Media;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina.Data;
	using Lumina.Excel;

	[Sheet("ENpcResident", 4149192844u)]
	public class ResidentNpc : ExcelRow, INpcBase
	{
		private string? name;

		public string Name => this.name ?? $"Resident NPC #{this.RowId}";
		public string Description { get; private set; } = string.Empty;
		public uint ModelCharaRow { get; private set; }

		public ImageSource? Icon => null;
		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => this.name != null;
		public string TypeKey => "Npc_Resident";

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

			this.ModelCharaRow = 0; // hmm...
		}

		public INpcAppearance? GetAppearance()
		{
			// Resident npc's actually just duplicate event npcs...
			EventNpc eventNpc = GameDataService.EventNPCs.Get(this.RowId);
			return eventNpc.GetAppearance();
		}
	}
}
