// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	public delegate void GposeEvent();

	[AddINotifyPropertyChangedInterface]
	public class GposeService : ServiceBase<GposeService>
	{
		private bool initialized = false;

		public static event GposeEvent? GposeStateChanging;
		public static event GposeEvent? GposeStateChanged;

		public bool IsGpose { get; private set; }
		public bool IsOverworld { get; private set; }

		public bool IsChangingState { get; private set; }

		[AlsoNotifyFor(nameof(GposeService.IsChangingState))]
		public bool IsNotChangingState { get => !this.IsChangingState; }

		public static bool GetIsGPose()
		{
			byte check1 = MemoryService.Read<byte>(AddressService.GposeCheck);
			byte check2 = MemoryService.Read<byte>(AddressService.GposeCheck2);
			return check1 == 1 && check2 == 4;
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
				bool newGpose = GetIsGPose();

				if (!this.initialized)
				{
					this.initialized = true;
					this.IsGpose = newGpose;
					this.IsOverworld = !this.IsGpose;
					continue;
				}

				if (newGpose != this.IsGpose)
				{
					this.IsGpose = newGpose;
					this.IsOverworld = !this.IsGpose;

					GposeStateChanging?.Invoke();
					this.IsChangingState = true;

					// retarget as we enter to allow modification of the actor before it loads
					await TargetService.Instance.Retarget();

					await Task.Delay(1000);
					this.IsChangingState = false;

					// retarget again as we have now loaded
					await TargetService.Instance.Retarget();

					GposeStateChanged?.Invoke();
				}

				this.IsChangingState = false;

				await Task.Delay(100);
			}
		}
	}
}
