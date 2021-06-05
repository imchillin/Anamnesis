// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class NpcResidentViewModel : ExcelRowViewModel<ENpcResident>, INpcResident
	{
		public NpcResidentViewModel(uint key, ExcelSheet<ENpcResident> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
		}

		public override string Name => this.Singular;
		public string Singular => this.Value.Singular;
		public string Plural => this.Value.Plural;
		public string Title => !string.IsNullOrEmpty(this.Value.Title) ? this.Value.Title : this.Key.ToString();
		public INpcBase? Appearance => GameDataService.BaseNPCs!.Get(this.Value.RowId);
		public Mod? Mod => null;
	}
}
