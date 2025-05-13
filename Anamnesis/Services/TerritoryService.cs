// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Core.Memory;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

public delegate void TerritoryEvent();

[AddINotifyPropertyChangedInterface]
public class TerritoryService : ServiceBase<TerritoryService>
{
	private const int TaskSuccessDelay = 16; // ms
	private const int TaskFailureDelay = 1000; // ms
	private uint currentWeatherId;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies =>
	[
		AddressService.Instance,
		GameService.Instance,
		GameDataService.Instance,
		GposeService.Instance
	];

	public static event TerritoryEvent? TerritoryChanged;

	public uint CurrentTerritoryId { get; private set; }
	public string CurrentTerritoryName { get; private set; } = "Unknown";
	public Territory? CurrentTerritory { get; private set; }

	public uint CurrentWeatherId
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
			this.CurrentWeather = GameDataService.Weathers?.GetRowOrDefault(value);
		}
	}

	public Weather? CurrentWeather { get; set; }

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.Update(this.CancellationToken));
		await base.OnStart();

		this.PropertyChanged += this.OnThisPropertyChanged;
	}

	private async Task Update(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				try
				{
					await Task.Delay(TaskSuccessDelay, cancellationToken);

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
								this.CurrentTerritoryName = $"Unknown ({this.CurrentTerritoryId})";
							}
							else
							{
								this.CurrentTerritory = GameDataService.Territories.GetRow(this.CurrentTerritoryId);
								this.CurrentTerritoryName = this.CurrentTerritory?.Place.Value.Name.ToString() + " (" + this.CurrentTerritory?.Region.Value.Name.ToString() + ")";
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
				catch (TaskCanceledException)
				{
					// Task was canceled, exit the loop.
					break;
				}
				catch (Exception)
				{
					Log.Information("Failed to update territory");
					this.currentWeatherId = 0;
					this.CurrentTerritoryId = 0;
					this.CurrentTerritory = null;
					this.CurrentTerritoryName = "Unknown";

					await Task.Delay(TaskFailureDelay, cancellationToken);
				}
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
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
					MemoryService.Write(AddressService.GPoseWeather, this.CurrentWeatherId, "Gpose weather Changed");
				}
			}
			else
			{
				current = MemoryService.Read<byte>(AddressService.Weather);

				if (current != this.CurrentWeatherId)
				{
					MemoryService.Write(AddressService.Weather, (byte)this.CurrentWeatherId, "Overworld weather Changed");
				}
			}
		}
		else if (e.PropertyName == nameof(TerritoryService.CurrentWeather))
		{
			if (this.CurrentWeather == null)
				return;

			if (this.CurrentWeatherId == this.CurrentWeather.Value.RowId)
				return;

			this.CurrentWeatherId = this.CurrentWeather.Value.RowId;
		}
	}
}
