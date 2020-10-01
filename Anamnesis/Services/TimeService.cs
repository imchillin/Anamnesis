// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class TimeService : ServiceBase<TimeService>
	{
		private const double EorzeaTimeConstant = 20.571428571428573;
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

		private bool freeze;

		public DateTime RealTime { get; private set; }
		public int RealTimeOfDay { get; private set; }
		public int RealDayOfMonth { get; private set; }

		public DateTime Time { get; private set; }
		public int TimeOfDay { get; set; }
		public int DayOfMonth { get; set; }

		public bool Freeze
		{
			get
			{
				return this.freeze;
			}
			set
			{
				if (this.freeze == value)
					return;

				this.freeze = value;

				/*if (this.freeze)
				{
					Task.Run(this.FreezeTime);
				}*/
			}
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			_ = Task.Run(this.CheckTime);
		}

		public override async Task Shutdown()
		{
			this.Freeze = false;

			await base.Shutdown();

			if (AddressService.Time != IntPtr.Zero)
			{
				MemoryService.Write(AddressService.Time, 0);
			}
		}

		private async Task CheckTime()
		{
			while (this.IsAlive)
			{
				await Task.Delay(10);

				if (AddressService.Time == IntPtr.Zero)
					continue;

				// time is off by 19 minutes on windows
				double offset = 19 * 60;

				double timeSeconds = (DateTime.Now.ToUniversalTime() - Epoch).TotalSeconds;
				double eorzeaSeconds = (timeSeconds * EorzeaTimeConstant) + offset;
				this.RealTime = Epoch + TimeSpan.FromSeconds(eorzeaSeconds);

				this.RealTimeOfDay = (this.RealTime.Hour * 60) + this.RealTime.Minute;
				this.RealDayOfMonth = this.RealTime.Day;

				////int currentTimeOffset = MemoryService.Read<int>(AddressService.Time);

				if (this.Freeze)
				{
					int minuteOffset = this.TimeOfDay - this.RealTimeOfDay;

					int dayOffset = this.DayOfMonth - this.RealDayOfMonth;
					minuteOffset += dayOffset * 24 * 60;

					if (minuteOffset <= 0)
						minuteOffset += 30 * 24 * 60;

					MemoryService.Write(AddressService.Time, minuteOffset * 60);

					this.Time = this.RealTime.AddMinutes(minuteOffset);
				}
				else
				{
					this.TimeOfDay = this.RealTimeOfDay;
					this.DayOfMonth = this.RealDayOfMonth;
					this.Time = this.RealTime;

					MemoryService.Write(AddressService.Time, 0);
				}
			}
		}

		private async Task FreezeTime()
		{
			// eorzean day is 70 minutes real-time
			double eorzeaDayInMs = 70 * 60 * 1000;

			// there may be a tiny drift because of rounding errors
			int eorzeaMinuteInMs = (int)Math.Round(eorzeaDayInMs / 24 / 60);

			while (this.Freeze && this.IsAlive)
			{
				await Task.Delay(eorzeaMinuteInMs);

				int time = this.TimeOfDay;

				time -= 60;

				if (time <= 0)
					time = 24 * 60;

				this.TimeOfDay = time;
			}
		}
	}
}
