// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Core.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Documents.Serialization;
	using Anamnesis.Memory;
	using SimpleLog;

	public class AddressService : IService
	{
		protected static readonly Logger Log = SimpleLog.Log.GetLogger<AddressService>();

		// Static offsets
		public static IntPtr ActorTable { get; private set; }
		public static IntPtr TargetManager { get; private set; }
		public static IntPtr Territory { get; private set; }
		public static IntPtr Weather { get; private set; }
		public static IntPtr Time { get; private set; }
		public static IntPtr Camera { get; private set; }

		#pragma warning disable SA1027, SA1025
		public static IntPtr SkeletonFreezeRotation { get; private set; }   // SkeletonOffset
		public static IntPtr SkeletonFreezeRotation2 { get; private set; }  // SkeletonOffset2
		public static IntPtr SkeletonFreezeRotation3 { get; private set; }  // SkeletonOffset3
		public static IntPtr SkeletonFreezeScale { get; private set; }      // SkeletonOffset4
		public static IntPtr SkeletonFreezePosition { get; private set; }   // SkeletonOffset5
		public static IntPtr SkeletonFreezeScale2 { get; private set; }     // SkeletonOffset6
		public static IntPtr SkeletonFreezePosition2 { get; private set; }  // SkeletonOffset7
		public static IntPtr SkeletonFreezePhysics { get; private set; }	// PhysicsOffset
		public static IntPtr SkeletonFreezePhysics2 { get; private set; }	// PhysicsOffset2
		public static IntPtr SkeletonFreezePhysics3 { get; private set; }	// PhysicsOffset3

		public async Task Initialize()
		{
			if (MemoryService.Process == null)
				return;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			List<Task> tasks = new List<Task>();

			// Scan for all static addresses
			// Some signatures taken from Dalamud: https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/ClientStateAddressResolver.cs
			tasks.Add(GetAddressFromSignature("88 91 ?? ?? ?? ?? 48 8D 3D ?? ?? ?? ??", 0, (p) => { ActorTable = p; }));
			tasks.Add(GetAddressFromSignature("48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB", 3, (p) => { TargetManager = p; }));

			IntPtr baseAddress = MemoryService.Process.MainModule.BaseAddress;

			// TODO: replace these manual offsets with signautres
			Weather = baseAddress + 0x1CC0B08;
			Territory = baseAddress + 0x1D08760;
			Time = baseAddress + 0x1CEA6E8;
			Camera = baseAddress + 0x1D09BA0;

			SkeletonFreezePosition = baseAddress + 0x140821B;
			SkeletonFreezeRotation = baseAddress + 0x1408290;
			SkeletonFreezeScale = baseAddress + 0x14082A0;
			SkeletonFreezeRotation2 = baseAddress + 0x14093BD;
			SkeletonFreezeScale2 = baseAddress + 0x14093CD;
			SkeletonFreezePosition2 = baseAddress + 0x1409348;
			SkeletonFreezeRotation3 = baseAddress + 0x1417024;
			SkeletonFreezePhysics2 = baseAddress + 0x38332F;
			SkeletonFreezePhysics = baseAddress + 0x383338;
			SkeletonFreezePhysics3 = baseAddress + 0x383342;

			await Task.WhenAll(tasks.ToArray());

			Log.Write($"Took {sw.ElapsedMilliseconds}ms to scan for {tasks.Count} addresses");
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
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
