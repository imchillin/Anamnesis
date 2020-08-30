// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class TimeService : IService
	{
		private IMarshaler<int>? timeMem;
		private int time = 0;
		private int moon = 0;
		private bool freeze;

		public int Time
		{
			get
			{
				return this.time;
			}

			set
			{
				this.time = value;
				this.timeMem?.SetValue((this.moon * 86400) + (this.time * 60));
			}
		}

		public int Moon
		{
			get
			{
				return this.moon;
			}

			set
			{
				this.moon = value;
				this.timeMem?.SetValue((this.moon * 86400) + (this.time * 60));
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
					Task.Run(this.CheckTime);
				}
			}
		}

		public Task Initialize()
		{
			this.timeMem = MemoryService.GetMarshaler(Offsets.Main.Time, Offsets.Main.TimeControl);
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.timeMem?.SetValue(0);
			this.timeMem?.Dispose();

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private async Task CheckTime()
		{
			// eorzean day is 70 minutes real-time
			double eorzeaDayInMs = 70 * 60 * 1000;

			// there may be a tiny drift because of rounding errors
			int eorzeaMinuteInMs = (int)Math.Round(eorzeaDayInMs / 24 / 60);

			while (this.Freeze)
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
