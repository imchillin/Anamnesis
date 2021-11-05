// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Character;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class CompanionViewModel : ExcelRowViewModel<Companion>, INpcBase
	{
		public CompanionViewModel(uint key, ExcelSheet<Companion> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			this.Race = GameDataService.Races!.Get(1);
			this.Tribe = GameDataService.Tribes!.Get(1);
		}

		public IRace Race { get; private set; }
		public ITribe Tribe { get; private set; }

		public int FacePaintColor => 0;
		public int FacePaint => 0;
		public int ExtraFeature2OrBust => 0;
		public int ExtraFeature1 => 0;
		public int Gender => 0;
		public int BodyType => 0;
		public int Height => 0;
		public int Face => 0;
		public int HairStyle => 0;
		public bool EnableHairHighlight => false;
		public int SkinColor => 0;
		public int EyeHeterochromia => 0;
		public int HairHighlightColor => 0;
		public int FacialFeature => 0;
		public int FacialFeatureColor => 0;
		public int Eyebrows => 0;
		public int EyeColor => 0;
		public int EyeShape => 0;
		public int Nose => 0;
		public int Jaw => 0;
		public int Mouth => 0;
		public int LipColor => 0;
		public int BustOrTone1 => 0;
		public int HairColor => 0;

		public override string Name => !string.IsNullOrEmpty(this.Value.Singular) ? this.Value.Singular : $"Minion #{this.Key}";
		public uint ModelCharaRow => this.Value.Model.Row;
		public INpcEquip NpcEquip => ItemUtility.DummyNoneNpcEquip;

		public Mod? Mod => null;
		public bool CanFavorite => true;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}
	}
}
