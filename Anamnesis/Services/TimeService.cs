// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A service that monitors and controls the in-game time and date.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class TimeService : ServiceBase<TimeService>
{
	private const int TASK_SUCCESS_DELAY = 16; // ms
	private const int TASK_FAILURE_DELAY = 1000; // ms

	private FrameworkMemory? frameworkMemory;
	private NopHook? timeAsmHook;

	/// <summary>Gets the time of day as formatted string.</summary>
	public string TimeString { get; private set; } = "00:00";

	/// <summary>Gets or sets the time of day in Eorzea time.</summary>
	public long TimeOfDay { get; set; }

	/// <summary>Gets or sets the day of the month in Eorzea time.</summary>
	public byte DayOfMonth { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the in-game time should be frozen.
	/// </summary>
	public bool Freeze
	{
		get => this.timeAsmHook?.Enabled ?? false;
		set => this.timeAsmHook?.SetEnabled(value);
	}

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance];

	/// <inheritdoc/>
	public override async Task Shutdown()
	{
		this.Freeze = false;
		await base.Shutdown();
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.timeAsmHook = new NopHook(AddressService.TimeAsm, 0x07);
		this.frameworkMemory = new FrameworkMemory();
		this.frameworkMemory.SetAddress(AddressService.Framework);

		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.CheckTime(this.CancellationToken));
		await base.OnStart();
	}

	private async Task CheckTime(CancellationToken cancellationToken)
	{
		// Chache the previous time string to avoid unnecessary updates
		string previousTimeString = this.TimeString;

		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				try
				{
					if (!MemoryService.IsProcessAlive || !GameService.Instance.IsSignedIn || this.frameworkMemory == null)
					{
						if (this.Freeze)
							this.Freeze = false;

						await Task.Delay(TASK_SUCCESS_DELAY, cancellationToken);
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

					await Task.Delay(TASK_SUCCESS_DELAY, cancellationToken);
				}
				catch (TaskCanceledException)
				{
					// Task was canceled, exit the loop.
					break;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to update time");
					await Task.Delay(TASK_FAILURE_DELAY, cancellationToken);
				}
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
			}
		}
	}
}
