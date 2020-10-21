// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class NpcResidentViewModel : ExcelRowViewModel<ENpcResident>, INpcResident
	{
		public NpcResidentViewModel(int key, ExcelSheet<ENpcResident> sheet, Lumina lumina)
			: base(key, sheet, lumina)
		{
		}

		public string Name => this.Key.ToString();
		public string Singular => this.Value.Singular;
		public string Plural => this.Value.Plural;
		public string Title => this.Value.Title;

		public INpcBase Appearance
		{
			get
			{
				throw new NotSupportedException();
			}
		}
	}
}
