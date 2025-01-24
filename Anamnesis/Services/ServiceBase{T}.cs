// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public abstract class ServiceBase<T> : IService, INotifyPropertyChanged
	where T : ServiceBase<T>
{
	private static T? instance;

	public event PropertyChangedEventHandler? PropertyChanged;

	public static T Instance
	{
		get
		{
			if (instance == null)
				throw new Exception($"No service found: {typeof(T)}");

			return instance;
		}
	}

	public static bool Exists => instance != null;

	public bool IsAlive
	{
		get;
		private set;
	}

	protected static ILogger Log => Serilog.Log.ForContext<T>();

	public virtual Task Initialize()
	{
		instance = (T)this;
		this.IsAlive = true;
		return Task.CompletedTask;
	}

	public virtual Task Shutdown()
	{
		this.IsAlive = false;
		return Task.CompletedTask;
	}

	public virtual Task Start()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Checks if the specified services are alive within a given number of attempts.
	/// </summary>
	/// <param name="services">The services to check.</param>
	/// <param name="maxAttempts">The maximum number of attempts.</param>
	/// <param name="delay">The delay between attempts in milliseconds.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <exception cref="TimeoutException">Thrown if the services are not alive within the expected time.</exception>
	protected static async Task EnsureServicesAreAlive(IEnumerable<IService> services, int maxAttempts, int delay)
	{
		int attempts = 0;
		while (attempts < maxAttempts)
		{
			var notAliveServices = services.Where(service => !service.IsAlive).ToList();
			if (!notAliveServices.Any())
				return;

			await Task.Delay(delay);
			attempts++;
		}

		var serviceNames = services.Where(service => !service.IsAlive)
			.Select(service => service.GetType().Name)
			.ToList();

		throw new TimeoutException($"The following services failed to start within the expected time: {string.Join(", ", serviceNames)}");
	}

	protected void RaisePropertyChanged(string property)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
	}
}
