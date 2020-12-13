// © Anamnesis.
// Developed by W and A Walsh.
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
		private static IntPtr time;
		private static IntPtr camera;

		// Static offsets
		public static IntPtr ActorTable { get; private set; }
		public static IntPtr TargetManager { get; private set; }
		public static IntPtr GPoseActorTable { get; private set; }
		public static IntPtr GPoseTargetManager { get; private set; }
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

		public static IntPtr Camera
		{
			get
			{
				return MemoryService.ReadPtr(camera);
			}
			set
			{
				camera = value;
			}
		}

		public static IntPtr Time
		{
			get
			{
				IntPtr address = MemoryService.ReadPtr(time);

				if (address == IntPtr.Zero)
					throw new Exception("Failed to read time address");

				address += 0x88;
				return address;
			}
			set
			{
				time = value;
			}
		}

		public static IntPtr Weather
		{
			get
			{
				IntPtr address = MemoryService.ReadPtr(weather);

				if (address == IntPtr.Zero)
					throw new Exception("Failed to read weather address");

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
			tasks.Add(GetAddressFromSignature("88 91 ?? ?? ?? ?? 48 8D 3D ?? ?? ?? ??", 0, (p) => { ActorTable = p; }));
			tasks.Add(GetAddressFromSignature("48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB", 3, (p) => { TargetManager = p + 0x80; }));

			IntPtr baseAddress = MemoryService.Process.MainModule.BaseAddress;

			// TODO: replace these manual offsets with signautres
			Weather = baseAddress + 0x1D40FD8;
			Territory = baseAddress + 0x1D8A008;
			Time = baseAddress + 0x1D6A800;
			Camera = baseAddress + 0x1D8A070;
			GPoseActorTable = baseAddress + 0x1D8B660;
			GPoseTargetManager = baseAddress + 0x1D8B660;
			GPoseFilters = baseAddress + 0x1D682B8;
			GposeCheck = baseAddress + 0x1D8DE60;
			GposeCheck2 = baseAddress + 0x1D8DE40;
			GPose = baseAddress + 0x1D8A280;

			SkeletonFreezeRotation = baseAddress + 0x1463BE0; // SkeletonOffset
			SkeletonFreezeRotation2 = baseAddress + 0x1464D0D; // SkeletonOffset2
			SkeletonFreezeRotation3 = baseAddress + 0x1472974; // SkeletonOffset3
			SkeletonFreezeScale = baseAddress + 0x1463BF0; // SkeletonOffset4
			SkeletonFreezePosition = baseAddress + 0x1463B6B;  // SkeletonOffset5
			SkeletonFreezeScale2 = baseAddress + 0x1464D1D; // SkeletonOffset6
			SkeletonFreezePosition2 = baseAddress + 0x1464C98; // SkeletonOffset7
			SkeletonFreezePhysics = baseAddress + 0x38A768; // PhysicsOffset
			SkeletonFreezePhysics2 = baseAddress + 0x38A75F; // PhysicsOffset2
			SkeletonFreezePhysics3 = baseAddress + 0x38A772; // PhysicsOffset3

			await Task.WhenAll(tasks.ToArray());

			Log.Write($"Took {sw.ElapsedMilliseconds}ms to scan for {tasks.Count} addresses");
		}

		private static Task GetAddressFromSignature(string signature, int offset, Action<IntPtr> callback)
		{
			if (MemoryService.Scanner == null)
				throw new Exception("No memory scanner");

			return Task.Run(() =>
			{
				IntPtr ptr = MemoryService.Scanner.GetStaticAddressFromSig(signature, offset);
				callback.Invoke(ptr);
			});
		}
	}
}
