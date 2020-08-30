// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.GUI.Services;
	using Anamnesis.Memory;

	public class ServiceManager : IServices
	{
		private readonly List<IService> services = new List<IService>();

		public ServiceManager()
		{
			Anamnesis.Services.RegisterServicesProvider(this);
		}

		public delegate void ServiceEvent(string serviceName);

		public static event ServiceEvent? OnServiceInitializing;
		public static event ServiceEvent? OnServiceStarting;

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

		public async Task Add<T>()
			where T : IService, new()
		{
			try
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				string serviceName = GetServiceName<T>();

				IService service = Activator.CreateInstance<T>();
				this.services.Add(service);

				OnServiceInitializing?.Invoke(serviceName);
				await service.Initialize();

				// If we've already started, and this service is being added late (possibly by a module from its start method) start the service immediately.
				if (this.IsStarted)
				{
					OnServiceStarting?.Invoke(serviceName);
					await service.Start();
				}

				Log.Write($"Added service: {serviceName} in {sw.ElapsedMilliseconds}ms", "Services");
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to initialize service: {typeof(T).Name}", ex));
			}
		}

		public async Task InitializeServices()
		{
			await this.Add<LogService>();
			await this.Add<SerializerService>();
			await this.Add<SettingsService>();
			await this.Add<LocalizationService>();
			await this.Add<MemoryService>();
			await this.Add<OffsetsService>();
			await this.Add<AddressService>();
			await this.Add<ViewService>();
			await this.Add<TargetService>();
			await this.Add<FileService>();
			await this.Add<ActorRefreshService>();
			await this.Add<ModuleService>();
			await this.Add<TimeService>();

			this.IsInitialized = true;
			Log.Write($"Services Initialized", "Services");

			await this.StartServices();
		}

		public async Task StartServices()
		{
			// Since starting a service _can_ add new services, copy the list first.
			List<IService> services = new List<IService>(this.services);
			services.Reverse();
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
			// shutdown services in reverse order
			this.services.Reverse();

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
