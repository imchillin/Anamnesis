// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class DriverManager : IDisposable
{
	private static readonly Stopwatch s_timer = new();
	private readonly List<IDisposable> drivers = [];
	private bool disposedValue;

	/// <summary>
	/// Gets a value indicating whether the driver manager has been initialized.
	/// </summary>
	public bool IsInitialized { get; private set; }

	/// <summary>
	/// Initializes all drivers in dependency order.
	/// </summary>
	public void Initialize()
	{
		if (this.IsInitialized)
			throw new InvalidOperationException("DriverManager has already been initialized.");

		// Initialize drivers in dependency order
		this.Add(() => new FrameworkDriver());
		this.Add(() => new GposeDriver(FrameworkDriver.Instance));
		this.Add(() => new PosingDriver(GposeDriver.Instance));

		this.IsInitialized = true;
		Log.Information("DriverManager initialized successfully.");
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this.disposedValue)
			return;

		// Dispose in reverse order to respect dependencies
		for (int i = this.drivers.Count - 1; i >= 0; i--)
		{
			try
			{
				s_timer.Restart();
				this.drivers[i].Dispose();
				s_timer.Stop();

				Log.Information($"Disposed driver: {this.drivers[i].GetType().Name} in {s_timer.ElapsedMilliseconds}ms");
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to dispose driver: {this.drivers[i].GetType().Name}");
			}
		}

		this.drivers.Clear();
		this.IsInitialized = false;
		this.disposedValue = true;

		GC.SuppressFinalize(this);
	}

	private void Add<T>(Func<T> factory)
		where T : IDisposable
	{
		try
		{
			s_timer.Restart();
			var driver = factory();
			this.drivers.Add(driver);
			s_timer.Stop();

			Log.Information($"Initialized driver: {typeof(T).Name} in {s_timer.ElapsedMilliseconds}ms");
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, $"Failed to initialize driver: {typeof(T).Name}");
			throw;
		}
	}
}