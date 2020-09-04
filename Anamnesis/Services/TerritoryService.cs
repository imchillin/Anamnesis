// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class TerritoryService : ServiceBase<TerritoryService>
	{
		private const int Offset = 0x13D8;

		public int TerritoryId { get; set; }
		public string Territory { get; set; } = "Unknown";

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
				this.TerritoryId = MemoryService.Read<int>(territoryAddress);

				this.Territory = $"Unkown ({this.TerritoryId})";
			}
		}
	}
}
