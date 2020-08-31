// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using System.Threading.Tasks;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using SaintCoinach;
	using SaintCoinach.Ex;
	using SaintCoinach.Ex.Relational.Update;
	using SaintCoinach.Xiv;

	using Directory = System.IO.Directory;
	using File = System.IO.File;

	public class GameDataService : ServiceBase<GameDataService>, IGameDataService, IProgress<UpdateProgress>
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

		public override async Task Start()
		{
			await base.Start();

			string directory = MemoryService.GamePath;

			if (!IsValidInstallation(directory))
				throw new Exception("Invalid FFXIV installation");

			bool forceUpdate = true;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			if (forceUpdate)
			{
				if (File.Exists("SaintCoinach.History.zip"))
					File.Delete("SaintCoinach.History.zip");

				if (Directory.Exists("./Definitions/"))
					Directory.Delete("./Definitions/", true);
			}

			// Unzip definitions
			if (!Directory.Exists("./Definitions/"))
				ZipFile.ExtractToDirectory("./Modules/SaintCoinach/bin/Definitions.zip", "./Definitions/");

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

			List<Task> tasks = new List<Task>();
			tasks.Add(Task.Run(() => this.Items = this.Load<Table<IItem>, IItem, Item, ItemWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Races = this.Load<Table<IRace>, IRace, Race, RaceWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Tribes = this.Load<Table<ITribe>, ITribe, Tribe, TribeWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Dyes = this.Load<Table<IDye>, IDye, Stain, DyeWrapper>(realm)));
			tasks.Add(Task.Run(() => this.BaseNPCs = this.Load<Table<INpcBase>, INpcBase, ENpcBase, NpcBaseWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Territories = this.Load<Table<ITerritoryType>, ITerritoryType, TerritoryType, TerritoryTypeWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Weathers = this.Load<Table<IWeather>, IWeather, Weather, WeatherWrapper>(realm)));
			tasks.Add(Task.Run(() => this.CharacterMakeCustomize = this.Load<CustomizeTable, ICharaMakeCustomize, CharaMakeCustomize, CharacterMakeCustomizeWrapper>(realm)));
			tasks.Add(Task.Run(() => this.CharacterMakeTypes = this.Load<Table<ICharaMakeType>, ICharaMakeType, CharaMakeType, CharacterMakeTypeWrapper>(realm)));
			tasks.Add(Task.Run(() => this.ResidentNPCs = this.Load<Table<INpcResident>, INpcResident, ENpcResident, NpcResidentWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Titles = this.Load<Table<ITitle>, ITitle, Title, TitleWrapper>(realm)));
			tasks.Add(Task.Run(() => this.Statuses = this.Load<Table<IStatus>, IStatus, Status, StatusWrapper>(realm)));

			foreach (Task t in tasks)
				await t;

			Log.Write("Initialization took " + sw.ElapsedMilliseconds + "ms", @"Saint Coinach");
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
