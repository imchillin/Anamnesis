// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Actor;
using Anamnesis.Serialization;
using Anamnesis.TexTools;
using Serilog;
using XivToolsWpf;
using System.Diagnostics;

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
		catch (Exception ex)
		{
			Log.Fatal(ex, $"{typeof(T).Name} Error: {ex.Message}");
		}
	}

	public async Task InitializeServices()
	{
		StartupTimer.Start();

		await Add<LogService>();
		await Add<SerializerService>();
		await Add<SettingsService>();
		await Add<LocalizationService>();
		await Add<Updater.UpdateService>();
		await Add<ViewService>();
		await Add<MemoryService>();
		await Add<AddressService>();
		await Add<ActorService>();
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
		await Add<Keyboard.HotkeyService>();
		await Add<HistoryService>();
		await Add<CustomBoneNameService>();

		IsInitialized = true;

		Log.Information($"Services intialized in {StartupTimer.ElapsedMilliseconds}ms");

		await this.StartServices();

		StartupTimer.Restart();

		CheckWindowsVersion();

		StartupTimer.Stop();
		Log.Information($"took {StartupTimer.ElapsedMilliseconds}ms to check windows version");
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
