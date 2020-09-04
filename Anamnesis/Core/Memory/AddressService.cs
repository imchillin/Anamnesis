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

			// TODO: replace these manual offsets with signautres
			Weather = MemoryService.Process.MainModule.BaseAddress + 0x1CBFB08;
			Territory = MemoryService.Process.MainModule.BaseAddress + 0x1D07760;
			Time = MemoryService.Process.MainModule.BaseAddress + 0x1CE96E8;
			Camera = MemoryService.Process.MainModule.BaseAddress + 0x1D08BA0;

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
