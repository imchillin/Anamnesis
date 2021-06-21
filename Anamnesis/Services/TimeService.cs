// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class TimeService : ServiceBase<TimeService>
	{
		private NopHookViewModel? timeFreezeHook;

		public TimeSpan Time { get; private set; }
		public string TimeString { get; private set; } = "00:00";
		public long TimeOfDay { get; set; }
		public byte DayOfMonth { get; set; }

		public bool Freeze
		{
			get => this.timeFreezeHook?.Enabled ?? false;
			set => this.timeFreezeHook?.SetEnabled(value);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.timeFreezeHook = new NopHookViewModel(AddressService.TimeStop, 7);

			_ = Task.Run(this.CheckTime);
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
					if (!MemoryService.IsProcessAlive)
						continue;

					if (!GameService.Instance.IsSignedIn)
						continue;

					if (AddressService.Time == IntPtr.Zero)
						continue;

					if (this.Freeze)
					{
						long newTime = (long)((this.TimeOfDay * 60) + (86400 * (this.DayOfMonth - 1)));
						this.Time = TimeSpan.FromSeconds(newTime);
						MemoryService.Write(AddressService.Time, newTime, "Time frozen");
					}
					else
					{
						long timeVal = MemoryService.Read<long>(AddressService.Time) % 2764800;
						this.Time = TimeSpan.FromSeconds(timeVal);

						this.TimeOfDay = (long)this.Time.TotalMinutes - (long)(this.Time.Days * 24 * 60);
						this.DayOfMonth = (byte)this.Time.Days;
					}

					this.TimeString = string.Format("{0:D2}:{1:D2}", this.Time.Hours, this.Time.Minutes);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to update time");
					return;
				}
			}
		}
	}
}
