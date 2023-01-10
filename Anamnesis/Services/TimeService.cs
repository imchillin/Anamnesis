// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class TimeService : ServiceBase<TimeService>
{
	private FrameworkMemory? frameworkMemory;
	private NopHookViewModel? timeAsmHook;
	public string TimeString { get; private set; } = "00:00";
	public long TimeOfDay { get; set; }
	public byte DayOfMonth { get; set; }

	public bool Freeze
	{
		get => this.timeAsmHook?.Enabled ?? false;
		set => this.timeAsmHook?.SetEnabled(value);
	}

	public override Task Start()
	{
		this.timeAsmHook = new NopHookViewModel(AddressService.TimeAsm, 0x07);

		this.frameworkMemory = new FrameworkMemory();
		this.frameworkMemory.SetAddress(AddressService.Framework);

		_ = Task.Run(this.CheckTime);

		return base.Start();
	}

	public override async Task Shutdown()
	{
		this.Freeze = false;

		await base.Shutdown();
	}

	private async Task CheckTime()
	{
		while (this.IsAlive)
		{
			await Task.Delay(10);

			try
			{
				if (!MemoryService.IsProcessAlive || !GameService.Instance.IsSignedIn || this.frameworkMemory == null)
				{
					if (this.Freeze)
						this.Freeze = false;

					continue;
				}

				this.frameworkMemory.Tick();

				if (this.Freeze)
				{
					long newTime = (long)((this.TimeOfDay * 60) + (86400 * (this.DayOfMonth - 1)));

					this.frameworkMemory.EorzeaTime = newTime;

					if(this.frameworkMemory.IsTimeOverridden)
						this.frameworkMemory.OverrideEorzeaTime = newTime;
				}

				long currentTime = this.frameworkMemory.IsTimeOverridden ? this.frameworkMemory.OverrideEorzeaTime : this.frameworkMemory.EorzeaTime;
				long timeVal = currentTime % 2764800;
				long secondInDay = timeVal % 86400;
				this.TimeOfDay = (long)(secondInDay / 60f);
				this.DayOfMonth = (byte)(Math.Floor(timeVal / 86400f) + 1);

				var displayTime = TimeSpan.FromMinutes(this.TimeOfDay);
				this.TimeString = string.Format("{0:D2}:{1:D2}", displayTime.Hours, displayTime.Minutes);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to update time");
				return;
			}
		}
	}
}
