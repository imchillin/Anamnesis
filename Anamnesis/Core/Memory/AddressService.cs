// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core.Memory;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Anamnesis.Memory;
using XivToolsWpf;

#pragma warning disable SA1027, SA1025
public class AddressService : ServiceBase<AddressService>
{
	private static IntPtr weather;
	private static IntPtr cameraManager;

	// Static offsets
	public static IntPtr ActorTable { get; private set; }
	public static IntPtr GPoseFilters { get; private set; }
	public static IntPtr SkeletonFreezeRotation { get; private set; }   // SkeletonOffset
	public static IntPtr SkeletonFreezeRotation2 { get; private set; }  // SkeletonOffset2
	public static IntPtr SkeletonFreezeRotation3 { get; private set; }  // SkeletonOffset3
	public static IntPtr SkeletonFreezeScale { get; private set; }      // SkeletonOffset4
	public static IntPtr SkeletonFreezePosition { get; private set; }   // SkeletonOffset5
	public static IntPtr SkeletonFreezeScale2 { get; private set; }     // SkeletonOffset6
	public static IntPtr SkeletonFreezePosition2 { get; private set; }  // SkeletonOffset7
	public static IntPtr SkeletonFreezePhysics { get; private set; }    // PhysicsOffset
	public static IntPtr SkeletonFreezePhysics2 { get; private set; }   // PhysicsOffset2
	public static IntPtr SkeletonFreezePhysics3 { get; private set; }   // PhysicsOffset3
	public static IntPtr WorldPositionFreeze { get; set; }
	public static IntPtr WorldRotationFreeze { get; set; }
	public static IntPtr GPoseCameraTargetPositionFreeze { get; set; }
	public static IntPtr GposeCheck { get; private set; }   // GPoseCheckOffset
	public static IntPtr GposeCheck2 { get; private set; }   // GPoseCheck2Offset
	public static IntPtr Territory { get; private set; }
	public static IntPtr GPose { get; private set; }
	public static IntPtr TimeAsm { get; private set; }
	public static IntPtr Framework { get; set; }
	public static IntPtr PlayerTargetSystem { get; set; }
	public static IntPtr AnimationSpeedPatch { get; set; }
	public static IntPtr CameraAngleXFreeze { get; set; }
	public static IntPtr CameraAngleYFreeze { get; set; }
	public static IntPtr GPoseCameraPositionFreeze { get; set; }

	public static IntPtr Camera
	{
		get
		{
			IntPtr address = MemoryService.ReadPtr(cameraManager);

			if (address == IntPtr.Zero)
				throw new Exception("Failed to read camera address");

			return address;
		}
	}

	public static IntPtr Weather
	{
		get
		{
			IntPtr address = MemoryService.ReadPtr(weather);

			if (address == IntPtr.Zero)
				throw new Exception("Failed to read weather address");

			// CMtools Weather offset
			address += 0x20;
			return address;
		}
		private set
		{
			weather = value;
		}
	}

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

	public static IntPtr GPoseCamera
	{
		get
		{
			IntPtr address = MemoryService.ReadPtr(GPose);

			if (address == IntPtr.Zero)
				throw new Exception("Failed to read gpose address");

			// CMTools CamX offset
			address += 0xA0;
			return address;
		}
	}

	public override async Task Initialize()
	{
		await base.Initialize();
		await this.Scan();
	}

