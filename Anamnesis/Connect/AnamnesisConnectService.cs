// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Connect
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using AnamnesisConnect;

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

			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string path = appData + "\\XIVLauncher\\devPlugins\\AnamnesisConnect\\CommFile.txt";

			// If there isn't a commfile in devplugins, then check the default plugin dir.
			if (!File.Exists(path))
			{
				string pluginDir = appData + "\\XIVLauncher\\installedPlugins\\AnamnesisConnect\\";
				if (Directory.Exists(pluginDir))
				{
					string[] versionDirs = Directory.GetDirectories(pluginDir);
					path = versionDirs[versionDirs.Length - 1] + "\\CommFile.txt";
				}
			}

			if (!File.Exists(path))
				return;

			comm = new CommFile(path, false);

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
