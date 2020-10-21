// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class NpcBaseViewModel : ExcelRowViewModel<ENpcBase>, INpcBase
	{
		public NpcBaseViewModel(int key, ExcelSheet<ENpcBase> sheet, Lumina lumina)
			: base(key, sheet, lumina)
		{
		}

		public int FacePaintColor => this.Value.FacePaintColor;
		public int FacePaint => this.Value.FacePaint;
		public int ExtraFeature2OrBust => this.Value.ExtraFeature2OrBust;
		public int ExtraFeature1 => this.Value.ExtraFeature1;
		public int ModelType => this.Value.BodyType - 1;
		public IRace Race => GameDataService.Races!.Get((int)this.Value.Race.Value.RowId);
		public int Gender => this.Value.Gender;
		public int BodyType => this.Value.BodyType;
		public int Height => this.Value.Height;
		public ITribe Tribe => GameDataService.Tribes!.Get((int)this.Value.Tribe.Value.RowId);
		public int Face => this.Value.Face;
		public int HairStyle => this.Value.HairStyle;
		public int HairHighlight => this.Value.HairHighlight;
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
		public INpcEquip NpcEquip => new NpcEquipViewModel(this.Value);
	}
}
