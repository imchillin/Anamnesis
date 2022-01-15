// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class WorldService : ServiceBase<WorldService>
	{
		private NopHookViewModel? freezeWorldPosition;
		private NopHookViewModel? freezeWorldRotation;
		private NopHookViewModel? freezeGposeTargetPosition1;
		private NopHookViewModel? freezeGposeTargetPosition2;

		public bool WorldPositionNotFrozen => !this.FreezeWorldPosition;

		public bool FreezeWorldPosition
		{
			get
			{
				return this.freezeWorldPosition?.Enabled ?? false;
			}
			set
			{
				this.freezeWorldPosition?.SetEnabled(value);
				this.freezeWorldRotation?.SetEnabled(value);
				this.freezeGposeTargetPosition1?.SetEnabled(value);
				this.freezeGposeTargetPosition2?.SetEnabled(value);
				this.RaisePropertyChanged(nameof(WorldService.FreezeWorldPosition));
				this.RaisePropertyChanged(nameof(WorldService.WorldPositionNotFrozen));
			}
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.freezeWorldPosition = new NopHookViewModel(AddressService.WorldPositionFreeze, 16);
			this.freezeWorldRotation = new NopHookViewModel(AddressService.WorldRotationFreeze, 4);

			// We need to keep the MOV in the middle here otherwise we invalidate the ptr, but we patch the rest:
			//     MOVSS dword ptr[RCX + 0xa0],XMM1
			//     MOV RBX,RCX
			//     MOVSS dword ptr[RCX + 0xa4],XMM2
			//     MOVSS dword ptr[RCX + 0xa8],XMM3
			this.freezeGposeTargetPosition1 = new NopHookViewModel(AddressService.GPoseCameraTargetPositionFreeze, 8);
			this.freezeGposeTargetPosition2 = new NopHookViewModel(AddressService.GPoseCameraTargetPositionFreeze + 8 + 3, 16);

			GposeService.GposeStateChanging += this.OnGposeStateChanging;
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();

			GposeService.GposeStateChanging -= this.OnGposeStateChanging;

			this.FreezeWorldPosition = false;
		}

		private void OnGposeStateChanging()
		{
			if (GposeService.Instance.IsOverworld)
				this.FreezeWorldPosition = false;
		}
	}
}
