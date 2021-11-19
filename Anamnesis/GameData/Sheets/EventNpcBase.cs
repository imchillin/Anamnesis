// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System.Windows.Media;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina.Data;
	using Lumina.Excel;

	[Sheet("ENpcBase", 2457028568u)]
	public class EventNpcBase : ExcelRow, INpcBase
	{
		private string? name;

		public string Name => this.name ?? $"Event NPC #{this.RowId}";
		public string Description { get; private set; } = string.Empty;
		public uint ModelCharaRow { get; private set; }

		public ImageSource? Icon => null;
		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => this.name != null;
		public string TypeKey => "Npc_Event";

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
			ExcelSheet<EventNpc> sheet = GameDataService.GetSheet<EventNpc>();
			return sheet.Get(this.RowId);
		}
	}
}
