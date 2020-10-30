// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Collections.Generic;
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class TerritoryTypeViewModel : ExcelRowViewModel<TerritoryType>, ITerritoryType
	{
		private List<IWeather>? weathers;

		public TerritoryTypeViewModel(int key, ExcelSheet<TerritoryType> sheet, Lumina lumina)
			: base(key, sheet, lumina)
		{
		}

		public override string Name => this.Value.Name;
		public string Place => this.Value.PlaceName.Value.Name;
		public string Region => this.Value.PlaceNameRegion.Value.Name;
		public string Zone => this.Value.PlaceNameZone.Value.Name;

		public List<IWeather> Weathers
		{
			get
			{
				if (this.weathers == null)
				{
					this.weathers = new List<IWeather>();

					WeatherRate weatherRate = GameDataService.WeatherRates!.GetRow((uint)this.Value.WeatherRate);

					foreach (WeatherRate.UnkStruct0Struct wr in weatherRate.UnkStruct0)
					{
						if (wr.Weather == 0)
							continue;

						this.weathers.Add(GameDataService.Weathers!.Get(wr.Weather));
					}
				}

				return this.weathers;
			}
		}
	}
}
