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
	public static IntPtr SkeletonFreezeRotation { get; private set; }   // HkaPose::SyncModelSpace
	public static IntPtr SkeletonFreezeRotation2 { get; private set; }  // HkaPose::CalculateBoneModelSpace
	public static IntPtr SkeletonFreezeRotation3 { get; private set; }  // HkaLookAtIkSolver::Solve
	public static IntPtr SkeletonFreezeScale { get; private set; }      // HkaPose::SyncModelSpace
	public static IntPtr SkeletonFreezeScale2 { get; private set; }     // HkaPose::CalculateBoneModelSpace
	public static IntPtr SkeletonFreezePosition { get; private set; }   // HkaPose::SyncModelSpace
	public static IntPtr SkeletonFreezePosition2 { get; private set; }  // HkaPose::CalculateBoneModelSpace
	public static IntPtr SkeletonFreezePhysics { get; private set; }    // PhysicsOffset (Rotation)
	public static IntPtr SkeletonFreezePhysics2 { get; private set; }   // PhysicsOffset2 (Position)
	public static IntPtr SkeletonFreezePhysics3 { get; private set; }   // PhysicsOffset3 (Scale)
	public static IntPtr WorldPositionFreeze { get; set; }
	public static IntPtr WorldRotationFreeze { get; set; }
	public static IntPtr GPoseCameraTargetPositionFreeze { get; set; }
	public static IntPtr GposeCheck { get; private set; }               // GPoseCheckOffset
	public static IntPtr GposeCheck2 { get; private set; }              // GPoseCheck2Offset
	public static IntPtr Territory { get; private set; }
	public static IntPtr OverworldPlayerTarget => MemoryService.Read<IntPtr>(IntPtr.Add(TargetSystem, OVERWORLD_PLAYER_TARGET_OFFSET));
	public static IntPtr GPosePlayerTarget => MemoryService.Read<IntPtr>(IntPtr.Add(TargetSystem, GPOSE_PLAYER_TARGET_OFFSET));
	public static IntPtr TimeAsm { get; private set; }
	public static IntPtr Framework { get; set; }
	public static IntPtr TargetSystem { get; set; }
	public static IntPtr AnimationSpeedPatch { get; set; }

	// The kinematic driver is used as an additional measure in freezing the player's position, rotation, and scale.
	// It is used in conjunction with the skeleton freeze to ensure the player is completely frozen.
	// Otherwise, you won't be able to move the actor's visor and top bones.
	public static IntPtr KineDriverPosition { get; private set; }
	public static IntPtr KineDriverRotation { get; private set; }
	public static IntPtr KineDriverScale { get; private set; }

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
		SkeletonFreezeRotation = IntPtr.Zero;
		SkeletonFreezeRotation2 = IntPtr.Zero;
		SkeletonFreezeRotation3 = IntPtr.Zero;
		SkeletonFreezeScale = IntPtr.Zero;
		SkeletonFreezeScale2 = IntPtr.Zero;
		SkeletonFreezePosition = IntPtr.Zero;
		SkeletonFreezePosition2 = IntPtr.Zero;
		SkeletonFreezePhysics = IntPtr.Zero;
		SkeletonFreezePhysics2 = IntPtr.Zero;
		SkeletonFreezePhysics3 = IntPtr.Zero;
		WorldPositionFreeze = IntPtr.Zero;
		WorldRotationFreeze = IntPtr.Zero;
		GPoseCameraTargetPositionFreeze = IntPtr.Zero;
		GposeCheck = IntPtr.Zero;
		GposeCheck2 = IntPtr.Zero;
		Territory = IntPtr.Zero;
		TimeAsm = IntPtr.Zero;
		Framework = IntPtr.Zero;
		TargetSystem = IntPtr.Zero;
		AnimationSpeedPatch = IntPtr.Zero;
		KineDriverPosition = IntPtr.Zero;
		KineDriverRotation = IntPtr.Zero;
		KineDriverScale = IntPtr.Zero;
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

		var tasks = new List<Task>
		{
			// Scan for all static addresses
			// Some signatures taken from Dalamud: https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/ClientStateAddressResolver.cs
			GetAddressFromSignature("ObjectTable", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 0F B6 83", 0, (p) => { ObjectTable = p; }),
			GetAddressFromTextSignature("SkeletonFreezeRotation", "41 0F 29 5C 12 10", (p) => { SkeletonFreezeRotation = p; }),
			GetAddressFromTextSignature("SkeletonFreezeRotation2", "43 0F 29 5C 18 10", (p) => { SkeletonFreezeRotation2 = p; }),
			GetAddressFromTextSignature("SkeletonFreezeRotation3", "0F 29 5E 10 49 8B 73 28", (p) => { SkeletonFreezeRotation3 = p; }),
			GetAddressFromTextSignature("SkeletonFreezeScale", "41 0F 29 44 12 20", (p) => { SkeletonFreezeScale = p; }),
			GetAddressFromTextSignature("SkeletonFreezeScale2", "43 0F 29 44 18 20", (p) => { SkeletonFreezeScale2 = p; }),
			GetAddressFromTextSignature("SkeletonFreezePosition", "41 0F 29 24 12", (p) => { SkeletonFreezePosition = p; }),
			GetAddressFromTextSignature("SkeletonFreezePosition2", "43 0f 29 24 18", (p) => { SkeletonFreezePosition2 = p; }),
			GetAddressFromTextSignature("WorldPositionFreeze", "F3 0F 11 78 ?? F3 0F 11 70 ?? F3 44 0F 11 40 ?? 48 8B 8B ?? ?? ?? ??", (p) => { WorldPositionFreeze = p; }),
			GetAddressFromTextSignature("WorldRotationFreeze", "0F 11 40 60 48 83 48 ?? ?? 48 8B 8B ?? ?? ?? ?? 0F B6 81 ?? ?? ?? ?? 24 0F 3C 03 75 08 48 8B 01 B2 01", (p) => { WorldRotationFreeze = p; }),
			GetAddressFromTextSignature("GPoseCameraTargetPositionFreeze", "F3 0F 10 4D 00 E8 ?? ?? ?? ?? 48 8B 74 24", (p) => { GPoseCameraTargetPositionFreeze = p + 5; }),
			GetAddressFromTextSignature("AnimationSpeedPatch", "F3 0F 11 94 ?? ?? ?? ?? ?? 80 89 ?? ?? ?? ?? 01", (p) => { AnimationSpeedPatch = p; }),
			GetAddressFromSignature("Territory", "8B 1D ?? ?? ?? ?? 0F 45 D8 39 1D", 2, (p) => { Territory = p; }),

			// Get the ServerWeather struct from the WeatherManager Instance.
			GetAddressFromSignature("WeatherManager", "48 8D 0D ?? ?? ?? ?? 44 0F B7 45", 3, (p) => { ServerWeather = p + 0x48; }),
			GetAddressFromSignature("GPoseFilters", "48 85 D2 4C 8B 05 ?? ?? ?? ??", 0, (p) => { GPoseFilters = p; }),
			GetAddressFromSignature("GposeCheck", "0F 84 ?? ?? ?? ?? 8B 15 ?? ?? ?? ?? 48 89 6C 24 ??", 0, (p) => { GposeCheck = p; }),
			GetAddressFromSignature("GposeCheck2", "8D 48 FF 48 8D 05 ?? ?? ?? ?? 8B 04 88 83 F8 04 49 8B CA", 3, (p) => { GposeCheck2 = p; }),
			GetAddressFromSignature("TargetSystem", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 3B C6 0F 95 C0", 3, (p) => { TargetSystem = p; }),
			GetAddressFromSignature("Camera", "48 8D 35 ?? ?? ?? ?? 48 8B 09", 0, (p) => { s_cameraManager = p; }),
			GetAddressFromTextSignature("TimeAsm", "48 89 87 ?? ?? ?? ?? 48 69 C0", (p) => TimeAsm = p),
			GetAddressFromSignature("Framework", "48 8B 1D ?? ?? ?? ?? 8B 7C 24", 3, (p) => Framework = MemoryService.ReadPtr(p)),
			GetAddressFromTextSignature("SkeletonFreezePhysics (1/2/3)", "0F 11 00 41 0F 10 4C 24 ?? 0F 11 48 10 41 0F 10 44 24 ?? 0F 11 40 20 48 8B 46 28", (p) =>
				{
					SkeletonFreezePhysics2 = p;
					SkeletonFreezePhysics = p + 0x9;
					SkeletonFreezePhysics3 = p + 0x13;
				}),
			GetAddressFromTextSignature("KineDriverPosition", "41 0F 11 04 07", (p) => { KineDriverPosition = p; }),
			GetAddressFromTextSignature("KineDriverRotation", "41 0F 11 4C 07 ?? 0F 10 41 20", (p) => { KineDriverRotation = p; }),
			GetAddressFromTextSignature("KineDriverScale", "41 0F 11 44 07 ?? 48 8B 47 28", (p) => { KineDriverScale = p; }),
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

	private static Task GetBaseAddressFromSignature(string name, string signature, int skip, bool moduleBase, Action<IntPtr> callback)
	{
		if (MemoryService.Scanner == null)
			throw new Exception("No memory scanner");

		return Task.Run(() =>
		{
			if (MemoryService.Process?.MainModule == null)
				return;

			try
			{
				IntPtr ptr = MemoryService.Scanner.ScanText(signature);

				ptr += skip;
				int offset = MemoryService.Read<int>(ptr);

				if (moduleBase)
				{
					ptr = MemoryService.Process.MainModule.BaseAddress + offset;
				}
				else
				{
					ptr += offset + 4;
				}

				callback.Invoke(ptr);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to scan memory for base address from signature: {name} (Have you tried restarting FFXIV?)");
				callback.Invoke(IntPtr.Zero);
			}
		});
	}
}
