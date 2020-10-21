// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Windows.Media;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class CharaMakeCustomizeViewModel : ExcelRowViewModel<CharaMakeCustomize>, ICharaMakeCustomize
	{
		public CharaMakeCustomizeViewModel(int key, ExcelSheet<CharaMakeCustomize> sheet, Lumina lumina)
			: base(key, sheet, lumina)
		{
		}

		public ImageSource? Icon => this.lumina.GetImage(this.Value.Icon);
		public byte FeatureId => this.Value.FeatureID;
	}
}
