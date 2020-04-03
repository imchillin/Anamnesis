// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using System.Threading.Tasks;
	using ConceptMatrix.Services;
	using SaintCoinach;
	using SaintCoinach.Ex;
	using SaintCoinach.Ex.Relational.Update;
	using SaintCoinach.Xiv;

	using Directory = System.IO.Directory;
	using File = System.IO.File;

	public class GameDataService : IGameDataService, IProgress<UpdateProgress>
	{
		public IEnumerable<IRace> Races
		{
			get;
			private set;
		}

		public IEnumerable<ITribe> Tribes
		{
			get;
			private set;
		}

		public IEnumerable<IItem> Items
		{
			get;
			private set;
		}

		public IEnumerable<IStain> Stains
		{
			get;
			private set;
		}

		public IEnumerable<INpcBase> BaseNPCs
		{
			get;
			private set;
		}

		public IEnumerable<ITerritoryType> Territories
		{
			get;
			private set;
		}

		public IEnumerable<IWeather> Weathers
		{
			get;
			private set;
		}

		public IEnumerable<ICharaMakeCustomize> CharacterMakeCustomize
		{
			get;
			private set;
		}

		public IEnumerable<ICharaMakeType> CharacterMakeTypes
		{
			get;
			private set;
		}

		public IEnumerable<INpcResident> ResidentNPCs
		{
			get;
			private set;
		}

		public IEnumerable<ITitle> Titles
		{
			get;
			private set;
		}

		public IEnumerable<IStatus> Statuses
		{
			get;
			private set;
		}

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

			Stopwatch sw = new Stopwatch();
			sw.Start();

			if (File.Exists("SaintCoinach.History.zip"))
				File.Delete("SaintCoinach.History.zip");

			// Unzip definitions
			if (Directory.Exists("./Definitions/"))
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

			Log.Write("Reading data", @"Saint Coinach");

			this.Items = this.Load<Item, ItemWrapper>(realm);
			this.Races = this.Load<Race, RaceWrapper>(realm);
			this.Tribes = this.Load<Tribe, TribeWrapper>(realm);
			this.Stains = this.Load<Stain, StainWrapper>(realm);
			this.BaseNPCs = this.Load<ENpcBase, NpcBaseWrapper>(realm);
			this.Territories = this.Load<TerritoryType, TerritoryTypeWrapper>(realm);
			this.Weathers = this.Load<Weather, WeatherWrapper>(realm);
			this.CharacterMakeCustomize = this.Load<CharaMakeCustomize, CharacterMakeCustomizeWrapper>(realm);
			this.CharacterMakeTypes = this.Load<CharaMakeType, CharacterMakeTypeWrapper>(realm);
			this.ResidentNPCs = this.Load<ENpcResident, NpcResidentWrapper>(realm);
			this.Titles = this.Load<Title, TitleWrapper>(realm);
			this.Statuses = this.Load<Status, StatusWrapper>(realm);

			Log.Write("Finished Reading data", @"Saint Coinach");
			Log.Write("Initialization took " + sw.ElapsedMilliseconds + "ms", @"Saint Coinach");
		}

		public void Report(UpdateProgress value)
		{
			// this never seems to be called
		}

		public IEnumerable<T2> Load<T, T2>(ARealmReversed realm)
			where T : XivRow
		{
			List<T2> results = new List<T2>();

			IXivSheet<T> sheet = realm.GameData.GetSheet<T>();
			foreach (T row in sheet)
			{
				results.Add((T2)Activator.CreateInstance(typeof(T2), row));
			}

			Log.Write("Loaded " + results.Count + " " + typeof(T), @"Saint Coinach");
			return results;
		}

		private static async Task<string> GetInstallationDirectory()
		{
			ISettingsService settingsService = Module.Services.Get<ISettingsService>();
			GameDataSettings settings = await settingsService.Load<GameDataSettings>();

			string installationPath = settings.InstallationPath;

			while (!IsValidInstallation(installationPath))
			{
				// TODO: dialog explaining to select game folder?
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
