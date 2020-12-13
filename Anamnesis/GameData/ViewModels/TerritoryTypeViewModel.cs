// © Anamnesis.
// Developed by W and A Walsh.
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

		public override string Name => this.Value?.Name ?? "Unknown";
		public string Place => this.Value?.PlaceName?.Value?.Name ?? "Unknown";
		public string Region => this.Value?.PlaceNameRegion?.Value?.Name ?? "Unknown";
		public string Zone => this.Value?.PlaceNameZone?.Value?.Name ?? "Unknown";

		public List<IWeather> Weathers
		{
			get
			{
				if (this.weathers == null)
				{
					this.weathers = new List<IWeather>();

					if (this.Value != null)
					{
						WeatherRate weatherRate = GameDataService.WeatherRates!.GetRow((uint)this.Value.WeatherRate);

						if (weatherRate != null && weatherRate.UnkStruct0 != null)
						{
							foreach (WeatherRate.UnkStruct0Struct wr in weatherRate.UnkStruct0)
							{
								if (wr.Weather == 0)
									continue;

								this.weathers.Add(GameDataService.Weathers!.Get(wr.Weather));
							}
						}
					}
				}

				return this.weathers;
			}
		}
	}
}
