// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Memory.Types;
	using Anamnesis.Services;

	public class CameraService : ServiceBase<CameraService>
	{
		private bool delimitCamera;

		public CameraViewModel? Camera { get; set; }

		public bool LockCameraPosition
		{
			get;
			set;
		}

		public bool DelimitCamera
		{
			get
			{
				return this.delimitCamera;
			}
			set
			{
				this.delimitCamera = value;

				if (this.Camera == null)
					return;

				this.Camera.MaxZoom = value ? 350 : 20;
				this.Camera.MinZoom = value ? 0 : 1.75f;
				this.Camera.YMin = value ? 1.5f : 1.25f;
				this.Camera.YMax = value ? -1.5f : -1.4f;
			}
		}

		public Vector CameraPosition { get; set; }

		public override async Task Start()
		{
			await base.Start();

			this.Camera = new CameraViewModel(AddressService.Camera, null);

			_ = Task.Run(this.Tick);
		}

		public override Task Shutdown()
		{
			this.DelimitCamera = false;
			return base.Shutdown();
		}

		private async Task Tick()
		{
			while (this.IsAlive)
			{
				await Task.Delay(50);

				if (!GameService.Ready)
					continue;

				if (this.Camera == null)
					continue;

				this.Camera.Pointer = AddressService.Camera;

				if (!GposeService.Instance.IsGpose || GposeService.Instance.IsChangingState)
				{
					this.DelimitCamera = false;
					this.LockCameraPosition = false;
					this.Camera.FreezeAngle = false;
					continue;
				}

				try
				{
					if (this.LockCameraPosition)
					{
						MemoryService.Write(AddressService.GPoseCamera, this.CameraPosition, "Camera Locked");
					}
					else
					{
						this.CameraPosition = MemoryService.Read<Vector>(AddressService.GPoseCamera);
					}
				}
				catch (Exception ex)
				{
					Log.Warning(ex, "Failed to update camera");
				}
			}
		}
	}
}
