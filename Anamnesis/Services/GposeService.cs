// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using PropertyChanged;

public delegate void GposeEvent(bool newState);

[AddINotifyPropertyChangedInterface]
public class GposeService : ServiceBase<GposeService>
{
	private bool initialized = false;

	public static event GposeEvent? GposeStateChanged;

	public bool IsGpose { get; private set; }

	[DependsOn(nameof(IsGpose))]
	public bool IsOverworld => !this.IsGpose;

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
				continue;
			}

			if (newGpose != this.IsGpose)
			{
				this.IsGpose = newGpose;
				GposeStateChanged?.Invoke(newGpose);
			}

			// ~30 fps
			await Task.Delay(32);
		}
	}
}
