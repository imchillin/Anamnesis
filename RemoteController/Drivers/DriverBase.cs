// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using System;

/// <summary>
/// Base class for drivers that provides singleton access pattern.
/// </summary>
/// <typeparam name="T">The driver type.</typeparam>
public abstract class DriverBase<T> : IDisposable
	where T : DriverBase<T>
{
	protected bool DisposedValue;

	private static T? s_instance;

	/// <summary>
	/// Gets the singleton instance of the driver.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown if the driver has not been initialized.</exception>
	public static T Instance => s_instance ?? throw new InvalidOperationException($"Driver {typeof(T).Name} has not been initialized.");

	/// <summary>
	/// Gets the singleton instance of the driver, or null if not initialized.
	/// </summary>
	public static T? InstanceOrNull => s_instance;

	/// <summary>
	/// Gets a value indicating whether the driver has been initialized.
	/// </summary>
	public static bool IsInitialized => s_instance != null;

	/// <summary>
	/// Registers this instance as the singleton.
	/// </summary>
	protected void RegisterInstance()
	{
		if (s_instance != null)
			throw new InvalidOperationException($"Driver {typeof(T).Name} has already been initialized.");

		s_instance = (T)this;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (!this.DisposedValue)
		{
			this.OnDispose();
			s_instance = null;
			this.DisposedValue = true;
		}

		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Called when the driver is being disposed.
	/// </summary>
	protected abstract void OnDispose();
}