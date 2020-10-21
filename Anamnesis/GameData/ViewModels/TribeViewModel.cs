// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Memory;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class TribeViewModel : ExcelRowViewModel<Tribe>, ITribe
	{
		public TribeViewModel(int key, ExcelSheet<Tribe> sheet)
			: base(key, sheet)
		{
		}

		public Appearance.Tribes Tribe => (Appearance.Tribes)this.Key;
		public string Feminine => this.Value.Feminine;
		public string Masculine => this.Value.Masculine;
		public string DisplayName => this.DisplayName;

		public bool Equals(ITribe? other)
		{
			if (other is null)
				return false;

			return this.Tribe == other.Tribe;
		}
	}
}
