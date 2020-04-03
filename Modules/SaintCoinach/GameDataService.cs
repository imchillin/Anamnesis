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
		public IData<IRace> Races
		{
			get;
			private set;
		}

		public IData<ITribe> Tribes
		{
			get;
			private set;
		}

		public IData<IItem> Items
		{
			get;
			private set;
		}

		public IData<IStain> Stains
		{
			get;
			private set;
		}

		public IData<INpcBase> BaseNPCs
		{
			get;
			private set;
		}

		public IData<ITerritoryType> Territories
		{
			get;
			private set;
		}

		public IData<IWeather> Weathers
		{
			get;
			private set;
		}

		public IData<ICharaMakeCustomize> CharacterMakeCustomize
		{
			get;
			private set;
		}

		public IData<ICharaMakeType> CharacterMakeTypes
		{
			get;
			private set;
		}

		public IData<INpcResident> ResidentNPCs
		{
			get;
			private set;
		}

		public IData<ITitle> Titles
		{
			get;
			private set;
		}

		public IData<IStatus> Statuses
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

			this.Items = this.Load<IItem, Item, ItemWrapper>(realm);
			this.Races = this.Load<IRace, Race, RaceWrapper>(realm);
			this.Tribes = this.Load<ITribe, Tribe, TribeWrapper>(realm);
			this.Stains = this.Load<IStain, Stain, StainWrapper>(realm);
			this.BaseNPCs = this.Load<INpcBase, ENpcBase, NpcBaseWrapper>(realm);
			this.Territories = this.Load<ITerritoryType, TerritoryType, TerritoryTypeWrapper>(realm);
			this.Weathers = this.Load<IWeather, Weather, WeatherWrapper>(realm);
			this.CharacterMakeCustomize = this.Load<ICharaMakeCustomize, CharaMakeCustomize, CharacterMakeCustomizeWrapper>(realm);
			this.CharacterMakeTypes = this.Load<ICharaMakeType, CharaMakeType, CharacterMakeTypeWrapper>(realm);
			this.ResidentNPCs = this.Load<INpcResident, ENpcResident, NpcResidentWrapper>(realm);
			this.Titles = this.Load<ITitle, Title, TitleWrapper>(realm);
			this.Statuses = this.Load<IStatus, Status, StatusWrapper>(realm);

			Log.Write("Finished Reading data", @"Saint Coinach");
			Log.Write("Initialization took " + sw.ElapsedMilliseconds + "ms", @"Saint Coinach");
		}

		public void Report(UpdateProgress value)
		{
			// this never seems to be called
		}

		internal Table<TInterface> Load<TInterface, TRow, TWrapper>(ARealmReversed realm)
			where TRow : XivRow
			where TInterface : IDataObject
			where TWrapper : ObjectWrapper, TInterface
		{
			Table<TInterface> table = new Table<TInterface>();

			IXivSheet<TRow> sheet = realm.GameData.GetSheet<TRow>();
			table.Import<TRow, TWrapper>(sheet);
			return table;
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

		internal class Table<T> : IData<T>
			where T : IDataObject
		{
			private Dictionary<int, T> data = new Dictionary<int, T>();

			public IEnumerable<T> All
			{
				get
				{
					return this.data.Values;
				}
			}

			public int Count
			{
				get
				{
					return this.data.Count;
				}
			}

			public T Get(int key)
			{
				if (this.data.ContainsKey(key))
					return this.data[key];

				return default(T);
			}

			internal void Import<TRow, TWrapper>(IXivSheet<TRow> sheet)
				where TRow : XivRow
				where TWrapper : ObjectWrapper, T
			{
				foreach (TRow row in sheet)
				{
					TWrapper wrapper = (TWrapper)Activator.CreateInstance(typeof(TWrapper), row);
					this.data.Add(wrapper.Key, wrapper);
				}

				Log.Write("Loaded " + this.Count + " " + typeof(T).Name, @"Saint Coinach");
			}
		}
	}
}
