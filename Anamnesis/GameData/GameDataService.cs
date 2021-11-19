// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
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
		private static Dictionary<string, string>? npcNames;
		private static ExcelSheet<Lumina.Excel.GeneratedSheets.BNpcName>? battleNpcNames;

		private LuminaData? lumina;

		public enum ClientRegion
		{
			Global,
			Korean,
			Chinese,
		}

		public static ClientRegion Region { get; private set; }

		#pragma warning disable CS8618
		public static ExcelSheet<Race> Races { get; protected set; }
		public static ExcelSheet<Tribe> Tribes { get; protected set; }
		public static ISheet<IItem> Items { get; protected set; }
		public static ISheet<IItem> Perform { get; protected set; }
		public static ISheet<IDye> Dyes { get; protected set; }
		public static ISheet<INpcBase> EventNPCs { get; protected set; }
		public static ISheet<INpcBase> BattleNPCs { get; protected set; }
		public static ISheet<INpcBase> Mounts { get; protected set; }
		public static ISheet<INpcBase> Companions { get; protected set; }
		public static ISheet<ITerritoryType> Territories { get; protected set; }
		public static ISheet<IWeather> Weathers { get; protected set; }
		public static ICharaMakeCustomizeData CharacterMakeCustomize { get; protected set; }
		public static ExcelSheet<CharaMakeType> CharacterMakeTypes { get; protected set; }
		public static ISheet<INpcBase> ResidentNPCs { get; protected set; }
		public static ExcelSheet<Lumina.Excel.GeneratedSheets.WeatherRate> WeatherRates { get; protected set; }
		public static ExcelSheet<Lumina.Excel.GeneratedSheets.EquipRaceCategory> EquipRaceCategories { get; protected set; }
		public static ISheet<Prop> Props { get; private set; }
		public static ISheet<ItemCategory> ItemCategories { get; private set; }
		#pragma warning restore CS8618

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

				this.lumina = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\", options);

				Races = ExcelSheet<Race>.GetSheet(this.lumina);
				Tribes = ExcelSheet<Tribe>.GetSheet(this.lumina);

				Items = new LuminaSheet<IItem, Lumina.Excel.GeneratedSheets.Item, GameData.ViewModels.ItemViewModel>(this.lumina);
				Dyes = new LuminaSheet<IDye, Lumina.Excel.GeneratedSheets.Stain, DyeViewModel>(this.lumina);
				EventNPCs = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.ENpcBase, ENpcBaseViewModel>(this.lumina);
				BattleNPCs = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.BNpcBase, BNpcBaseViewModel>(this.lumina);
				battleNpcNames = ExcelSheet<Lumina.Excel.GeneratedSheets.BNpcName>.GetSheet(this.lumina);
				Mounts = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.Mount, MountViewModel>(this.lumina);
				Companions = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.Companion, CompanionViewModel>(this.lumina);
				Territories = new LuminaSheet<ITerritoryType, Lumina.Excel.GeneratedSheets.TerritoryType, TerritoryTypeViewModel>(this.lumina);
				Weathers = new LuminaSheet<IWeather, Lumina.Excel.GeneratedSheets.Weather, WeatherViewModel>(this.lumina);
				CharacterMakeCustomize = new CustomizeSheet(this.lumina);
				CharacterMakeTypes = ExcelSheet<CharaMakeType>.GetSheet(this.lumina);
				ResidentNPCs = new LuminaSheet<INpcBase, Lumina.Excel.GeneratedSheets.ENpcResident, NpcResidentViewModel>(this.lumina);
				Perform = new LuminaSheet<IItem, Lumina.Excel.GeneratedSheets.Perform, PerformViewModel>(this.lumina);
				WeatherRates = ExcelSheet<Lumina.Excel.GeneratedSheets.WeatherRate>.GetSheet(this.lumina);
				EquipRaceCategories = ExcelSheet<Lumina.Excel.GeneratedSheets.EquipRaceCategory>.GetSheet(this.lumina);
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