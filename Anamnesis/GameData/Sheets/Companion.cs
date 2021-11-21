// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System.Windows.Media;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina.Data;
	using Lumina.Excel;

	[Sheet("Companion", 2002798787u)]
	public class Companion : ExcelRow, INpcBase
	{
		private string? name;

		public string Name => this.name ?? $"Minion #{this.RowId}";
		public string Description { get; private set; } = string.Empty;
		public uint ModelCharaRow { get; private set; }

		public ImageSource? Icon { get; private set; }
		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => this.name != null;
		public string TypeKey => "Npc_Companion";

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);

			this.name = parser.ReadString(0);
			this.ModelCharaRow = (uint)parser.ReadColumn<ushort>(8);
			////Scale = parser.ReadColumn<byte>(9);
			this.Icon = parser.ReadImageReference<ushort>(26);
		}

		public INpcAppearance? GetAppearance()
		{
			return NpcNoneAppearance.NoneAppearance;
		}
	}
}
