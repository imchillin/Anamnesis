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
		////public IntPtr ViewportActorTable { get; private set; }
		public static IntPtr LocalContentId { get; private set; }
		public static IntPtr TargetManager { get; private set; }

		// Functions
		public static IntPtr SetupTerritoryType { get; private set; }
		////public IntPtr SomeActorTableAccess { get; private set; }
		////public static IntPtr PartyListUpdate { get; private set; }
		////public IntPtr ConditionFlags { get; private set; }

		public async Task Initialize()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			List<Task> tasks = new List<Task>();

			// Scan for all static addresses
			// Signatures taken from Dalamud: https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/ClientStateAddressResolver.cs
			tasks.Add(GetAddressFromSignature("88 91 ?? ?? ?? ?? 48 8D 3D ?? ?? ?? ??", 0, (p) => { ActorTable = p; }));
			tasks.Add(GetAddressFromSignature("48 0F 44 05 ?? ?? ?? ?? 48 39 07", 0, (p) => { LocalContentId = p; }));
			tasks.Add(GetAddressFromSignature("48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB", 3, (p) => { TargetManager = p; }));
			tasks.Add(GetAddressFromSignature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F9 66 89 91 ?? ?? ?? ??", 0, (p) => { SetupTerritoryType = p; }));

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
