// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Connect
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using AnamnesisConnect;

	public class AnamnesisConnectService : ServiceBase<AnamnesisConnectService>
	{
		private static CommFile? comm;

		public static bool IsPenumbraConnected { get; private set; }

		public static void PenumbraRedraw(string name)
		{
			comm?.Send(Actions.PenumbraRedraw, name);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			if (MemoryService.Process == null)
			{
				Log.Warning("No ffxiv process");
				return;
			}

			comm = new CommFile(MemoryService.Process, CommFile.Mode.Client);
			comm.OnLog = (s) => Log.Information(s);
			comm.OnError = (ex) => Log.Error(ex, "Anamnesis Connect Error");

			bool connected = await comm.Connect();

			if (connected)
			{
				// TODO: going to need two-way comms for this.
				IsPenumbraConnected = true;
			}
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			comm?.Stop();
		}
	}
}
