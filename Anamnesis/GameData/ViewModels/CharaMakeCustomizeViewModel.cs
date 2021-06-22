// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Windows.Media;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class CharaMakeCustomizeViewModel : ExcelRowViewModel<CharaMakeCustomize>, ICharaMakeCustomize
	{
		public CharaMakeCustomizeViewModel(uint key, ExcelSheet<CharaMakeCustomize> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
		}

		public override string Name => "Feature " + this.Key;
		public ImageSource? Icon => this.lumina.GetImage(this.Value.Icon);
		public byte FeatureId => this.Value.FeatureID;
	}
}
