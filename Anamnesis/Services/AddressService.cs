// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XivToolsWpf;

/// <summary>
/// A service that handles the scanning and resolution of memory addresses from the game process.
/// </summary>
public class AddressService : ServiceBase<AddressService>
{
	public const int OVERWORLD_PLAYER_TARGET_OFFSET = 0x80;
	public const int GPOSE_PLAYER_TARGET_OFFSET = 0x98;

	private static IntPtr s_cameraManager;

	public static IntPtr ObjectTable { get; private set; }
	public static IntPtr GPoseFilters { get; private set; }
	public static IntPtr Territory { get; private set; }
	public static IntPtr OverworldPlayerTarget => MemoryService.Read<IntPtr>(IntPtr.Add(TargetSystem, OVERWORLD_PLAYER_TARGET_OFFSET));
	public static IntPtr GPosePlayerTarget => MemoryService.Read<IntPtr>(IntPtr.Add(TargetSystem, GPOSE_PLAYER_TARGET_OFFSET));
	public static IntPtr TimeAsm { get; private set; }
	public static IntPtr Framework { get; set; }
	public static IntPtr TargetSystem { get; set; }
	public static IntPtr AnimationSpeedPatch { get; set; }

	/// <summary>
	/// Gets the camera address from the game process.
	/// </summary>
	public static IntPtr Camera
	{
		get
		{
			IntPtr address = MemoryService.ReadPtr(s_cameraManager);

			if (address == IntPtr.Zero)
				throw new Exception("Failed to read camera address");

			return address;
		}
	}

	/// <summary>
	/// Gets the address pointer to the server weather struct from the game process.
	/// </summary>
	public static IntPtr ServerWeather { get; private set; } = IntPtr.Zero;

	/// <summary>
	/// Gets the address pointer to a R/W byte that represents the target weather id.
	/// </summary>
	public static IntPtr NextWeatherId => ServerWeather + 0x08;

	/// <summary>
	/// Gets the GPose weather address from the game process.
	/// </summary>
	public static IntPtr GPoseWeather
	{
		get
		{
			IntPtr address = MemoryService.ReadPtr(GPoseFilters);

			if (address == IntPtr.Zero)
				throw new Exception("Failed to read gpose filters address");

			// CMTools ForceWeather offset
			address += 0x27;
			return address;
		}
	}

	/// <inheritdoc/>
	public override async Task Shutdown()
	{
		ObjectTable = IntPtr.Zero;
		GPoseFilters = IntPtr.Zero;
		Territory = IntPtr.Zero;
		TimeAsm = IntPtr.Zero;
		Framework = IntPtr.Zero;
		TargetSystem = IntPtr.Zero;
		AnimationSpeedPatch = IntPtr.Zero;
		s_cameraManager = IntPtr.Zero;
		ServerWeather = IntPtr.Zero;
		await base.Shutdown();
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		await Scan();
		await base.OnStart();
	}

	private static async Task Scan()
	{
		if (MemoryService.Process == null)
			return;

		if (MemoryService.Process.MainModule == null)
			throw new Exception("Process has no main module");

		await Dispatch.NonUiThread();

		// Some signatures taken from FFXIVClientStructs: https://github.com/aers/FFXIVClientStructs
		var tasks = new List<Task>
		{
			GetAddressFromSignature("ObjectTable", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 0F B6 83", 0, (p) => { ObjectTable = p; }),
			GetAddressFromTextSignature("AnimationSpeedPatch", "F3 0F 11 94 ?? ?? ?? ?? ?? 80 89 ?? ?? ?? ?? 01", (p) => { AnimationSpeedPatch = p; }),
			GetAddressFromSignature("Territory", "8B 1D ?? ?? ?? ?? 0F 45 D8 39 1D", 2, (p) => { Territory = p; }),
			GetAddressFromSignature("WeatherManager", "48 8D 0D ?? ?? ?? ?? 44 0F B7 45", 3, (p) => { ServerWeather = p + 0x48; }), // Get the ServerWeather struct from the WeatherManager Instance.
			GetAddressFromSignature("GPoseFilters", "48 85 D2 4C 8B 05 ?? ?? ?? ??", 0, (p) => { GPoseFilters = p; }),
			GetAddressFromSignature("TargetSystem", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 3B C6 0F 95 C0", 3, (p) => { TargetSystem = p; }),
			GetAddressFromSignature("Camera", "48 8D 35 ?? ?? ?? ?? 48 8B 09", 0, (p) => { s_cameraManager = p; }),
			GetAddressFromTextSignature("TimeAsm", "48 89 87 ?? ?? ?? ?? 48 69 C0", (p) => TimeAsm = p),
			GetAddressFromSignature("Framework", "48 8B 1D ?? ?? ?? ?? 8B 7C 24", 0, (p) => Framework = MemoryService.ReadPtr(p)),
		};

		await Task.WhenAll(tasks.ToArray());

		Log.Information($"Scanned for {tasks.Count} addresses");
	}

	private static Task GetAddressFromSignature(string name, string signature, int offset, Action<IntPtr> callback)
	{
		if (MemoryService.Scanner == null)
			throw new Exception("No memory scanner");

		return Task.Run(() =>
		{
			try
			{
				IntPtr ptr = MemoryService.Scanner.GetStaticAddressFromSig(signature, offset);
				callback.Invoke(ptr);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to scan memory for signature: {name} (Have you tried restarting FFXIV?)");
			}
		});
	}

	private static Task GetAddressFromTextSignature(string name, string signature, Action<IntPtr> callback)
	{
		if (MemoryService.Scanner == null)
			throw new Exception("No memory scanner");

		return Task.Run(() =>
		{
			try
			{
				IntPtr ptr = MemoryService.Scanner.ScanText(signature);
				callback.Invoke(ptr);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to scan memory for text signature: {name} (Have you tried restarting FFXIV?)");
			}
		});
	}
}
