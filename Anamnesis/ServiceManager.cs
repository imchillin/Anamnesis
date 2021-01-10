// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Serialization;
	using Anamnesis.TexTools;
	using Anamnesis.XMA;
	using Serilog;

	public class ServiceManager
	{
		private static readonly List<IService> Services = new List<IService>();

		public delegate void ServiceEvent(string serviceName);

		public static event ServiceEvent? OnServiceInitializing;
		public static event ServiceEvent? OnServiceStarting;

		public static bool IsInitialized { get; private set; } = false;
		public static bool IsStarted { get; private set; } = false;

		public static async Task Add<T>()
			where T : IService, new()
		{
			try
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				string serviceName = GetServiceName<T>();

				IService service = Activator.CreateInstance<T>();
				Services.Add(service);

				OnServiceInitializing?.Invoke(serviceName);
				await service.Initialize();

				// If we've already started, and this service is being added late (possibly by a module from its start method) start the service immediately.
				if (IsStarted)
				{
					OnServiceStarting?.Invoke(serviceName);
					await service.Start();
				}

				Log.Information($"Added service: {serviceName} in {sw.ElapsedMilliseconds}ms");
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to initialize service: {typeof(T).Name}");
			}
		}

		public async Task InitializeServices()
		{
			await Add<LogService>();
			await Add<SerializerService>();
			await Add<SettingsService>();
			await Add<LocalizationService>();
			await Add<ViewService>();
			await Add<Updater.UpdateService>();
			await Add<MemoryService>();
			await Add<VersionService>();
			await Add<AddressService>();
			await Add<TargetService>();
			await Add<FileService>();
			await Add<TerritoryService>();
			await Add<GameService>();
			await Add<TimeService>();
			await Add<CameraService>();
			await Add<GposeService>();
			await Add<PoseService>();
			await Add<GameDataService>();
			await Add<TipService>();
			await Add<TexToolsService>();

			IsInitialized = true;
			Log.Information($"Services Initialized");

			await this.StartServices();
		}

		public async Task StartServices()
		{
			Stopwatch sw = new Stopwatch();

			// Since starting a service _can_ add new services, copy the list first.
			List<IService> services = new List<IService>(Services);
			services.Reverse();
			foreach (IService service in services)
			{
				string name = GetServiceName(service.GetType());
				sw.Restart();
				OnServiceStarting?.Invoke(name);
				await service.Start();

				Log.Information($"Started Service: {name} in {sw.ElapsedMilliseconds}ms");
			}

			IsStarted = true;
			Log.Information($"Services Started");
		}

		public async Task ShutdownServices()
		{
			// shutdown services in reverse order
			Services.Reverse();

			foreach (IService service in Services)
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
