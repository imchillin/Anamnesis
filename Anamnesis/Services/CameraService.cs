// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Memory.Types;

	using MediaQuaternion = System.Windows.Media.Media3D.Quaternion;

	public class CameraService : ServiceBase<CameraService>
	{
		private bool unlimitCamera;

		public CameraViewModel? Camera { get; set; }

		public bool LockCameraAngle
		{
			get;
			set;
		}

		public bool LockCameraPosition
		{
			get;
			set;
		}

		public bool UnlimitCamera
		{
			get
			{
				return this.unlimitCamera;
			}
			set
			{
				this.unlimitCamera = value;

				if (this.Camera == null)
					return;

				this.Camera.MaxZoom = value ? 256 : 20;
				this.Camera.MinZoom = value ? 0 : 1.75f;
				this.Camera.YMin = value ? 1.5f : 1.25f;
				this.Camera.YMax = value ? -1.5f : -1.4f;
			}
		}

		public override async Task Start()
		{
			await base.Start();

			IntPtr ptr = MemoryService.ReadPtr(AddressService.Camera);
			this.Camera = new CameraViewModel(ptr);
		}
	}
}
