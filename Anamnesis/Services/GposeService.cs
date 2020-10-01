// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Views;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class GposeService : ServiceBase<GposeService>
	{
		private bool initialized = false;

		public bool IsGpose { get; private set; }
		public bool IsOverworld { get; private set; }

		public bool IsChangingState { get; private set; }

		[AlsoNotifyFor(nameof(GposeService.IsChangingState))]
		public bool IsNotChangingState { get => !this.IsChangingState; }

		public static Task<bool> LeaveGpose(string message)
		{
			return GposePrompt.WaitForChange(false, message);
		}

		public static Task<bool> EnterGpose(string message)
		{
			return GposePrompt.WaitForChange(true, message);
		}

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
				bool newGpose = check1 == 1 && check2 == 4;

				if (newGpose != this.IsGpose || !this.initialized)
				{
					this.initialized = true;
					this.IsGpose = newGpose;
					this.IsOverworld = !this.IsGpose;

					this.IsChangingState = true;

					// GPose takes a while before actors become availalbe. I dont know why, or how to detect it
					// so we just wait a magic duration.
					await Task.Delay(1000);

					TargetService.Instance.Retarget();
				}

				this.IsChangingState = false;

				await Task.Delay(100);
			}
		}
	}
}
