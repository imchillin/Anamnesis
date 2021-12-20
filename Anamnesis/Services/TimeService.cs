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
		private TimeMemory? timeMemory;

		public TimeSpan Time { get; private set; }
		public string TimeString { get; private set; } = "00:00";
		public long TimeOfDay { get; set; }
		public byte DayOfMonth { get; set; }

		public bool Freeze
		{
			get => this.timeMemory?.Freeze ?? false;
			set => this.timeMemory?.SetFrozen(value);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.timeMemory = new TimeMemory();

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

					if (AddressService.TimeReal == IntPtr.Zero)
						continue;

					if (this.Freeze)
					{
						long newTime = (long)((this.TimeOfDay * 60) + (86400 * (this.DayOfMonth - 1)));
						this.Time = TimeSpan.FromSeconds(newTime);
						this.timeMemory?.SetTime(newTime);
					}
					else
					{
						long timeVal = this.timeMemory!.CurrentTime % 2764800;
						this.Time = TimeSpan.FromSeconds(timeVal);
						this.TimeOfDay = (long)(this.Time.TotalMinutes - (this.Time.Days * 24 * 60));
						this.DayOfMonth = (byte)this.Time.Days;
					}

					int hours = this.Time.Hours;
					int minutes = this.Time.Minutes;

					if (hours < 0)
						hours += 24;

					if (minutes < 0)
						minutes += 60;

					this.TimeString = string.Format("{0:D2}:{1:D2}", hours, minutes);
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
