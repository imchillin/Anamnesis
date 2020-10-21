// Concept Matrix 3.
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
		public WeatherViewModel(int key, ExcelSheet<Weather> sheet, Lumina lumina)
			: base(key, sheet, lumina)
		{
		}

		public string Name => this.Value.Name;
		public string Description => this.Value.Description;
		public ushort WeatherId => (ushort)this.Value.RowId;

		public ImageSource? Icon => this.lumina.GetIcon(this.Value.Icon).GetImage();
	}
}
