// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Companion", 0x776048c3)]
public class Companion : ExcelRow, INpcBase
{
	private string? name;
	private CompanionAppearance? appearance;

	public string Name => this.name ?? $"{this.TypeName} #{this.RowId}";
	public string Description { get; private set; } = string.Empty;
	public uint ModelCharaRow { get; private set; }

	public ImageReference? Icon { get; private set; }
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => this.name != null;
	public string TypeName => "Minion";

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
		if (this.appearance == null)
			this.appearance = new CompanionAppearance(this);

		return this.appearance;
	}

	public class CompanionAppearance : INpcAppearance
	{
		public CompanionAppearance(Companion companion)
		{
			this.ModelCharaRow = companion.ModelCharaRow;
		}

		public uint ModelCharaRow { get; private set; }

		public int FacePaintColor => 0;
		public int FacePaint => 0;
		public int ExtraFeature2OrBust => 0;
		public int ExtraFeature1 => 0;
		public Race? Race => GameDataService.Races.Get(1);
		public int Gender => 0;
		public int BodyType => 0;
		public int Height => 0;
		public Tribe? Tribe => GameDataService.Tribes.Get(1);
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

		public IItem MainHand => ItemUtility.NoneItem;
		public IDye DyeMainHand => DyeUtility.NoneDye;
		public IItem OffHand => ItemUtility.NoneItem;
		public IDye DyeOffHand => DyeUtility.NoneDye;
		public IItem Head => ItemUtility.NoneItem;
		public IDye DyeHead => DyeUtility.NoneDye;
		public IItem Body => ItemUtility.NoneItem;
		public IDye DyeBody => DyeUtility.NoneDye;
		public IItem Legs => ItemUtility.NoneItem;
		public IDye DyeLegs => DyeUtility.NoneDye;
		public IItem Feet => ItemUtility.NoneItem;
		public IDye DyeFeet => DyeUtility.NoneDye;
		public IItem Hands => ItemUtility.NoneItem;
		public IDye DyeHands => DyeUtility.NoneDye;
		public IItem Wrists => ItemUtility.NoneItem;
		public IDye DyeWrists => DyeUtility.NoneDye;
		public IItem Neck => ItemUtility.NoneItem;
		public IDye DyeNeck => DyeUtility.NoneDye;
		public IItem Ears => ItemUtility.NoneItem;
		public IDye DyeEars => DyeUtility.NoneDye;
		public IItem LeftRing => ItemUtility.NoneItem;
		public IDye DyeLeftRing => DyeUtility.NoneDye;
		public IItem RightRing => ItemUtility.NoneItem;
		public IDye DyeRightRing => DyeUtility.NoneDye;
	}
}
