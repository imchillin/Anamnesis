// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using System.Threading.Tasks;
	using ConceptMatrix.GameData;
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

		public IData<IDye> Dyes
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

		public ICharaMakeCustomizeData CharacterMakeCustomize
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

		public Task Initialize()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			IInjectionService injection = Services.Get<IInjectionService>();

			string directory = injection.GamePath;

			if (!IsValidInstallation(directory))
				throw new Exception("Invalid FFXIV installation");

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

			this.Items = this.Load<Table<IItem>, IItem, Item, ItemWrapper>(realm);
			this.Races = this.Load<Table<IRace>, IRace, Race, RaceWrapper>(realm);
			this.Tribes = this.Load<Table<ITribe>, ITribe, Tribe, TribeWrapper>(realm);
			this.Dyes = this.Load<Table<IDye>, IDye, Stain, DyeWrapper>(realm);
			this.BaseNPCs = this.Load<Table<INpcBase>, INpcBase, ENpcBase, NpcBaseWrapper>(realm);
			this.Territories = this.Load<Table<ITerritoryType>, ITerritoryType, TerritoryType, TerritoryTypeWrapper>(realm);
			this.Weathers = this.Load<Table<IWeather>, IWeather, Weather, WeatherWrapper>(realm);
			this.CharacterMakeCustomize = this.Load<CustomizeTable, ICharaMakeCustomize, CharaMakeCustomize, CharacterMakeCustomizeWrapper>(realm);
			this.CharacterMakeTypes = this.Load<Table<ICharaMakeType>, ICharaMakeType, CharaMakeType, CharacterMakeTypeWrapper>(realm);
			this.ResidentNPCs = this.Load<Table<INpcResident>, INpcResident, ENpcResident, NpcResidentWrapper>(realm);
			this.Titles = this.Load<Table<ITitle>, ITitle, Title, TitleWrapper>(realm);
			this.Statuses = this.Load<Table<IStatus>, IStatus, Status, StatusWrapper>(realm);

			Log.Write("Finished Reading data", @"Saint Coinach");
			Log.Write("Initialization took " + sw.ElapsedMilliseconds + "ms", @"Saint Coinach");
			return Task.CompletedTask;
		}

		public void Report(UpdateProgress value)
		{
			// this never seems to be called
		}

		internal TTable Load<TTable, TInterface, TRow, TWrapper>(ARealmReversed realm)
			where TRow : XivRow
			where TInterface : IDataObject
			where TTable : Table<TInterface>
			where TWrapper : ObjectWrapper, TInterface
		{
			TTable table = Activator.CreateInstance<TTable>();

			IXivSheet<TRow> sheet = realm.GameData.GetSheet<TRow>();
			table.Import<TRow, TWrapper>(sheet);

			return table;
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
