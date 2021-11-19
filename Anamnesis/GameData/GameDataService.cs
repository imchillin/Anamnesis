// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Anamnesis.Character;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Memory;
	using Anamnesis.Serialization;
	using Lumina.Data;

	using LuminaData = global::Lumina.GameData;

	public class GameDataService : ServiceBase<GameDataService>
	{
		private static readonly Dictionary<Type, Lumina.Excel.ExcelSheetImpl> Sheets = new Dictionary<Type, Lumina.Excel.ExcelSheetImpl>();

		private static Dictionary<string, string>? npcNames;
		private static ExcelSheet<Lumina.Excel.GeneratedSheets.BNpcName>? battleNpcNames;

		private static LuminaData? lumina;

		public enum ClientRegion
		{
			Global,
			Korean,
			Chinese,
		}

		public static ClientRegion Region { get; private set; }

		#pragma warning disable CS8618
		public static ExcelSheet<Race> Races { get; private set; }
		public static ExcelSheet<Tribe> Tribes { get; private set; }
		public static ExcelSheet<Item> Items { get; private set; }
		public static ExcelSheet<Perform> Perform { get; private set; }
		public static ISheet<IDye> Dyes { get; private set; }
		public static ISheet<INpcBase> EventNPCs { get; private set; }
		public static ISheet<INpcBase> BattleNPCs { get; private set; }
		public static ISheet<INpcBase> Mounts { get; private set; }
		public static ISheet<INpcBase> Companions { get; private set; }
		public static ISheet<ITerritoryType> Territories { get; private set; }
		public static ISheet<IWeather> Weathers { get; private set; }
		public static ICharaMakeCustomizeData CharacterMakeCustomize { get; private set; }
		public static ExcelSheet<CharaMakeType> CharacterMakeTypes { get; private set; }
		public static ISheet<INpcBase> ResidentNPCs { get; private set; }
		public static ExcelSheet<Lumina.Excel.GeneratedSheets.WeatherRate> WeatherRates { get; private set; }
		public static ExcelSheet<EquipRaceCategory> EquipRaceCategories { get; private set; }
		public static ISheet<Prop> Props { get; private set; }
		public static ISheet<ItemCategory> ItemCategories { get; private set; }
		#pragma warning restore CS8618

		public static ExcelSheet<T> GetSheet<T>()
			where T : Lumina.Excel.ExcelRow
		{
			lock (Sheets)
			{
				Type type = typeof(T);

				Lumina.Excel.ExcelSheetImpl? sheet;
				if (Sheets.TryGetValue(type, out sheet) && sheet is ExcelSheet<T> sheetT)
					return sheetT;

				if (lumina == null)
					throw new Exception("Game Data Service has not been initialized");

				sheet = ExcelSheet<T>.GetSheet(lumina);
				Sheets.Add(type, sheet);
				return (ExcelSheet<T>)sheet;
			}
		}

		public static string? GetNpcName(INpcBase npc)
		{
			if (npcNames == null)
				return null;

			string stringKey = npc.ToStringKey();
			string? name;

			if (!npcNames.TryGetValue(stringKey, out name))
				return null;

			// Is this a BattleNpcName entry?
			if (name.Contains("B:"))
			{
				if (battleNpcNames == null)
					return name;

				uint bNpcNameKey = uint.Parse(name.Remove(0, 2));

				Lumina.Excel.GeneratedSheets.BNpcName? row = battleNpcNames.GetRow(bNpcNameKey);
				if (row == null || string.IsNullOrEmpty(row.Singular))
					return name;

				return row.Singular;
			}

			return name;
		}

		public override Task Initialize()
		{
			string file = MemoryService.GamePath + "game/ffxivgame.ver";
			string gameVer = File.ReadAllText(file);

			Log.Information($"Found game version: {gameVer}");

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
			}
			else if (File.Exists(Path.Combine(MemoryService.GamePath, "boot", "FFXIV_Boot.exe")))
			{
				Region = ClientRegion.Korean;
				defaultLuminaLaunguage = Language.Korean;
			}

			Log.Information($"Found game client region: {Region}");

			try
			{
				Lumina.LuminaOptions options = new Lumina.LuminaOptions();
				options.DefaultExcelLanguage = defaultLuminaLaunguage;

				lumina = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\", options);

				Races = GetSheet<Race>();
				Tribes = GetSheet<Tribe>();
				Items = GetSheet<Item>();

				Dyes = new LuminaSheet<IDye, Lumina.Excel.GeneratedSheets.Stain, DyeViewModel>(lumina);
				EventNPCs = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.ENpcBase, ENpcBaseViewModel>(lumina);
				BattleNPCs = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.BNpcBase, BNpcBaseViewModel>(lumina);
				battleNpcNames = ExcelSheet<Lumina.Excel.GeneratedSheets.BNpcName>.GetSheet(lumina);
				Mounts = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.Mount, MountViewModel>(lumina);
				Companions = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.Companion, CompanionViewModel>(lumina);
				Territories = new LuminaSheet<ITerritoryType, Lumina.Excel.GeneratedSheets.TerritoryType, TerritoryTypeViewModel>(lumina);
				Weathers = new LuminaSheet<IWeather, Lumina.Excel.GeneratedSheets.Weather, WeatherViewModel>(lumina);
				CharacterMakeCustomize = new CustomizeSheet(lumina);
				CharacterMakeTypes = GetSheet<CharaMakeType>();
				ResidentNPCs = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.ENpcResident, NpcResidentViewModel>(lumina);
				Perform = GetSheet<Perform>();
				WeatherRates = GetSheet<Lumina.Excel.GeneratedSheets.WeatherRate>();
				EquipRaceCategories = GetSheet<EquipRaceCategory>();
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
			}

			// these are json files that we write by hand
			try
			{
				Props = new PropSheet("Data/Props.json");
				ItemCategories = new JsonDictionarySheet<ItemCategories, ItemCategory>("Data/ItemCategories.json");
				npcNames = EmbeddedFileUtility.Load<Dictionary<string, string>>("Data/NpcNames.json");
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to read data sheets", ex);
			}

			return base.Initialize();
		}
	}
}