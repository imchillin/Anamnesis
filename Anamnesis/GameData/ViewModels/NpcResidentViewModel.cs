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
		public string Title => !string.IsNullOrEmpty(this.Value.Title) ? this.Value.Title : $"{this.Appearance.ModelCharaRow}";
		public INpcBase Appearance => GameDataService.EventNPCs!.Get(this.Value.RowId);
		public Mod? Mod => null;
		public bool CanFavorite => true;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		int INpcBase.FacePaintColor => this.Appearance.FacePaintColor;
		int INpcBase.FacePaint => this.Appearance.FacePaint;
		int INpcBase.ExtraFeature2OrBust => this.Appearance.ExtraFeature2OrBust;
		int INpcBase.ExtraFeature1 => this.Appearance.ExtraFeature1;
		uint INpcBase.ModelCharaRow => this.Appearance.ModelCharaRow;
		IRace INpcBase.Race => this.Appearance.Race;
		int INpcBase.Gender => this.Appearance.Gender;
		int INpcBase.BodyType => this.Appearance.BodyType;
		int INpcBase.Height => this.Appearance.Height;
		ITribe INpcBase.Tribe => this.Appearance.Tribe;
		int INpcBase.Face => this.Appearance.Face;
		int INpcBase.HairStyle => this.Appearance.HairStyle;
		bool INpcBase.EnableHairHighlight => this.Appearance.EnableHairHighlight;
		int INpcBase.SkinColor => this.Appearance.SkinColor;
		INpcEquip INpcBase.NpcEquip => this.Appearance.NpcEquip;
		int INpcBase.EyeHeterochromia => this.Appearance.EyeHeterochromia;
		int INpcBase.HairHighlightColor => this.Appearance.HairHighlightColor;
		int INpcBase.FacialFeature => this.Appearance.FacialFeature;
		int INpcBase.FacialFeatureColor => this.Appearance.FacialFeatureColor;
		int INpcBase.Eyebrows => this.Appearance.Eyebrows;
		int INpcBase.EyeColor => this.Appearance.EyeColor;
		int INpcBase.EyeShape => this.Appearance.EyeShape;
		int INpcBase.Nose => this.Appearance.Nose;
		int INpcBase.Jaw => this.Appearance.Jaw;
		int INpcBase.Mouth => this.Appearance.Mouth;
		int INpcBase.LipColor => this.Appearance.LipColor;
		int INpcBase.BustOrTone1 => this.Appearance.BustOrTone1;
		int INpcBase.HairColor => this.Appearance.HairColor;
	}
}
