// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;

public delegate void TerritoryEvent();

[AddINotifyPropertyChangedInterface]
public class TerritoryService : ServiceBase<TerritoryService>
{
	private ushort currentWeatherId;

	public static event TerritoryEvent? TerritoryChanged;

	public uint CurrentTerritoryId { get; private set; }
	public string CurrentTerritoryName { get; private set; } = "Unknown";
	public Territory? CurrentTerritory { get; private set; }

	public ushort CurrentWeatherId
	{
		get
		{
			return this.currentWeatherId;
		}
		set
		{
			if (this.currentWeatherId == value)
				return;

			this.currentWeatherId = value;
			this.CurrentWeather = GameDataService.Weathers?.GetOrDefault(value);
		}
	}

	public Weather? CurrentWeather { get; set; }

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
			try
			{
				await Task.Delay(10);

				if (!MemoryService.IsProcessAlive)
					continue;

				// Update territory
				int newTerritoryId = MemoryService.Read<int>(AddressService.Territory);

				if (newTerritoryId == -1)
				{
					this.currentWeatherId = 0;
					this.CurrentTerritoryId = 0;
					this.CurrentTerritory = null;
					this.CurrentTerritoryName = "Menu";
				}
				else
				{
					if (newTerritoryId != this.CurrentTerritoryId)
					{
						this.CurrentTerritoryId = (uint)newTerritoryId;

						if (GameDataService.Territories == null)
						{
							this.CurrentTerritoryName = $"Unkown ({this.CurrentTerritoryId})";
						}
						else
						{
							this.CurrentTerritory = GameDataService.Territories.Get(this.CurrentTerritoryId);
							this.CurrentTerritoryName = this.CurrentTerritory?.Place + " (" + this.CurrentTerritory?.Region + ")";
						}

						TerritoryChanged?.Invoke();
					}

					// Update weather
					ushort weatherId;
					if (GposeService.Instance.IsGpose)
					{
						weatherId = MemoryService.Read<ushort>(AddressService.GPoseWeather);
					}
					else
					{
						weatherId = MemoryService.Read<byte>(AddressService.Weather);
					}

					if (weatherId != this.CurrentWeatherId)
					{
						this.CurrentWeatherId = weatherId;
					}
				}
			}
			catch (Exception)
			{
				Log.Information("Failed to update territory");
				this.currentWeatherId = 0;
				this.CurrentTerritoryId = 0;
				this.CurrentTerritory = null;
				this.CurrentTerritoryName = "Unknown";

				await Task.Delay(1000);
			}
		}
	}

	private void OnThisPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(TerritoryService.CurrentWeatherId))
		{
			ushort current;
			if (GposeService.Instance.IsGpose)
			{
				current = MemoryService.Read<ushort>(AddressService.GPoseWeather);

				if (current != this.CurrentWeatherId)
				{
					MemoryService.Write<ushort>(AddressService.GPoseWeather, this.CurrentWeatherId, "Gpose weather Changed");
				}
			}
			else
			{
				current = MemoryService.Read<byte>(AddressService.Weather);

				if (current != this.CurrentWeatherId)
				{
					MemoryService.Write<byte>(AddressService.Weather, (byte)this.CurrentWeatherId, "Overworld weather Changed");
				}
			}
		}
		else if (e.PropertyName == nameof(TerritoryService.CurrentWeather))
		{
			if (this.CurrentWeather == null)
				return;

			if (this.CurrentWeatherId == this.CurrentWeather.WeatherId)
				return;

			this.CurrentWeatherId = this.CurrentWeather.WeatherId;
		}
	}
}
