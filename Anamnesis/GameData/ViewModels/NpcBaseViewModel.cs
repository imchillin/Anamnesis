// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class NpcBaseViewModel : ExcelRowViewModel<ENpcBase>, INpcBase
	{
		public NpcBaseViewModel(uint key, ExcelSheet<ENpcBase> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			this.Race = GameDataService.Races!.Get(this.Value.Race.Value!.RowId);
			this.Tribe = GameDataService.Tribes!.Get(this.Value.Tribe.Value!.RowId);
		}

		public IRace Race { get; private set; }
		public ITribe Tribe { get; private set; }

		public override string Name => "Unknown";
		public int FacePaintColor => this.Value.FacePaintColor;
		public int FacePaint => this.Value.FacePaint;

		// Lumina has these values backwards, so flip them here.
		public int ExtraFeature2OrBust => this.Value.ExtraFeature1;
		public int ExtraFeature1 => this.Value.ExtraFeature2OrBust;

		public int ModelType => 0;
		public int Gender => this.Value.Gender;
		public int BodyType => this.Value.BodyType;
		public int Height => this.Value.Height;
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
