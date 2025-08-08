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

/// <summary>
/// A service that monitors and controls the current territory and weather in the game.
/// </summary>
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

	/// <summary>
	/// The delegate object for the <see cref="TerritoryService.TerritoryChanged"/> event.
	/// </summary>
	public delegate void TerritoryEvent();

	/// <summary>
	/// Event that is triggered when the territory changes (when the player enters a new zone).
	/// </summary>
	public static event TerritoryEvent? TerritoryChanged;

	/// <summary>Gets the current territory's identifier.</summary>
	public uint CurrentTerritoryId { get; private set; }

	/// <summary>Gets the current territory in the game.</summary>
	public Territory? CurrentTerritory { get; private set; }

	/// <summary>Gets the current territory's name.</summary>
	public string CurrentTerritoryName { get; private set; } = "Unknown";

	/// <summary>
	/// Gets or sets the current weather in the game by its identifier.
	/// </summary>
	/// <remarks>
	/// Changes to this property affect the value of <see cref="CurrentWeather"/> as well.
	/// </remarks>
	public uint CurrentWeatherId
	{
		get => this.currentWeatherId;
		set
		{
			if (this.currentWeatherId == value)
				return;

			this.currentWeatherId = value;
			this.CurrentWeather = GameDataService.Weathers?.GetRowOrDefault(value);
		}
	}

	/// <summary>
	/// Gets or sets the current weather in the game.
	/// </summary>
	/// <remarks>
	/// Changes to this property affect the value of <see cref="CurrentWeatherId"/> as well.
	/// </remarks>
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
							weatherId = MemoryService.Read<byte>(AddressService.NextWeatherId);
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
				current = MemoryService.Read<byte>(AddressService.NextWeatherId);

				if (current != this.CurrentWeatherId)
				{
					MemoryService.Write(AddressService.NextWeatherId, (byte)this.CurrentWeatherId, "Overworld weather Changed");
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
