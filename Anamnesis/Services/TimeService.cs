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

		public DateTimeOffset Time { get; private set; }
		public string TimeString { get; private set; } = "00:00";
		public int TimeOfDay { get; set; }
		public int DayOfMonth { get; set; }

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
						var offset = TimeSpan.FromDays(this.DayOfMonth - 1) + TimeSpan.FromMinutes(this.TimeOfDay);
						this.Time = new DateTimeOffset(this.Time.Year, this.Time.Month, 1, 0, 0, 0, TimeSpan.Zero) + offset;
						this.timeMemory?.SetTime(this.Time.ToUnixTimeSeconds());
					}
					else
					{
						this.Time = DateTimeOffset.FromUnixTimeSeconds(this.timeMemory!.CurrentTime);
						this.TimeOfDay = (int)this.Time.TimeOfDay.TotalMinutes;
						this.DayOfMonth = this.Time.Day;
					}

					this.TimeString = this.Time.ToString("HH:mm");
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
