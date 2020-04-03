// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Threading.Tasks;
	using ConceptMatrix.Services;
	using SaintCoinach;
	using SaintCoinach.Ex;
	using SaintCoinach.Ex.Relational.Update;

	public class GameDataService : IGameDataService, IProgress<UpdateProgress>
	{
		public Task Initialize(IServices services)
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public async Task Start()
		{
			string directory = await GetInstallationDirectory();

			if (File.Exists("SaintCoinach.History.zip"))
				File.Delete("SaintCoinach.History.zip");

			// Unzip definitions
			Directory.Delete("./Definitions/", true);
			ZipFile.ExtractToDirectory("./Modules/SaintCoinach/Definitions.zip", "./Definitions/");

			// TODO get language from language service?
			ARealmReversed realm = new ARealmReversed(directory, Language.English);

			try
			{
				if (!realm.IsCurrentVersion)
				{
					realm.Update(true, this);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(@"Failed to update Saint Coinach", ex);
			}
		}

		public void Report(UpdateProgress value)
		{
			// this never seems to be called
		}

		private static async Task<string> GetInstallationDirectory()
		{
			ISettingsService settingsService = Module.Services.Get<ISettingsService>();
			GameDataSettings settings = await settingsService.Load<GameDataSettings>();

			string installationPath = settings.InstallationPath;

			while (!IsValidInstallation(installationPath))
			{
				// TODO: dialog popup explaining to select game folder?
				IFileService fileService = Module.Services.Get<IFileService>();
				string dir = await fileService.OpenDirectory(
					"Select game installation",
					@"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\",
					@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XIV Online\");

				if (string.IsNullOrEmpty(dir))
				{
					// TODO: graceful shutdown? work without data?
					throw new Exception("Invalid installation");
				}

				installationPath = dir;
			}

			settings.InstallationPath = installationPath;
			await settings.SaveAsync();

			return installationPath;
		}

		private static bool IsValidInstallation(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			if (!Directory.Exists(path))
				return false;

			string bootFolder = Path.Combine(path, "boot");

			if (!Directory.Exists(bootFolder))
				return false;

			string gameFolder = Path.Combine(path, "game");

			if (!Directory.Exists(gameFolder))
				return false;

			return true;
		}
	}
}
