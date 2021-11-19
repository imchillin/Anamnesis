// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Windows.Media;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class ENpcBaseViewModel : ExcelRowViewModel<ENpcBase>, INpcBase
	{
		private readonly string? name;

		public ENpcBaseViewModel(uint key, ExcelSheet<ENpcBase> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			uint raceId = this.Value.Race.Row;
			uint tribeId = this.Value.Tribe.Row;

			if (raceId <= 0)
				raceId = 1;

			if (tribeId <= 0)
				tribeId = 1;

			this.Race = GameDataService.Races.Get(raceId);
			this.Tribe = GameDataService.Tribes.Get(tribeId);

			this.name = GameDataService.GetNpcName(this);
		}

		public Sheets.Race Race { get; private set; }
		public Sheets.Tribe Tribe { get; private set; }

		public ImageSource? Icon => null;
		public override string Name => this.name ?? $"Event NPC #{this.RowId}";
		public override string? Description => null;
		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => this.name != null;
		public string TypeKey => "Npc_Event";

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public int FacePaintColor => this.Value.FacePaintColor;
		public int FacePaint => this.Value.FacePaint;

		// Lumina has these values backwards, so flip them here.
		public int ExtraFeature2OrBust => this.Value.ExtraFeature1;
		public int ExtraFeature1 => this.Value.ExtraFeature2OrBust;

		public uint ModelCharaRow => this.Value.ModelChara.Row;
		public int Gender => this.Value.Gender;
		public int BodyType => this.Value.BodyType;
		public int Height => this.Value.Height;
		public int Face => this.Value.Face;
		public int HairStyle => this.Value.HairStyle;
		public bool EnableHairHighlight => this.Value.HairHighlight > 1;
		public int SkinColor => this.Value.SkinColor;
		public int EyeHeterochromia => this.Value.EyeHeterochromia;
		public int HairHighlightColor => this.Value.HairHighlightColor;
		public int FacialFeature => this.Value.FacialFeature;
		public int FacialFeatureColor => this.Value.FacialFeatureColor;
		public int Eyebrows => this.Value.Eyebrows;
		public int EyeColor => this.Value.EyeColor;
		public int EyeShape => this.Value.EyeShape;
		public int Nose => this.Value.Nose;
		public int Jaw => this.Value.Jaw;
		public int Mouth => this.Value.Mouth;
		public int LipColor => this.Value.LipColor;
		public int BustOrTone1 => this.Value.BustOrTone1;
		public int HairColor => this.Value.HairColor;
		public INpcEquip NpcEquip => new ENpcEquipViewModel(this.Value);
	}
}
