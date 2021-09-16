// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Memory;
	using Lumina.Data;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	using LuminaData = global::Lumina.GameData;

	public class GameDataService : ServiceBase<GameDataService>
	{
		private LuminaData? lumina;

		public enum ClientRegion
		{
			Global,
			Korean,
			Chinese,
		}

		public static ClientRegion Region { get; private set; }

		#pragma warning disable CS8618
		public static ISheet<IRace> Races { get; protected set; }
		public static ISheet<ITribe> Tribes { get; protected set; }
		public static ISheet<IItem> Items { get; protected set; }
		public static ISheet<IItem> Perform { get; protected set; }
		public static ISheet<IDye> Dyes { get; protected set; }
		public static ISheet<INpcBase> BaseNPCs { get; protected set; }
		public static ISheet<ITerritoryType> Territories { get; protected set; }
		public static ISheet<IWeather> Weathers { get; protected set; }
		public static ICharaMakeCustomizeData CharacterMakeCustomize { get; protected set; }
		public static ISheet<ICharaMakeType> CharacterMakeTypes { get; protected set; }
		public static ISheet<INpcResident> ResidentNPCs { get; protected set; }
		public static ExcelSheet<WeatherRate> WeatherRates { get; protected set; }
		public static ExcelSheet<EquipRaceCategory> EquipRaceCategories { get; protected set; }
		public static ISheet<Monster> Monsters { get; private set; }
		public static ISheet<Prop> Props { get; private set; }
		public static ISheet<ItemCategory> ItemCategories { get; private set; }
		#pragma warning restore CS8618

		public override Task Initialize()
		{
			string file = MemoryService.GamePath + "game/ffxivgame.ver";
			string gameVer = File.ReadAllText(file);

			if (gameVer != VersionInfo.ValidatedGameVersion)
			{
				Log.Warning($"Anamnesis has not been validated against this game version: {gameVer}. This may cause problems.");
			}

			Language defaultLuminaLaunguage = Language.English;
			Region = ClientRegion.Global;

			if (File.Exists(Path.Combine(MemoryService.GamePath, "FFXIVBoot.exe")) || File.Exists(Path.Combine(MemoryService.GamePath, "rail_files", "rail_game_identify.json")))
			{
				Region = ClientRegion.Chinese;
				defaultLuminaLaunguage = Language.ChineseSimplified;
				Log.Warning($"Anamnesis has not been validated against this game region: {Region}. This may cause problems.");
			}
			else if (File.Exists(Path.Combine(MemoryService.GamePath, "boot", "FFXIV_Boot.exe")))
			{
				Region = ClientRegion.Korean;
				defaultLuminaLaunguage = Language.Korean;
				Log.Warning($"Anamnesis has not been validated against this game region: {Region}. This may cause problems.");
			}

			try
			{
				Lumina.LuminaOptions options = new Lumina.LuminaOptions();
				options.DefaultExcelLanguage = defaultLuminaLaunguage;

				this.lumina = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\", options);

				Races = new LuminaSheet<IRace, Race, RaceViewModel>(this.lumina);
				Tribes = new LuminaSheet<ITribe, Tribe, TribeViewModel>(this.lumina);
				Items = new LuminaSheet<IItem, Lumina.Excel.GeneratedSheets.Item, GameData.ViewModels.ItemViewModel>(this.lumina);
				Dyes = new LuminaSheet<IDye, Stain, DyeViewModel>(this.lumina);
				BaseNPCs = new LuminaSheet<INpcBase, ENpcBase, NpcBaseViewModel>(this.lumina);
				Territories = new LuminaSheet<ITerritoryType, TerritoryType, TerritoryTypeViewModel>(this.lumina);
				Weathers = new LuminaSheet<IWeather, Weather, WeatherViewModel>(this.lumina);
				CharacterMakeCustomize = new CustomizeSheet(this.lumina);
				CharacterMakeTypes = new LuminaSheet<ICharaMakeType, GameData.Sheets.CharaMakeType, CharaMakeTypeViewModel>(this.lumina);
				ResidentNPCs = new LuminaSheet<INpcResident, ENpcResident, NpcResidentViewModel>(this.lumina);
				Perform = new LuminaSheet<IItem, Perform, PerformViewModel>(this.lumina);
				WeatherRates = GetNotNullExcelSheet<WeatherRate>(this.lumina);
				EquipRaceCategories = GetNotNullExcelSheet<EquipRaceCategory>(this.lumina);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
			}

			// these are json files that we write by hand
			try
			{
				Monsters = new JsonListSheet<Monster>("Data/Monsters.json");
				Props = new PropSheet("Data/Props.json");
				ItemCategories = new JsonDictionarySheet<ItemCategories, ItemCategory>("Data/ItemCategories.json");
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to read data sheets", ex);
			}

			return base.Initialize();
		}

		// no view models for these
		private static ExcelSheet<T> GetNotNullExcelSheet<T>(LuminaData lumina)
			where T : ExcelRow
		{
			ExcelSheet<T>? sheet = lumina.GetExcelSheet<T>();

			if (sheet == null)
				throw new Exception($"Missing sheet {typeof(T).Name}");

			return sheet;
		}
	}
}