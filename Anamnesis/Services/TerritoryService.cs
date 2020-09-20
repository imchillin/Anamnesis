// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class TerritoryService : ServiceBase<TerritoryService>
	{
		public int CurrentTerritoryId { get; set; }
		public string CurrentTerritoryName { get; set; } = "Unknown";
		public ITerritoryType? CurrentTerritory { get; set; }

		public ushort CurrentWeatherId { get; set; }
		public IWeather? CurrentWeather { get; set; }

		public override async Task Start()
		{
			await base.Start();

			_ = Task.Run(this.Update);

			this.PropertyChanged += this.OnThisPropertyChanged;
		}

		private async Task Update()
		{
			while (this.IsAlive)
			{
				await Task.Delay(10);

				// Update territory
				int newTerritoryId = MemoryService.Read<int>(AddressService.Territory);

				if (newTerritoryId != this.CurrentTerritoryId)
				{
					this.CurrentTerritoryId = newTerritoryId;

					if (GameDataService.Territories == null)
					{
						this.CurrentTerritoryName = $"Unkown ({this.CurrentTerritoryId})";
					}
					else
					{
						this.CurrentTerritory = GameDataService.Territories.Get(this.CurrentTerritoryId);
						this.CurrentTerritoryName = this.CurrentTerritory?.Region + " - " + this.CurrentTerritory?.Place;
					}
				}

				// Update weather
				ushort weatherId = MemoryService.Read<ushort>(AddressService.Weather);

				if (weatherId != this.CurrentWeatherId)
				{
					this.CurrentWeatherId = weatherId;
					this.CurrentWeather = null;

					if (this.CurrentTerritory != null)
					{
						foreach (IWeather weather in this.CurrentTerritory.Weathers)
						{
							if (weather.WeatherId == weatherId)
							{
								this.CurrentWeather = weather;
							}
						}
					}
				}
			}
		}

		private void OnThisPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TerritoryService.CurrentWeatherId))
			{
				ushort current = MemoryService.Read<ushort>(AddressService.Weather);

				if (current != this.CurrentWeatherId)
				{
					MemoryService.Write<ushort>(AddressService.Weather, this.CurrentWeatherId);
				}
			}
			else if (e.PropertyName == nameof(TerritoryService.CurrentWeather))
			{
				if (this.CurrentWeather == null)
					return;

				this.CurrentWeatherId = this.CurrentWeather.WeatherId;
			}
		}
	}
}
