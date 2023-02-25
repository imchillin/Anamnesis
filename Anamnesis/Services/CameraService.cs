// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using Anamnesis.Services;

public class CameraService : ServiceBase<CameraService>
{
	private NopHookViewModel? freezeCameraAngleX;
	private NopHookViewModel? freezeCameraAngleY;

	private bool delimitCamera;

	public CameraMemory Camera { get; set; } = new CameraMemory();
	public GPoseCameraMemory GPoseCamera { get; set; } = new GPoseCameraMemory();

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

	public bool FreezeAngle
	{
		get
		{
			return this.Camera.FreezeAngle;
		}
		set
		{
			this.Camera.FreezeAngle = value;

			this.freezeCameraAngleX?.SetEnabled(value);
			this.freezeCameraAngleY?.SetEnabled(value);
		}
	}

	public override async Task Start()
	{
		await base.Start();

		this.freezeCameraAngleX = new NopHookViewModel(AddressService.CameraAngleXFreeze, 8);
		this.freezeCameraAngleY = new NopHookViewModel(AddressService.CameraAngleYFreeze, 6);

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
			await Task.Delay(33);

			if (!GameService.Ready)
				continue;

			if (this.Camera == null)
				continue;

			try
			{
				if (!MemoryService.IsProcessAlive)
				{
					await Task.Delay(1000);
					continue;
				}

				if (!GposeService.Instance.IsGpose)
				{
					this.DelimitCamera = false;
					this.FreezeAngle = false;
				}
				else
				{
					this.GPoseCamera.SetAddress(AddressService.GPoseCamera);
					this.GPoseCamera.Tick();
				}

				this.Camera.SetAddress(AddressService.Camera);
				this.Camera.Tick();
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to update camera");
				await Task.Delay(1000);
			}
		}
	}
}
