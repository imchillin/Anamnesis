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
		private bool freeze;
		private int dayOfMonth = 0;
		private int timeOfDay = 0;

		public int Time { get; set; } = 0;

		public int TimeOfDay
		{
			get
			{
				return this.timeOfDay;
			}

			set
			{
				this.timeOfDay = value;
				this.Time = (this.dayOfMonth * 86400) + (this.timeOfDay * 60);
			}
		}

		public int DayOfMonth
		{
			get
			{
				return this.dayOfMonth;
			}

			set
			{
				this.dayOfMonth = value;
				this.Time = (this.dayOfMonth * 86400) + (this.TimeOfDay * 60);
			}
		}

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

				if (this.freeze)
				{
					Task.Run(this.FreezeTime);
				}
			}
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			_ = Task.Run(this.CheckTime);
		}

		private async Task CheckTime()
		{
			while (this.IsAlive)
			{
				await Task.Delay(10);

				IntPtr timeAddress = MemoryService.ReadPtr(AddressService.Time);
				timeAddress = MemoryService.ReadPtr(timeAddress + 0x10);
				timeAddress = MemoryService.ReadPtr(timeAddress + 0x8);
				timeAddress = MemoryService.ReadPtr(timeAddress + 0x28);
				timeAddress += 0x80;

				if (timeAddress == IntPtr.Zero)
					continue;

				MemoryService.Write(timeAddress, this.Time);
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

				int time = this.Time;

				time -= 1;

				if (time <= 0)
					time = 24 * 60;

				this.Time = time;
			}
		}
	}
}
