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
						this.timeMemory?.SetTime(newTime);
					}
					else
					{
						long timeVal = this.timeMemory!.CurrentTime % 2764800;
						long secondInDay = timeVal % 86400;
						this.TimeOfDay = (long)(secondInDay / 60f);
						this.DayOfMonth = (byte)(Math.Floor(timeVal / 86400f) + 1);
					}

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
}
