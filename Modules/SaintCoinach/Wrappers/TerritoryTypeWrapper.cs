// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System.Collections.Generic;
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class TerritoryTypeWrapper : ObjectWrapper<TerritoryType>, ITerritoryType
	{
		private List<IWeather> weathers;

		public TerritoryTypeWrapper(TerritoryType row)
			: base(row)
		{
		}

		public string Name
		{
			get
			{
				return this.Value.Name;
			}
		}

		public string Place
		{
			get
			{
				return this.Value.PlaceName.Name;
			}
		}

		public string Region
		{
			get
			{
				return this.Value.RegionPlaceName.Name;
			}
		}

		public string Zone
		{
			get
			{
				return this.Value.ZonePlaceName.Name;
			}
		}

		public string Background
		{
			get
			{
				return this.Value.Bg;
			}
		}

		public List<IWeather> Weathers
		{
			get
			{
				if (this.weathers == null)
				{
					this.weathers = new List<IWeather>();

					foreach (Weather weather in this.Value.WeatherRate.PossibleWeathers)
					{
						// ?
						if (weather.Key == 0)
							continue;

						this.weathers.Add(new WeatherWrapper(weather));
					}
				}

				return this.weathers;
			}
		}
	}
}
