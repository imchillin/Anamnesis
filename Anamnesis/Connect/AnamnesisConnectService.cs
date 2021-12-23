// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Connect
{
	using System;
	using System.IO;
	using System.IO.Pipes;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using AnamnesisConnect;

	public class AnamnesisConnectService : ServiceBase<AnamnesisConnectService>
	{
		private const int ConnectionTimeout = 1000;

		private NamedPipeClientStream? client;
		private StreamReader? reader;
		private StreamWriter? writer;

		public override async Task Initialize()
		{
			await base.Initialize();
			_ = Task.Run(this.Run);
		}

		public void Send(string message)
		{
			if (this.writer == null)
				return;

			this.writer.WriteLine(message);
			this.writer.Flush();
		}

		private async Task Run()
		{
			try
			{
				if (MemoryService.Process == null)
					return;

				int procId = MemoryService.Process.Id;
				string name = Settings.PipeName + procId;

				Log.Information($"Starting client for pipe: {name}");

				this.client = new NamedPipeClientStream(Settings.PipeName);
				await this.client.ConnectAsync(ConnectionTimeout);
				this.reader = new StreamReader(this.client);
				this.writer = new StreamWriter(this.client);

				await Task.Delay(1000);

				this.Send("Hello world");

				while (this.IsAlive)
				{
					string? message = await this.reader.ReadLineAsync();
					Log.Information("Recieved message: " + message);
				}
			}
			catch (TimeoutException)
			{
				Log.Information("Anamnesis Connect timed out");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Anamnesis Connect error");
			}

			this.client?.Dispose();
			this.reader?.Dispose();
			this.writer?.Dispose();
		}
	}
}
