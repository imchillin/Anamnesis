// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Actor;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using LuminaData = global::Lumina.GameData;

public class GameDataService : ServiceBase<GameDataService>
{
	internal static LuminaData? LuminaData;

	private static Dictionary<string, string>? npcNames;
	private static Dictionary<uint, ItemCategories>? itemCategories;

	public enum ClientRegion
	{
		Global,
		Korean,
		Chinese,
	}

	public static ClientRegion Region { get; private set; }

	public static ExcelSheet<Race> Races { get; private set; } = null!;
	public static ExcelSheet<Tribe> Tribes { get; private set; } = null!;
	public static ExcelSheet<Item> Items { get; private set; } = null!;
	public static ExcelSheet<Perform> Perform { get; private set; } = null!;
	public static ExcelSheet<Stain> Dyes { get; private set; } = null!;
	public static ExcelSheet<EventNpc> EventNPCs { get; private set; } = null!;
	public static ExcelSheet<BattleNpc> BattleNPCs { get; private set; } = null!;
	public static ExcelSheet<Mount> Mounts { get; private set; } = null!;
	public static ExcelSheet<MountCustomize> MountCustomize { get; private set; } = null!;
	public static ExcelSheet<Companion> Companions { get; private set; } = null!;
	public static ExcelSheet<Territory> Territories { get; private set; } = null!;
	public static ExcelSheet<Weather> Weathers { get; private set; } = null!;
	public static ExcelSheet<CharaMakeCustomize> CharacterMakeCustomize { get; private set; } = null!;
	public static ExcelSheet<CharaMakeType> CharacterMakeTypes { get; private set; } = null!;
	public static ExcelSheet<ResidentNpc> ResidentNPCs { get; private set; } = null!;
	public static ExcelSheet<WeatherRate> WeatherRates { get; private set; } = null!;
	public static ExcelSheet<EquipRaceCategory> EquipRaceCategories { get; private set; } = null!;
	public static ExcelSheet<BattleNpcName> BattleNpcNames { get; private set; } = null!;
	public static ExcelSheet<GameData.Excel.Action> Actions { get; private set; } = null!;
	public static ExcelSheet<ActionTimeline> ActionTimelines { get; private set; } = null!;
	public static ExcelSheet<Emote> Emotes { get; private set; } = null!;
	public static ExcelSheet<Ornament> Ornaments { get; private set; } = null!;
	public static ExcelSheet<BuddyEquip> BuddyEquips { get; private set; } = null!;
	public static ExcelSheet<Glasses> Glasses { get; private set; } = null!;

	public static EquipmentSheet Equipment { get; private set; } = null!;

	public static ExcelSheet<T> GetExcelSheet<T>(Language? language = null, string? name = null)
					where T : struct, IExcelRow<T>
	{
		if (LuminaData == null)
			throw new InvalidOperationException("LuminaData is not initialized.");

		return LuminaData.Excel.GetSheet<T>(language, name);
	}

	public static RowRef<T> CreateRef<T>(uint rowId)
		where T : struct, IExcelRow<T>
	{
		if (LuminaData == null)
			throw new InvalidOperationException("LuminaData is not initialized.");

		return new(LuminaData.Excel, rowId);
	}

	public static byte[] GetFileData(string path)
	{
		if (LuminaData == null)
			throw new Exception("Game Data Service has not been initialized");

		FileResource? file = LuminaData.GetFile(path) ?? throw new Exception($"Failed to read file from game data: \"{path}\"");
		return file.Data;
	}

	public static string? GetNpcName(INpcBase npc)
	{
		if (npcNames == null)
			return null;

		string stringKey = npc.ToStringKey();

		if (!npcNames.TryGetValue(stringKey, out string? name))
			return null;

		// Is this a BattleNpcName entry?
		if (name.Contains("N:"))
		{
			if (BattleNpcNames == null)
				return name;

			uint bNpcNameKey = uint.Parse(name.Remove(0, 2));

			BattleNpcName? row = BattleNpcNames.GetRow(bNpcNameKey);
			if (row == null || string.IsNullOrEmpty(row.Value.Name))
				return name;

			return row.Value.Name;
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
			Equipment = new EquipmentSheet("Data/Equipment.json");
			itemCategories = EmbeddedFileUtility.Load<Dictionary<uint, ItemCategories>>("Data/ItemCategories.json");
			npcNames = EmbeddedFileUtility.Load<Dictionary<string, string>>("Data/NpcNames.json");
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to read data sheets", ex);
		}

		try
		{
			Lumina.LuminaOptions options = new()
			{
				DefaultExcelLanguage = defaultLuminaLaunguage,
				LoadMultithreaded = true,
				CacheFileResources = true,
				PanicOnSheetChecksumMismatch = true,
			};

			LuminaData = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\", options);

			Races = GetExcelSheet<Race>();
			Tribes = GetExcelSheet<Tribe>();
			Items = GetExcelSheet<Item>();
			Dyes = GetExcelSheet<Stain>();
			EventNPCs = GetExcelSheet<EventNpc>();
			BattleNPCs = GetExcelSheet<BattleNpc>();
			Mounts = GetExcelSheet<Mount>();
			MountCustomize = GetExcelSheet<MountCustomize>();
			Companions = GetExcelSheet<Companion>();
			Territories = GetExcelSheet<Territory>();
			Weathers = GetExcelSheet<Weather>();
			CharacterMakeCustomize = GetExcelSheet<CharaMakeCustomize>();
			CharacterMakeTypes = GetExcelSheet<CharaMakeType>();
			ResidentNPCs = GetExcelSheet<ResidentNpc>();
			Perform = GetExcelSheet<Perform>();
			WeatherRates = GetExcelSheet<WeatherRate>();
			EquipRaceCategories = GetExcelSheet<EquipRaceCategory>();
			BattleNpcNames = GetExcelSheet<BattleNpcName>();
			Actions = GetExcelSheet<GameData.Excel.Action>();
			ActionTimelines = GetExcelSheet<ActionTimeline>();
			Emotes = GetExcelSheet<Emote>();
			Ornaments = GetExcelSheet<Ornament>();
			BuddyEquips = GetExcelSheet<BuddyEquip>();
			Glasses = GetExcelSheet<Glasses>();
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
		}

		return base.Initialize();
	}
}
