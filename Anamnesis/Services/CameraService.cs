// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A service that keeps track of the camera's memory state, responsible for
/// updating the camera every time a user enters or exits GPose to ensure camera object validity.
/// </summary>
public class CameraService : ServiceBase<CameraService>
{
	private const int TaskSuccessDelay = 32; // ms
	private const int TaskFailureDelay = 1000; // ms

	private bool delimitCamera;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [GameService.Instance, GposeService.Instance];

	/// <summary>Gets the overworld camera's memory.</summary>
	public CameraMemory Camera { get; private set; } = new CameraMemory();

	/// <summary>Gets the gpose camera's memory.</summary>
	public GPoseCameraMemory GPoseCamera { get; private set; } = new GPoseCameraMemory();

	/// <summary>
	/// Gets or sets a value indicating whether the camera
	/// should be limited to the in-game camera limitations.
	/// </summary>
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

	/// <inheritdoc/>
	public override async Task Shutdown()
	{
		this.DelimitCamera = false;
		await base.Shutdown();
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.Tick(this.CancellationToken));
		await base.OnStart();
	}

	private async Task Tick(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(TaskSuccessDelay, cancellationToken);

				if (!GameService.Ready)
					continue;

				if (this.Camera == null)
					continue;

				try
				{
					if (!MemoryService.IsProcessAlive)
					{
						await Task.Delay(TaskFailureDelay, cancellationToken);
						continue;
					}

					if (!GposeService.Instance.IsGpose)
					{
						this.DelimitCamera = false;
						this.Camera.FreezeAngle = false;
					}
					else
					{
						// SetAddress will synchronize if addresses are different
						// so we don't need to call Synchronize() again.
						if (this.GPoseCamera.Address == AddressService.GPoseCamera)
							this.GPoseCamera.Synchronize();
						else
							this.GPoseCamera.SetAddress(AddressService.GPoseCamera);
					}

					// Same as above
					if (this.Camera.Address == AddressService.Camera)
						this.Camera.Synchronize();
					else
						this.Camera.SetAddress(AddressService.Camera);
				}
				catch (TaskCanceledException)
				{
					// Task was canceled, exit the loop
					break;
				}
				catch (Exception ex)
				{
					Log.Warning(ex, "Failed to update camera");
					await Task.Delay(TaskFailureDelay, cancellationToken);
				}
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop
				break;
			}
		}
	}
}