	private async Task Scan()
	{
		if (MemoryService.Process == null)
			return;

		if (MemoryService.Process.MainModule == null)
			throw new Exception("Process has no main module");

		await Dispatch.NonUiThread();

		List<Task> tasks = new List<Task>();

		// Scan for all static addresses
		// Some signatures taken from Dalamud: https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/ClientStateAddressResolver.cs
		tasks.Add(this.GetAddressFromSignature("ActorTable", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 0F B6 83", 0, (p) => { ActorTable = p; }));
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezeRotation", "41 0F 29 5C 12 10", (p) => { SkeletonFreezeRotation = p; }));    // SkeletonAddress
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezeRotation2", "43 0F 29 5C 18 10", (p) => { SkeletonFreezeRotation2 = p; }));   // SkeletonAddress2
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezeRotation3", "0F 29 5E 10 49 8B 73 28", (p) => { SkeletonFreezeRotation3 = p; })); // SkeletonAddress3
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezeScale", "41 0F 29 44 12 20", (p) => { SkeletonFreezeScale = p; }));   // SkeletonAddress4
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezePosition", "41 0F 29 24 12", (p) => { SkeletonFreezePosition = p; }));   // SkeletonAddress5
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezeScale2", "43 0F 29 44 18 20", (p) => { SkeletonFreezeScale2 = p; }));  // SkeletonAddress6
		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezePosition2", "43 0f 29 24 18", (p) => { SkeletonFreezePosition2 = p; }));  // SkeletonAddress7
		tasks.Add(this.GetAddressFromTextSignature("WorldPositionFreeze", "F3 0F 11 78 ?? F3 0F 11 70 ?? F3 44 0F 11 40 ?? 48 83 48 ?? 02", (p) => { WorldPositionFreeze = p; }));
		tasks.Add(this.GetAddressFromTextSignature("WorldRotationFreeze", "0F 11 40 ?? 48 8B 8B ?? ?? ?? ?? 0F B6 81 ?? ?? ?? ?? 24 0F 3C 03 75 ?? 48 8B 01 B2 01 FF 50 ?? 48 8B 83 ?? ?? ?? ??", (p) => { WorldRotationFreeze = p; }));
		tasks.Add(this.GetAddressFromTextSignature("GPoseCameraTargetPositionFreeze", "F3 0F 11 89 ?? ?? ?? ?? 48 8B D9 F3 0F 11 91 ?? ?? ?? ??", (p) => { GPoseCameraTargetPositionFreeze = p; }));
		tasks.Add(this.GetAddressFromTextSignature("AnimationSpeedPatch", "F3 0F 11 94 ?? ?? ?? ?? ?? 80 89 ?? ?? ?? ?? 01", (p) => { AnimationSpeedPatch = p; }));
		tasks.Add(this.GetAddressFromSignature("Territory", "8B 1D ?? ?? ?? ?? 0F 45 D8 39 1D", 2, (p) => { Territory = p; }));
		tasks.Add(this.GetAddressFromSignature("Weather", "49 8B 9D ?? ?? ?? ?? 48 8D 0D", 0, (p) => { Weather = p + 0x8; }));
		tasks.Add(this.GetAddressFromSignature("GPoseFilters", "4C 8B 05 ?? ?? ?? ?? 41 8B 80 ?? ?? ?? ?? C1 E8 02", 0, (p) => { GPoseFilters = p; }));
		tasks.Add(this.GetAddressFromSignature("GposeCheck", "48 8B 15 ?? ?? ?? ?? 48 89 6C 24", 0, (p) => { GposeCheck = p; }));
		tasks.Add(this.GetAddressFromSignature("GposeCheck2", "8D 48 FF 48 8D 05 ?? ?? ?? ?? 8B 0C 88 48 8B 02 83 F9 04 49 8B CA", 0, (p) => { GposeCheck2 = p; }));
		tasks.Add(this.GetAddressFromSignature("GPose", "48 39 0D ?? ?? ?? ?? 75 28", 0, (p) => { GPose = p + 0x20; }));
		tasks.Add(this.GetAddressFromSignature("Camera", "48 8D 35 ?? ?? ?? ?? 48 8B 09", 0, (p) => { cameraManager = p; })); // CameraAddress
		tasks.Add(this.GetAddressFromSignature("PlayerTargetSystem", "48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB", 0, (p) => { PlayerTargetSystem = p; }));
		tasks.Add(this.GetAddressFromTextSignature("CameraAngleXFreeze", "F3 0F 11 83 30 01 00 00 48 83 C4 20 5B C3", (p) => { CameraAngleXFreeze = p; }));
		tasks.Add(this.GetAddressFromTextSignature("CameraAngleYFreeze", "89 83 34 01 00 00 F3 0F 10 83 40 01 00 00", (p) => { CameraAngleYFreeze = p; }));
		tasks.Add(this.GetAddressFromTextSignature("GPoseCameraPositionFreeze", "F3 0F 10 5E 08 49 8B CF F3 0F 10 56 04 F3 0F 10 0E E8", (p) => { GPoseCameraPositionFreeze = p + 0x11; }));

		tasks.Add(this.GetAddressFromTextSignature("TimeAsm", "48 89 87 ?? ?? ?? ?? 48 69 C0", (p) => TimeAsm = p));

		tasks.Add(this.GetAddressFromTextSignature(
			"Framework",
			"48 C7 05 ?? ?? ?? ?? 00 00 00 00 E8 ?? ?? ?? ?? 48 8D ?? ?? ?? 00 00 E8 ?? ?? ?? ?? 48 8D",
			(p) =>
			{
				int frameworkOffset = MemoryService.Read<int>(p + 3);
				IntPtr frameworkPtr = MemoryService.ReadPtr(p + 11 + frameworkOffset);
				Framework = frameworkPtr;
				}));

		tasks.Add(this.GetAddressFromTextSignature("SkeletonFreezePhysics (1/2/3)", "0F 29 48 10 41 0F 28 44 24 20 0F 29 40 20 48 8B 46", (p) =>
		{
			SkeletonFreezePhysics = p;  // PhysicsAddress
			SkeletonFreezePhysics2 = p - 0x9;   // SkeletonAddress2
			SkeletonFreezePhysics3 = p + 0xA;   // SkeletonAddress3
			}));

		await Task.WhenAll(tasks.ToArray());

		Log.Information($"Scanned for {tasks.Count} addresses");
	}

	private Task GetAddressFromSignature(string name, string signature, int offset, Action<IntPtr> callback)
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

	private Task GetAddressFromTextSignature(string name, string signature, Action<IntPtr> callback)
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

	private Task GetBaseAddressFromSignature(string name, string signature, int skip, bool moduleBase, Action<IntPtr> callback)
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
