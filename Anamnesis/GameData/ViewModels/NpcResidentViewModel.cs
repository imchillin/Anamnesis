// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using System.Windows.Media;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class NpcResidentViewModel : ExcelRowViewModel<ENpcResident>, INpcBase
	{
		public NpcResidentViewModel(uint key, ExcelSheet<ENpcResident> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
		}

		public ImageSource? Icon => null;
		public override string Name => this.HasName ? this.Singular : $"Resident #{this.RowId}";
		public string Singular => this.Value.Singular;
		public string Plural => this.Value.Plural;
		public string Title => this.Value.Title;
		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => !string.IsNullOrEmpty(this.Singular);
		public override string? Description => this.Title;
		public string TypeKey => "Npc_Resident";

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public uint ModelCharaRow { get; private set; }

		/*public int FacePaintColor => this.Appearance.FacePaintColor;
		public int FacePaint => this.Appearance.FacePaint;
		public int ExtraFeature2OrBust => this.Appearance.ExtraFeature2OrBust;
		public int ExtraFeature1 => this.Appearance.ExtraFeature1;
		public uint ModelCharaRow => this.Appearance.ModelCharaRow;
		public Sheets.Race? Race => this.Appearance.Race;
		public int Gender => this.Appearance.Gender;
		public int BodyType => this.Appearance.BodyType;
		public int Height => this.Appearance.Height;
		public Sheets.Tribe? Tribe => this.Appearance.Tribe;
		public int Face => this.Appearance.Face;
		public int HairStyle => this.Appearance.HairStyle;
		public bool EnableHairHighlight => this.Appearance.EnableHairHighlight;
		public int SkinColor => this.Appearance.SkinColor;
		////public INpcEquip NpcEquip => this.Appearance.NpcEquip;
		public int EyeHeterochromia => this.Appearance.EyeHeterochromia;
		public int HairHighlightColor => this.Appearance.HairHighlightColor;
		public int FacialFeature => this.Appearance.FacialFeature;
		public int FacialFeatureColor => this.Appearance.FacialFeatureColor;
		public int Eyebrows => this.Appearance.Eyebrows;
		public int EyeColor => this.Appearance.EyeColor;
		public int EyeShape => this.Appearance.EyeShape;
		public int Nose => this.Appearance.Nose;
		public int Jaw => this.Appearance.Jaw;
		public int Mouth => this.Appearance.Mouth;
		public int LipColor => this.Appearance.LipColor;
		public int BustOrTone1 => this.Appearance.BustOrTone1;
		public int HairColor => this.Appearance.HairColor;

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
		public IDye? DyeRightRing => null;*/

		public INpcAppearance? GetAppearance()
		{
			throw new System.NotImplementedException();
		}
	}
}
