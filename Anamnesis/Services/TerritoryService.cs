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
				IntPtr territoryAddress = MemoryService.ReadPtr(AddressService.Territory);
				territoryAddress = MemoryService.ReadPtr(territoryAddress);
				territoryAddress += 0x13D8;

				int newTerritoryId = MemoryService.Read<int>(territoryAddress);

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
						this.CurrentTerritoryName = this.CurrentTerritory.Region + " - " + this.CurrentTerritory.Place;
					}
				}

				// Update weather
				IntPtr weatherAddress = MemoryService.ReadPtr(AddressService.Weather);
				weatherAddress += 0x20;
				ushort weatherId = MemoryService.Read<ushort>(weatherAddress);

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
				IntPtr weatherAddress = MemoryService.ReadPtr(AddressService.Weather);
				weatherAddress += 0x20;
				MemoryService.Write<ushort>(weatherAddress, this.CurrentWeatherId);
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
