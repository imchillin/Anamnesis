// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using Anamnesis.GameData.Excel;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public class ModelListEntry : INpcBase
	{
		public enum Categories
		{
			FashionAccessory,
			Effect,
		}

		public uint ModelCharaRow { get; set; }
		public string Name { get; set; } = string.Empty;
		public ImageReference? Icon { get; set; }
		public Categories Type { get; set; }

		public Mod? Mod => null;
		public bool CanFavorite => false;
		public bool HasName => true;
		public string TypeName => this.Type.ToString();
		public uint RowId { get; set; }
		public string? Description => null;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public INpcAppearance? GetAppearance()
		{
			return new ModelListEntryAppearance(this.ModelCharaRow);
		}

		public class ModelListEntryAppearance : INpcAppearance
		{
			public ModelListEntryAppearance(uint modelCharaRow)
			{
				this.ModelCharaRow = modelCharaRow;
			}

			public uint ModelCharaRow { get; private set; }

			public int FacePaintColor => 0;
			public int FacePaint => 0;
			public int ExtraFeature2OrBust => 0;
			public int ExtraFeature1 => 0;
			public Race? Race => null;
			public int Gender => 0;
			public int BodyType => 0;
			public int Height => 0;
			public Tribe? Tribe => null;
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
		}
	}
}
