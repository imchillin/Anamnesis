// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class TerritoryService : ServiceBase<TerritoryService>
	{
		private const int Offset = 0x13D8;

		public int CurrentTerritoryId { get; set; }
		public string CurrentTerritoryName { get; set; } = "Unknown";
		public ITerritoryType? CurrentTerritory { get; set; }

		public override async Task Start()
		{
			await base.Start();

			_ = Task.Run(this.Update);
		}

		private async Task Update()
		{
			while (this.IsAlive)
			{
				await Task.Delay(10);

				IntPtr territoryAddress = MemoryService.ReadPtr(AddressService.Territory);
				territoryAddress = MemoryService.ReadPtr(territoryAddress);
				territoryAddress += Offset;

				int newTerritoryId = MemoryService.Read<int>(territoryAddress);

				if (newTerritoryId == this.CurrentTerritoryId)
					continue;

				this.CurrentTerritoryId = newTerritoryId;

				if (GameDataService.Territories == null)
				{
					this.CurrentTerritoryName = $"Unkown ({this.CurrentTerritoryId})";
					continue;
				}

				this.CurrentTerritory = GameDataService.Territories.Get(this.CurrentTerritoryId);
				this.CurrentTerritoryName = this.CurrentTerritory.Region + " - " + this.CurrentTerritory.Place;
			}
		}
	}
}
