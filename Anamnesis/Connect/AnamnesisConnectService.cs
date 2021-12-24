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
			Instance.Send($"-penumbra \"{name}\"");
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			if (MemoryService.Process == null)
			{
				Log.Warning("No ffxiv process");
				return;
			}

			comm = new CommFile(MemoryService.Process, false);

			// TODO: going to need two-way comms for this.
			IsPenumbraConnected = false;

			this.Send("Connected");
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.Send("Disconnected");
			comm?.Stop();
		}

		public void Send(string message)
		{
			try
			{
				if (comm == null)
					return;

				comm.SetAction(message);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to send message");
			}
		}
	}
}
