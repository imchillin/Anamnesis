// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Character;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class BNpcBaseViewModel : ExcelRowViewModel<BNpcBase>, INpcBase
	{
		public BNpcBaseViewModel(uint key, ExcelSheet<BNpcBase> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			uint raceId = 1;
			uint tribeId = 1;

			if (this.Value.BNpcCustomize.Value != null)
			{
				raceId = this.Value.BNpcCustomize.Value.Race.Row;
				tribeId = this.Value.BNpcCustomize.Value.Tribe.Row;
			}

			if (raceId <= 0)
				raceId = 1;

			if (tribeId <= 0)
				tribeId = 1;

			this.Race = GameDataService.Races!.Get(raceId);
			this.Tribe = GameDataService.Tribes!.Get(tribeId);
		}

		public IRace Race { get; private set; }
		public ITribe Tribe { get; private set; }

		public int FacePaintColor => this.Value.BNpcCustomize.Value?.FacePaintColor ?? 0;
		public int FacePaint => this.Value.BNpcCustomize.Value?.FacePaint ?? 0;
		public int ExtraFeature2OrBust => this.Value.BNpcCustomize.Value?.ExtraFeature2OrBust ?? 0;
		public int ExtraFeature1 => this.Value.BNpcCustomize.Value?.ExtraFeature1 ?? 0;
		public int Gender => this.Value.BNpcCustomize.Value?.Gender ?? 0;
		public int BodyType => this.Value.BNpcCustomize.Value?.BodyType ?? 0;
		public int Height => this.Value.BNpcCustomize.Value?.Height ?? 0;
		public int Face => this.Value.BNpcCustomize.Value?.Face ?? 0;
		public int HairStyle => this.Value.BNpcCustomize.Value?.HairStyle ?? 0;
		public bool EnableHairHighlight => (this.Value.BNpcCustomize.Value?.HairHighlight ?? 0) > 1;
		public int SkinColor => this.Value.BNpcCustomize.Value?.SkinColor ?? 0;
		public int EyeHeterochromia => this.Value.BNpcCustomize.Value?.EyeHeterochromia ?? 0;
		public int HairHighlightColor => this.Value.BNpcCustomize.Value?.HairHighlightColor ?? 0;
		public int FacialFeature => this.Value.BNpcCustomize.Value?.FacialFeature ?? 0;
		public int FacialFeatureColor => this.Value.BNpcCustomize.Value?.FacialFeatureColor ?? 0;
		public int Eyebrows => this.Value.BNpcCustomize.Value?.Eyebrows ?? 0;
		public int EyeColor => this.Value.BNpcCustomize.Value?.EyeColor ?? 0;
		public int EyeShape => this.Value.BNpcCustomize.Value?.EyeShape ?? 0;
		public int Nose => this.Value.BNpcCustomize.Value?.Nose ?? 0;
		public int Jaw => this.Value.BNpcCustomize.Value?.Jaw ?? 0;
		public int Mouth => this.Value.BNpcCustomize.Value?.Mouth ?? 0;
		public int LipColor => this.Value.BNpcCustomize.Value?.LipColor ?? 0;
		public int BustOrTone1 => this.Value.BNpcCustomize.Value?.BustOrTone1 ?? 0;
		public int HairColor => this.Value.BNpcCustomize.Value?.HairColor ?? 0;

		public override string Name => $"Battle NPC #{this.Key}";
		public uint ModelCharaRow => this.Value.ModelChara.Row;
		public INpcEquip NpcEquip => new NpcEquipViewModel(this.Value.NpcEquip.Value!);

		public Mod? Mod => null;
		public bool CanFavorite => true;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}
	}
}
