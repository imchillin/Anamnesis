// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Memory.Exceptions;
using Anamnesis.Serialization;
using Anamnesis.TexTools;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using XivToolsWpf;

public class ServiceManager
{
	private static readonly Stopwatch AddTimer = new();
	private static readonly Stopwatch StartupTimer = new();
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
			await InitializeService(service);
		}
		catch (UpdateTriggeredException)
		{
			throw; // Do not log as fatal, just rethrow so its handled gracefully
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, $"{typeof(T).Name} Error: {ex.Message}");
		}
	}

	public async Task InitializeServices()
	{
		StartupTimer.Start();

		try
		{
			await Add<LogService>();
			await Add<SerializerService>();
			await Add<SettingsService>();
			await Add<LocalizationService>();
			await Add<Updater.UpdateService>();
			await Add<ViewService>();
			await Add<MemoryService>();
			await Add<AddressService>();
			await Add<ActorService>();
			await Add<GameDataService>();
			await Add<GameService>();
			await Add<FileService>();
			await Add<TimeService>();
			await Add<GposeService>();
			await Add<TerritoryService>();
			await Add<TargetService>();
			await Add<CameraService>();
			await Add<PoseService>();
			await Add<TipService>();
			await Add<TexToolsService>();
			await Add<FavoritesService>();
			await Add<AnimationService>();
			await Add<Keyboard.HotkeyService>();
			await Add<HistoryService>();
			await Add<CustomBoneNameService>();
			await Add<AutoSaveService>();
		}
		catch (UpdateTriggeredException)
		{
			Log.Information("Application update has been triggered, halting further service initialization...");
			return;
		}

		IsInitialized = true;

		Log.Information($"Services intialized in {StartupTimer.ElapsedMilliseconds}ms");

		await this.StartServices();

		StartupTimer.Restart();

		CheckWindowsVersion();

		StartupTimer.Stop();
		Log.Information($"Took {StartupTimer.ElapsedMilliseconds}ms to check windows version");
	}

	public async Task StartServices()
	{
		StartupTimer.Restart();

		await Dispatch.MainThread();

		foreach (IService service in Services)
		{
			AddTimer.Restart();
			await service.Start();
			AddTimer.Stop();

			Log.Information($"Started service: {service.GetType().Name} in {AddTimer.ElapsedMilliseconds}ms");
		}

		IsStarted = true;

		StartupTimer.Stop();
		Log.Information($"Services started in {StartupTimer.ElapsedMilliseconds}ms");
	}

	public async Task ShutdownServices()
	{
		// Shutdown services in reverse order (local copy so that we don't mess up order on restart)
		var reversedServices = new List<IService>(Services);
		reversedServices.Reverse();

		foreach (IService service in reversedServices)
		{
			try
			{
				// If this throws an exception we should keep trying to shut down the rest
				// not doing so can leave the game memory in a corrupt state
				AddTimer.Restart();
				await service.Shutdown();
				AddTimer.Stop();

				Log.Information($"Shutdown service: {service.GetType().Name} in {AddTimer.ElapsedMilliseconds}ms");
			}
			catch (Exception ex)
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

	private static async Task InitializeService(IService service)
	{
		AddTimer.Restart();
		await Dispatch.NonUiThread();
		await service.Initialize();
		AddTimer.Stop();

		Log.Information($"Initialized service: {service.GetType().Name} in {AddTimer.ElapsedMilliseconds}ms");
	}
}
