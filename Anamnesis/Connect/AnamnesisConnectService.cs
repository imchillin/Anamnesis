// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Connect
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.GUI;
	using Anamnesis.Memory;
	using AnamnesisConnect;
	using XivToolsWpf;

	public class AnamnesisConnectService : ServiceBase<AnamnesisConnectService>
	{
		private static CommFile? comm;

		public static void PenumbraRedraw(string name)
		{
			Instance.Send($"/penumbra redraw {name}");
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
