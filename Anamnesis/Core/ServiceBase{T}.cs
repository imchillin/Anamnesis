// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core;

using Anamnesis.Memory.Exceptions;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public abstract class ServiceBase<T> : IService, INotifyPropertyChanged
	where T : ServiceBase<T>
{
	/// <summary>Maximum number of attempts to start the service.</summary>
	protected const int MAX_START_ATTEMPTS = 10;

	/// <summary>Delay between attempts to start the service.</summary>
	protected const int START_ATTEMPT_DELAY = 1000; // ms

	/// <summary>
	/// Gets or sets a stoppable background task for the service (if any).
	/// </summary>
	/// <remarks>
	/// Do not assign a task to this property if you want the service's task
	/// to continue running after the service is shut down.
	/// </remarks>
	protected Task? BackgroundTask = null;

	/// <summary>The internal instance of the service.</summary>
	private static T? s_instance;

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Gets the singleton instance of the service.</summary>
	public static T Instance
	{
		get
		{
			if (s_instance == null)
				throw new ServiceNotFoundException(typeof(T));

			return s_instance;
		}
	}

	/// <summary>
	/// Gets the singleton instance of the service, or null if it has not been instantiated.
	/// </summary>
	/// <remarks>
	/// The use of <see cref="InstanceOrNull"/> is discouraged in favor of <see cref="Instance"/>
	/// to avoid situations where the service's validity is required. Use this property only when
	/// you are certain that the service is not required to be running at the time of access.
	/// </remarks>
	public static T? InstanceOrNull => s_instance;

	/// <summary>
	/// Gets a value indicating whether the service is initialized.
	/// </summary>
	/// <remarks>
	/// The use of <see cref="IsInitialized"/> and <see cref="IsAlive"/> is situational based on the service.
	/// Some services do not perform any actions on startup and can be considered operational after initialization.
	/// </remarks>
	public bool IsInitialized { get; private set; } = false;

	/// <summary>
	/// Gets a value indicating whether the service is currently operational.
	/// </summary>
	/// <remarks>
	/// The use of <see cref="IsInitialized"/> and <see cref="IsAlive"/> is situational based on the service.
	/// Some services do not perform any actions on startup and can be considered operational after initialization.
	/// </remarks>
	public bool IsAlive { get; private set; } = false;

	/// <summary>Gets the contextual logger for the service.</summary>
	protected static ILogger Log => Serilog.Log.ForContext<T>();

	/// <summary>Gets or sets the cancellation token source for the service.</summary>
	protected CancellationTokenSource? CancellationTokenSource { get; set; }

	/// <summary>Gets the cancellation token for the service.</summary>
	/// <remarks>
	/// If the <see cref="CancellationTokenSource"/> is null, an empty cancellation token is returned.
	/// </remarks>
	protected CancellationToken CancellationToken => this.CancellationTokenSource?.Token ?? CancellationToken.None;

	/// <summary>
	/// Gets the dependencies of the service required for initialization or startup (depends on the service).
	/// </summary>
	protected virtual IEnumerable<IService> Dependencies => [];

	/// <summary>Initializes the service.</summary>
	/// <remarks>
	/// It is not guaranteed that the service is alive after initialization.
	/// </remarks>
	/// <returns>A <see cref="Task.CompletedTask"/> object.</returns>"/>
	public virtual Task Initialize()
	{
		s_instance = (T)this;
		this.IsInitialized = true;
		return Task.CompletedTask;
	}

	/// <summary>Starts the service.</summary>
	/// <remarks>
	/// The property <see cref="IsAlive"/> is set to true in the process.
	/// </remarks>
	/// <returns>A <see cref="Task.CompletedTask"/> object.</returns>"/>
	public async Task Start()
	{
		await this.PreStart();
		await this.OnStart();
		this.IsAlive = true;
	}

	/// <summary>Shuts down the service.</summary>
	/// <remarks>
	/// The property <see cref="IsAlive"/> is set to false in the process.
	/// </remarks>
	/// <returns>A <see cref="Task.CompletedTask"/> object.</returns>"/>
	public virtual async Task Shutdown()
	{
		this.IsAlive = false;
		this.CancellationTokenSource?.Cancel();

		// Wait for the background task to complete
		if (this.BackgroundTask != null && !this.BackgroundTask.IsCompleted)
			await this.BackgroundTask;

		await Task.CompletedTask;
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
			var notAliveServices = new List<IService?>();
			foreach (var service in services)
			{
				try
				{
					if (service == null || !service.IsAlive)
						notAliveServices.Add(service);
				}
				catch (ServiceNotFoundException)
				{
					notAliveServices.Add(service); // Service instance not yet available, treat as not alive and wait
				}
			}

			if (notAliveServices.Count == 0)
				return;

			await Task.Delay(delay);
			attempts++;
		}

		var serviceNames = services.Where(service => !service.IsAlive)
			.Select(service => service.GetType().Name)
			.ToList();

		throw new TimeoutException($"The following services failed to start within the expected time for {typeof(T).Name}: {string.Join(", ", serviceNames)}");
	}

	/// <summary>
	/// Pre-start method that is called before the service is started.
	/// </summary>
	/// <remarks>
	/// By default, this method checks if the service's dependencies are alive.
	/// </remarks>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	protected virtual async Task PreStart()
	{
		// Wait for the dependency services to be operational
		if (this.Dependencies.Any())
			await EnsureServicesAreAlive(this.Dependencies, MAX_START_ATTEMPTS, START_ATTEMPT_DELAY);
	}

	/// <summary>
	/// On-start method that is called when the service is started but before the service is marked as alive.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	protected virtual async Task OnStart()
	{
		await Task.CompletedTask;
	}

	/// <summary>
	/// Raises the <see cref="PropertyChanged"/> event for the specified property.
	/// </summary>
	/// <param name="property">The name of the property to raise the event for.</param>
	protected void RaisePropertyChanged(string property)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
	}
}
