// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.Services;

	public class ServiceManager : IServices
	{
		private List<IService> services = new List<IService>();

		public delegate void ServiceEvent(string serviceName);

		public static event ServiceEvent OnServiceInitializing;
		public static event ServiceEvent OnServiceStarting;

		public bool IsInitialized { get; private set; } = false;
		public bool IsStarted { get; private set; } = false;

		public T Get<T>()
			where T : IService
		{
			// TODO: cache these for faster lookups
			foreach (IService service in this.services)
			{
				if (typeof(T).IsAssignableFrom(service.GetType()))
				{
					return (T)service;
				}
			}

			throw new Exception($"Failed to find service: {typeof(T)}");
		}

		public async Task AddService<T>()
			where T : IService, new()
		{
			try
			{
				string serviceName = GetServiceName<T>();

				Log.Write($"Adding service: {serviceName}", "Services");
				IService service = Activator.CreateInstance<T>();
				this.services.Add(service);

				OnServiceInitializing?.Invoke(serviceName);
				await service.Initialize(this);

				// If we've already started, and this service is being added late (possibly by a module from its start method) start the service immediately.
				if (this.IsStarted)
				{
					OnServiceStarting?.Invoke(serviceName);
					await service.Start();
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to initialize service: {typeof(T).Name}", ex));
			}
		}

		public async Task InitializeServices()
		{
			await this.AddService<InjectionService>();
			await this.AddService<ViewService>();
			await this.AddService<SelectionService>();
			await this.AddService<ModuleService>();

			this.IsInitialized = true;
			Log.Write($"Services Initialized", "Services");

			await this.StartServices();
		}

		public async Task StartServices()
		{
			// Since starting a service _can_ add new services, copy the list first.
			List<IService> services = new List<IService>(this.services);
			foreach (IService service in services)
			{
				OnServiceStarting?.Invoke(GetServiceName(service.GetType()));
				await service.Start();
			}

			this.IsStarted = true;
			Log.Write($"Services Started", "Services");
		}

		public async Task ShutdownServices()
		{
			foreach (IService service in this.services)
			{
				await service.Shutdown();
			}
		}

		private static string GetServiceName<T>()
		{
			return GetServiceName(typeof(T));
		}

		private static string GetServiceName(Type type)
		{
			return type.Name;
		}
	}
}
