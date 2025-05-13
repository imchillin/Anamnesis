// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public delegate void GposeEvent(bool newState);

[AddINotifyPropertyChangedInterface]
public class GposeService : ServiceBase<GposeService>
{
	private const int TaskDelay = 32; // ms (~30 fps)

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance, GameService.Instance];

	public static event GposeEvent? GposeStateChanged;

	// TODO: Remove this property. The base class already has IsInitialized.
	public bool Initialized { get; private set; } = false;
	public bool IsGpose { get; private set; }

	public static bool GetIsGPose()
	{
		if (AddressService.GposeCheck == IntPtr.Zero)
			return false;

		// Character select screen counts as gpose.
		if (!GameService.Instance.IsSignedIn)
			return true;

		byte check1 = MemoryService.Read<byte>(AddressService.GposeCheck);
		byte check2 = MemoryService.Read<byte>(AddressService.GposeCheck2);

		return check1 == 1 && check2 == 4;
	}

	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.CheckThread(this.CancellationToken));
		await base.OnStart();
	}

	private async Task CheckThread(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				bool newGpose = GetIsGPose();

				if (!this.Initialized)
				{
					this.Initialized = true;
					this.IsGpose = newGpose;
					continue;
				}

				if (newGpose != this.IsGpose)
				{
					this.IsGpose = newGpose;
					GposeStateChanged?.Invoke(newGpose);
				}

				await Task.Delay(TaskDelay, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
			}
		}
	}
}
