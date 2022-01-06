﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Character;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Memory;
	using Lumina.Data;

	using LuminaData = global::Lumina.GameData;

	public class GameDataService : ServiceBase<GameDataService>
	{
		internal static LuminaData? LuminaData;

		private static readonly Dictionary<Type, Lumina.Excel.ExcelSheetImpl> Sheets = new Dictionary<Type, Lumina.Excel.ExcelSheetImpl>();

		private static Dictionary<string, string>? npcNames;
		private static Dictionary<uint, ItemCategories>? itemCategories;

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
		public static ExcelSheet<Stain> Dyes { get; private set; }
		public static ExcelSheet<EventNpc> EventNPCs { get; private set; }
		public static ExcelSheet<BattleNpc> BattleNPCs { get; private set; }
		public static ExcelSheet<Mount> Mounts { get; private set; }
		public static ExcelSheet<Companion> Companions { get; private set; }
		public static ExcelSheet<Territory> Territories { get; private set; }
		public static ExcelSheet<Weather> Weathers { get; private set; }
		public static ExcelSheet<CharaMakeCustomize> CharacterMakeCustomize { get; private set; }
		public static ExcelSheet<CharaMakeType> CharacterMakeTypes { get; private set; }
		public static ExcelSheet<ResidentNpc> ResidentNPCs { get; private set; }
		public static ExcelSheet<WeatherRate> WeatherRates { get; private set; }
		public static ExcelSheet<EquipRaceCategory> EquipRaceCategories { get; private set; }
		public static ExcelSheet<BattleNpcName> BattleNpcNames { get; private set; }
		public static ExcelSheet<ActionTimeline> ActionTimelines { get; private set; }
		public static ExcelSheet<Emote> Emotes { get; private set; }

		public static PropSheet Props { get; private set; }
		public static ModelListSheet ModelList { get; private set; }
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

				if (LuminaData == null)
					throw new Exception("Game Data Service has not been initialized");

				sheet = ExcelSheet<T>.GetSheet(LuminaData);
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
				if (BattleNpcNames == null)
					return name;

				uint bNpcNameKey = uint.Parse(name.Remove(0, 2));

				BattleNpcName? row = BattleNpcNames.GetRow(bNpcNameKey);
				if (row == null || string.IsNullOrEmpty(row.Singular))
					return name;

				return row.Singular;
			}

			return name;
		}

		public static ItemCategories GetCategory(Item item)
		{
			ItemCategories category = ItemCategories.None;
			if (itemCategories != null && !itemCategories.TryGetValue(item.RowId, out category))
				category = ItemCategories.None;

			if (FavoritesService.IsFavorite(item))
				category = category.SetFlag(ItemCategories.Favorites, true);

			if (FavoritesService.IsOwned(item))
				category = category.SetFlag(ItemCategories.Owned, true);

			return category;
		}

		public override Task Initialize()
		{
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

			// these are json files that we write by hand
			try
			{
				Props = new PropSheet("Data/Props.json");
				itemCategories = EmbeddedFileUtility.Load<Dictionary<uint, ItemCategories>>("Data/ItemCategories.json");
				npcNames = EmbeddedFileUtility.Load<Dictionary<string, string>>("Data/NpcNames.json");
				ModelList = new ModelListSheet("Data/ModelList.json");
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to read data sheets", ex);
			}

			try
			{
				Lumina.LuminaOptions options = new Lumina.LuminaOptions();
				options.DefaultExcelLanguage = defaultLuminaLaunguage;

				LuminaData = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\", options);

				Races = GetSheet<Race>();
				Tribes = GetSheet<Tribe>();
				Items = GetSheet<Item>();
				Dyes = GetSheet<Stain>();
				EventNPCs = GetSheet<EventNpc>();
				BattleNPCs = GetSheet<BattleNpc>();
				Mounts = GetSheet<Mount>();
				Companions = GetSheet<Companion>();
				Territories = GetSheet<Territory>();
				Weathers = GetSheet<Weather>();
				CharacterMakeCustomize = GetSheet<CharaMakeCustomize>();
				CharacterMakeTypes = GetSheet<CharaMakeType>();
				ResidentNPCs = GetSheet<ResidentNpc>();
				Perform = GetSheet<Perform>();
				WeatherRates = GetSheet<WeatherRate>();
				EquipRaceCategories = GetSheet<EquipRaceCategory>();
				BattleNpcNames = GetSheet<BattleNpcName>();
				ActionTimelines = GetSheet<ActionTimeline>();
				Emotes = GetSheet<Emote>();
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
			}

			return base.Initialize();
		}
	}
}