// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Windows.Media;
	using Anamnesis.Character;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;

	public class CompanionViewModel : ExcelRowViewModel<Lumina.Excel.GeneratedSheets.Companion>, INpcBase
	{
		public CompanionViewModel(uint key, Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Companion> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			this.Race = GameDataService.Races.Get(1);
			this.Tribe = GameDataService.Tribes.Get(1);
		}

		public Race Race { get; private set; }
		public Tribe Tribe { get; private set; }

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

		public ImageSource? Icon => this.lumina.GetImage(this.Value.Icon);
		public override string Name => this.HasName ? this.Value.Singular : $"Minion #{this.RowId}";
		public override string? Description => this.Value?.MinionRace.Value?.Name?.ToString();
		public uint ModelCharaRow => this.Value.Model.Row;
		////public INpcEquip NpcEquip => ItemUtility.DummyNoneNpcEquip;
		public string TypeKey => "Npc_Companion";

		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => !string.IsNullOrEmpty(this.Value.Singular);

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public IItem? MainHand => null;
		public IDye? DyeMainHand => null;
		public IItem? OffHand => null;
		public IDye? DyeOffHand => null;
		public IItem? Head => null;
		public IDye? DyeHead => null;
		public IItem? Body => null;
		public IDye? DyeBody => null;
		public IItem? Legs => null;
		public IDye? DyeLegs => null;
		public IItem? Feet => null;
		public IDye? DyeFeet => null;
		public IItem? Hands => null;
		public IDye? DyeHands => null;
		public IItem? Wrists => null;
		public IDye? DyeWrists => null;
		public IItem? Neck => null;
		public IDye? DyeNeck => null;
		public IItem? Ears => null;
		public IDye? DyeEars => null;
		public IItem? LeftRing => null;
		public IDye? DyeLeftRing => null;
		public IItem? RightRing => null;
		public IDye? DyeRightRing => null;

		public INpcAppearance? GetAppearance()
		{
			throw new System.NotImplementedException();
		}
	}
}
