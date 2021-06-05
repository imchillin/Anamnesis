// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using System.Windows.Media;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;
	using Lumina.Extensions;

	public class WeatherViewModel : ExcelRowViewModel<Weather>, IWeather
	{
		public WeatherViewModel(uint key, ExcelSheet<Weather> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
		}

		public override string Name => this.Value.Name;
		public override string Description => this.Value.Description;
		public ushort WeatherId => (ushort)this.Value.RowId;

		public ImageSource? Icon => this.lumina.GetImage(this.Value.Icon);
	}
}
