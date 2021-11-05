// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
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

		public override string Name => this.Singular;
		public string Singular => this.Value.Singular;
		public string Plural => this.Value.Plural;
		public string Title => this.Value.Title;
		public INpcBase Appearance => GameDataService.EventNPCs!.Get(this.Value.RowId);
		public Mod? Mod => null;
		public bool CanFavorite => true;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public int FacePaintColor => this.Appearance.FacePaintColor;
		public int FacePaint => this.Appearance.FacePaint;
		public int ExtraFeature2OrBust => this.Appearance.ExtraFeature2OrBust;
		public int ExtraFeature1 => this.Appearance.ExtraFeature1;
		public uint ModelCharaRow => this.Appearance.ModelCharaRow;
		public IRace Race => this.Appearance.Race;
		public int Gender => this.Appearance.Gender;
		public int BodyType => this.Appearance.BodyType;
		public int Height => this.Appearance.Height;
		public ITribe Tribe => this.Appearance.Tribe;
		public int Face => this.Appearance.Face;
		public int HairStyle => this.Appearance.HairStyle;
		public bool EnableHairHighlight => this.Appearance.EnableHairHighlight;
		public int SkinColor => this.Appearance.SkinColor;
		public INpcEquip NpcEquip => this.Appearance.NpcEquip;
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
	}
}
