// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Anamnesis.Memory;

	#pragma warning disable SA1027, SA1025
	public class AddressService : ServiceBase<AddressService>
	{
		private static IntPtr weather;
		private static IntPtr cameraManager;

		// Static offsets
		public static IntPtr ActorTable { get; private set; }
		public static IntPtr GPoseActorTable { get; private set; }
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
		public static IntPtr GposeCheck { get; private set; }   // GPoseCheckOffset
		public static IntPtr GposeCheck2 { get; private set; }   // GPoseCheck2Offset
		public static IntPtr Territory { get; private set; }
		public static IntPtr GPose { get; private set; }
		public static IntPtr TimeAsm { get; private set; }
		public static IntPtr TimeReal { get; set; }
		public static IntPtr PlayerTargetSystem { get; set; }
		public static IntPtr AnimationPatch { get; set; }
		public static IntPtr SlowMotionPatch { get; set; }

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

		public static async Task Scan()
		{
			if (MemoryService.Process == null)
				return;

			if (MemoryService.Process.MainModule == null)
				throw new Exception("Process has no main module");

			Stopwatch sw = new Stopwatch();
			sw.Start();

			List<Task> tasks = new List<Task>();

			// Scan for all static addresses
			// Some signatures taken from Dalamud: https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/ClientStateAddressResolver.cs
			tasks.Add(GetAddressFromSignature("ActorTable", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 0F B6 83", 0, (p) => { ActorTable = p; }));
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezeRotation", "41 0F 29 5C 12 10", (p) => { SkeletonFreezeRotation = p; }));    // SkeletonAddress
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezeRotation2", "43 0F 29 5C 18 10", (p) => { SkeletonFreezeRotation2 = p; }));   // SkeletonAddress2
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezeRotation3", "0F 29 5E 10 49 8B 73 28", (p) => { SkeletonFreezeRotation3 = p; })); // SkeletonAddress3
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezeScale", "41 0F 29 44 12 20", (p) => { SkeletonFreezeScale = p; }));   // SkeletonAddress4
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezePosition", "41 0F 29 24 12", (p) => { SkeletonFreezePosition = p; }));   // SkeletonAddress5
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezeScale2", "43 0F 29 44 18 20", (p) => { SkeletonFreezeScale2 = p; }));  // SkeletonAddress6
			tasks.Add(GetAddressFromTextSignature("SkeletonFreezePosition2", "43 0f 29 24 18", (p) => { SkeletonFreezePosition2 = p; }));  // SkeletonAddress7
			tasks.Add(GetAddressFromTextSignature("AnimationPatch", "66 89 8B D0 00 00 00 48 8B 43 60 48 85 C0", (p) => { AnimationPatch = p; }));
			tasks.Add(GetAddressFromTextSignature("SlowMotionPatch", "F3 0F 11 94 ?? ?? ?? ?? ?? 80 89 ?? ?? ?? ?? 01", (p) => { SlowMotionPatch = p; }));
			tasks.Add(GetAddressFromSignature("Territory", "8B 1D ?? ?? ?? ?? 0F 45 D8 39 1D", 2, (p) => { Territory = p; }));
			tasks.Add(GetAddressFromSignature("Weather", "49 8B 9D ?? ?? ?? ?? 48 8D 0D", 0, (p) => { Weather = p + 0x8; }));
			tasks.Add(GetAddressFromSignature("GPoseFilters", "4C 8B 05 ?? ?? ?? ?? 41 8B 80 ?? ?? ?? ?? C1 E8 02", 0, (p) => { GPoseFilters = p; }));
			tasks.Add(GetAddressFromSignature("GposeCheck", "48 8B 15 ?? ?? ?? ?? 48 89 6C 24", 0, (p) => { GposeCheck = p; }));
			tasks.Add(GetAddressFromSignature("GposeCheck2", "8D 48 FF 48 8D 05 ?? ?? ?? ?? 8B 0C 88 48 8B 02 83 F9 04 49 8B CA", 0, (p) => { GposeCheck2 = p; }));
			tasks.Add(GetAddressFromSignature("GPoseActorTable / GPoseTargetManager", "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 8E", 0, (p) => GPoseActorTable = p + 0x14A0));
			tasks.Add(GetAddressFromSignature("GPose", "48 39 0D ?? ?? ?? ?? 75 28", 0, (p) => { GPose = p + 0x20; }));
			tasks.Add(GetAddressFromSignature("Camera", "48 8D 35 ?? ?? ?? ?? 48 8B 09", 0, (p) => { cameraManager = p; })); // CameraAddress
			tasks.Add(GetAddressFromSignature("PlayerTargetSystem", "48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB", 0, (p) => { PlayerTargetSystem = p; }));

			tasks.Add(GetAddressFromTextSignature(
				"TimeAsm",
				"48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 48 8B DA 48 81 C1 ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 8B 87",
				(p) =>
				{
					TimeAsm = p + 0xE5;
				}));

			tasks.Add(GetAddressFromTextSignature(
				"Time",
				"48 C7 05 ?? ?? ?? ?? 00 00 00 00 E8 ?? ?? ?? ?? 48 8D ?? ?? ?? 00 00 E8 ?? ?? ?? ?? 48 8D",
				(p) =>
				{
					int frameworkOffset = MemoryService.Read<int>(p + 3);
					IntPtr frameworkPtr = MemoryService.ReadPtr(p + 11 + frameworkOffset);
					TimeReal = frameworkPtr + 0x1770;

					// For reference, gpose time is at frameworkPtr + 0x1798 if it's ever needed
				}));

			tasks.Add(GetAddressFromTextSignature("SkeletonFreezePhysics (1/2/3)", "0F 29 48 10 41 0F 28 44 24 20 0F 29 40 20 48 8B 46", (p) =>
			{
				SkeletonFreezePhysics = p;  // PhysicsAddress
				SkeletonFreezePhysics2 = p - 0x9;   // SkeletonAddress2
				SkeletonFreezePhysics3 = p + 0xA;   // SkeletonAddress3
			}));

			await Task.WhenAll(tasks.ToArray());

			Log.Information($"Took {sw.ElapsedMilliseconds}ms to scan for {tasks.Count} addresses");
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
}
