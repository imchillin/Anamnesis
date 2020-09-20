// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class GposeService : ServiceBase<GposeService>
	{
		public bool IsGpose { get; private set; }
		public bool IsOverworld { get; private set; }

		public override Task Start()
		{
			Task.Run(this.CheckThread);
			return base.Start();
		}

		private async Task CheckThread()
		{
			while (this.IsAlive)
			{
				byte check1 = MemoryService.Read<byte>(AddressService.GposeCheck);
				byte check2 = MemoryService.Read<byte>(AddressService.GposeCheck2);
				this.IsGpose = check1 == 1 && check2 == 4;
				this.IsOverworld = !this.IsGpose;

				await Task.Delay(100);
			}
		}
	}
}
