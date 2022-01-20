﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Anamnesis.Connect;
	using Anamnesis.Core.Memory;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Serialization;
	using Anamnesis.TexTools;
	using Serilog;
	using XivToolsWpf;

	public class ServiceManager
	{
		private static readonly List<IService> Services = new List<IService>();

		public static bool IsInitialized { get; private set; } = false;
		public static bool IsStarted { get; private set; } = false;

		public static async Task Add<T>()
			where T : IService, new()
		{
			try
			{
				if (IsStarted)
					throw new Exception("Attempt to add service after services have started");

				IService service = Activator.CreateInstance<T>();
				Services.Add(service);
				await Dispatch.MainThread();
				await service.Initialize();

				Log.Information($"Initialized service: {typeof(T).Name}");
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, $"{typeof(T).Name} Error: {ex.Message}");
			}
		}

		public async Task InitializeServices()
		{
			await Add<LogService>();
			await Add<SerializerService>();
			await Add<SettingsService>();
			await Add<Updater.UpdateService>();
			await Add<LocalizationService>();
			await Add<ViewService>();
			await Add<MemoryService>();
			await Add<AddressService>();
			await Add<TargetService>();
			await Add<FileService>();
			await Add<TerritoryService>();
			await Add<GameService>();
			await Add<TimeService>();
			await Add<CameraService>();
			await Add<GposeService>();
			await Add<GameDataService>();
			await Add<PoseService>();
			await Add<TipService>();
			await Add<TexToolsService>();
			await Add<FavoritesService>();
			await Add<AnimationService>();
			await Add<AnamnesisConnectService>();

			IsInitialized = true;

			await this.StartServices();

			CheckWindowsVersion();
		}

		public async Task StartServices()
		{
			await Dispatch.MainThread();

			foreach (IService service in Services)
			{
				await service.Start();
			}

			IsStarted = true;
		}

		public async Task ShutdownServices()
		{
			// shutdown services in reverse order
			Services.Reverse();

			foreach (IService service in Services)
			{
				try
				{
					// If this throws an exception we should keep trying to shut down the rest
					// not doing so can leave the game memory in a corrupt state
					await service.Shutdown();
				}
				catch(Exception ex)
				{
					Log.Error(ex, "Failed to shutdown service.");
				}
			}
		}

		private static void CheckWindowsVersion()
		{
			OperatingSystem os = Environment.OSVersion;
			if (os.Platform != PlatformID.Win32NT)
				throw new Exception("Only Windows NT or later is supported");

			if (os.Version.Major < 10)
			{
				throw new Exception("Only Windows 10 or newer is supported");
			}
		}
	}
}
