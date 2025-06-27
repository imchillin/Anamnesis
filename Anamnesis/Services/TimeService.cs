// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class TimeService : ServiceBase<TimeService>
{
	private FrameworkMemory? frameworkMemory;
	private NopHook? timeAsmHook;

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
		this.timeAsmHook = new NopHook(AddressService.TimeAsm, 0x07);

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
		// Chache the previous time string to avoid unnecessary updates
		string previousTimeString = this.TimeString;

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

				this.frameworkMemory.Synchronize();

				if (this.Freeze)
				{
					long newTime = (this.TimeOfDay * 60) + (86400 * (this.DayOfMonth - 1));

					this.frameworkMemory.EorzeaTime = newTime;

					if (this.frameworkMemory.IsTimeOverridden)
						this.frameworkMemory.OverrideEorzeaTime = newTime;
				}

				long currentTime = this.frameworkMemory.IsTimeOverridden ? this.frameworkMemory.OverrideEorzeaTime : this.frameworkMemory.EorzeaTime;
				long timeVal = currentTime % 2764800;
				long secondInDay = timeVal % 86400;
				this.TimeOfDay = secondInDay / 60;
				this.DayOfMonth = (byte)((timeVal / 86400) + 1);

				var displayTime = TimeSpan.FromMinutes(this.TimeOfDay);
				string newTimeString = string.Create(5, displayTime, (span, value) =>
				{
					span[0] = (char)('0' + (value.Hours / 10));
					span[1] = (char)('0' + (value.Hours % 10));
					span[2] = ':';
					span[3] = (char)('0' + (value.Minutes / 10));
					span[4] = (char)('0' + (value.Minutes % 10));
				});

				if (newTimeString != previousTimeString)
				{
					this.TimeString = newTimeString;
					previousTimeString = newTimeString;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to update time");
				return;
			}
		}
	}
}
