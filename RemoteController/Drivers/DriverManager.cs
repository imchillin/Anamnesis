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
	private const int FRAMEWORK_TIMEOUT_TICKS = 60;
	private const int MANAGER_WAIT_TIMEOUT_MS = 1000;

	private static readonly Stopwatch s_timer = new();
	private readonly List<DriverEntry> drivers = [];
	private bool disposedValue;

	/// <summary>
	/// Defines the initialization and disposal behavior for drivers.
	/// </summary>
	/// <remarks>
	/// This is particularly important for the safe creation of function hooks.
	/// </remarks>
	public enum SyncMode
	{
		/// <summary>
		/// Standard initialization on the current thread.
		/// </summary>
		Immediate,

		/// <summary>
		/// Synchronizes initialization onto the game's framework thread.
		/// </summary>
		FrameworkThread
	}

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
			throw new InvalidOperationException("Driver manager has already been initialized.");

		// Initialize drivers in dependency order
		this.Add(() => new FrameworkDriver(), SyncMode.Immediate);
		this.Add(() => new GposeDriver(FrameworkDriver.Instance), SyncMode.FrameworkThread);
		this.Add(() => new PosingDriver(GposeDriver.Instance), SyncMode.FrameworkThread);
		this.Add(() => new ActorDriver(), SyncMode.FrameworkThread);

		this.IsInitialized = true;
		Log.Information("Driver manager initialized successfully.");
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this.disposedValue)
			return;

		// Dispose in reverse order to respect dependencies
		for (int i = this.drivers.Count - 1; i >= 0; i--)
		{
			var entry = this.drivers[i];
			try
			{
				s_timer.Restart();
				if (entry.Mode == SyncMode.FrameworkThread)
				{
					DisposeOnFramework(entry.Driver);
				}
				else
				{
					entry.Driver.Dispose();
				}

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

	private static T InitializeOnFramework<T>(Func<T> factory)
	{
		if (FrameworkDriver.Instance == null || FrameworkDriver.Instance.IsDisposed)
			throw new InvalidOperationException("FrameworkDriver instance is null. Cannot initialize on framework thread.");

		T? instance = default;
		Exception? capturedException = null;
		using var signal = new ManualResetEventSlim(false);

		FrameworkDriver.Instance.RunOnTickUntil(null, 0, FRAMEWORK_TIMEOUT_TICKS, () =>
		{
			try
			{
				instance = factory();
			}
			catch (Exception ex)
			{
				capturedException = ex;
			}
			finally
			{
				signal.Set();
			}
		});

		if (!signal.Wait(MANAGER_WAIT_TIMEOUT_MS))
			throw new TimeoutException($"Timed out initializing {typeof(T).Name} on framework thread.");

		if (capturedException != null)
			throw capturedException;

		return instance!;
	}

	private static void DisposeOnFramework(IDisposable driver)
	{
		if (FrameworkDriver.Instance == null || FrameworkDriver.Instance.IsDisposed)
		{
			Log.Warning($"Framework driver unavailable. Disposing {driver.GetType().Name} on caller thread.");
			driver.Dispose();
			return;
		}

		using var signal = new ManualResetEventSlim(false);
		Exception? capturedException = null;

		FrameworkDriver.Instance.RunOnTickUntil(null, 0, FRAMEWORK_TIMEOUT_TICKS, () =>
		{
			try
			{
				driver.Dispose();
			}
			catch (Exception ex)
			{
				capturedException = ex;
			}
			finally
			{
				signal.Set();
			}
		});

		if (!signal.Wait(MANAGER_WAIT_TIMEOUT_MS))
			throw new TimeoutException($"Timed out disposing {driver.GetType().Name} on framework thread.");

		if (capturedException != null)
			throw capturedException;
	}

	private void Add<T>(Func<T> factory, SyncMode mode)
		where T : IDisposable
	{
		try
		{
			s_timer.Restart();
			T driver;

			if (mode == SyncMode.FrameworkThread)
			{
				driver = InitializeOnFramework(factory);
			}
			else
			{
				driver = factory();
			}

			this.drivers.Add(new DriverEntry(driver, mode));
			s_timer.Stop();
			Log.Information($"Initialized driver: {typeof(T).Name} in {s_timer.ElapsedMilliseconds}ms");
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, $"Failed to initialize driver: {typeof(T).Name}");
			throw;
		}
	}

	private readonly record struct DriverEntry(IDisposable Driver, SyncMode Mode);
}