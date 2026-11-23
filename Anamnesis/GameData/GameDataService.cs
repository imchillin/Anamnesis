// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using LuminaData = global::Lumina.GameData;

public class GameDataService : ServiceBase<GameDataService>
{
	internal static LuminaData? s_luminaData;

	private static Dictionary<string, string>? s_npcNames;
	private static Dictionary<uint, ItemCategories>? s_itemCategories;

	public enum ClientRegion
	{
		Global,  // EN, DE, FR, JP
		Korean,  // KO
		Chinese, // CHS
		Taiwan,  // CHT, TC
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

	public static ILookup<ulong, uint> ItemsByModel { get; private set; } = null!;
	public static ILookup<ulong, uint> ItemsBySubModel { get; private set; } = null!;

	protected override IEnumerable<IService> Dependencies => [MemoryService.Instance, SettingsService.Instance, LocalizationService.Instance];

	/// <summary>
	/// Converts a <see cref="CultureInfo"/> to a supported Lumina <see cref="Language"/>.
	/// </summary>
	public static Language ConvertToLuminaLanguage(CultureInfo culture, Lumina.Data.Files.Excel.ExcelHeaderFile? header = null)
	{
		try
		{
			string languageName = CultureInfo.GetCultureInfo(culture.TwoLetterISOLanguageName).EnglishName;
			if (Enum.TryParse<Language>(languageName, ignoreCase: true, out var preferredLanguage))
			{
				if (header == null || header.Languages.Contains(preferredLanguage))
					return preferredLanguage;
			}
		}
		catch (CultureNotFoundException)
		{
			// Exit gracefully
		}

		return Language.English;
	}

	/// <summary>
	/// Resolves the Lumina <see cref="Language"/> for the currently active <see cref="LocalizationService"/> locale.
	/// </summary>
	public static Language GetLanguageForLocale(Lumina.Data.Files.Excel.ExcelHeaderFile header)
		=> ConvertToLuminaLanguage(LocalizationService.CurrentCultureInfo, header);

	public static ExcelSheet<T> GetExcelSheet<T>(Language? language = null, string? name = null)
					where T : struct, IExcelRow<T>
	{
		if (s_luminaData == null)
			throw new InvalidOperationException("LuminaData is not initialized.");

		return s_luminaData.Excel.GetSheet<T>(language, name);
	}

	public static RowRef<T> CreateRef<T>(uint rowId)
		where T : struct, IExcelRow<T>
	{
		if (s_luminaData == null)
			throw new InvalidOperationException("LuminaData is not initialized.");

		return new(s_luminaData.Excel, rowId);
	}

	public static bool TryGetRow<T>(string sheetName, uint rowId, out T row)
		where T : struct, IExcelRow<T>
		=> TryGetRow(sheetName, rowId, null, out row);

	public static bool TryGetRow<T>(string sheetName, uint rowId, Language? language, out T row)
		where T : struct, IExcelRow<T>
	{
		if (s_luminaData == null)
			throw new InvalidOperationException("LuminaData is not initialized.");

		return s_luminaData.Excel.GetSheet<T>(language, sheetName).TryGetRow(rowId, out row);
	}

	public static bool TryGetRawRow(string sheetName, uint rowId, out RawRow rawRow)
	   => TryGetRow(sheetName, rowId, out rawRow);

	public static byte[] GetFileData(string path)
	{
		if (s_luminaData == null)
			throw new Exception("Game Data Service has not been initialized");

		FileResource? file = s_luminaData.GetFile(path) ?? throw new Exception($"Failed to read file from game data: \"{path}\"");
		return file.Data;
	}

	public static bool FileExists(string path)
	{
		if (s_luminaData == null)
			throw new Exception("Game Data Service has not been initialized");

		return s_luminaData.FileExists(path);
	}

	public static string? GetNpcName(INpcBase npc)
	{
		if (s_npcNames == null)
			return null;

		string stringKey = npc.ToStringKey();

		if (!s_npcNames.TryGetValue(stringKey, out string? name))
			return null;

		// Is this a BattleNpcName entry?
		if (name.Contains("N:"))
		{
			if (BattleNpcNames == null)
				return name;

			uint bNpcNameKey = uint.Parse(name[2..]);

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
		if (s_itemCategories != null && !s_itemCategories.TryGetValue(item.RowId, out category))
			category = ItemCategories.None;

		if (FavoritesService.IsFavorite(item))
			category = category.SetFlag(ItemCategories.Favorites, true);

		if (FavoritesService.IsOwned(item))
			category = category.SetFlag(ItemCategories.Owned, true);

		return category;
	}

	public override Task Initialize()
	{
		Region = ClientRegion.Global;

		// These are JSON files that we write by hand
		try
		{
			Equipment = new EquipmentSheet("Data/Equipment.json");
			s_itemCategories = EmbeddedFileUtility.Load<Dictionary<uint, ItemCategories>>("Data/ItemCategories.json");
			s_npcNames = EmbeddedFileUtility.Load<Dictionary<string, string>>("Data/NpcNames.json");
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to read data sheets", ex);
		}

		try
		{
			Lumina.LuminaOptions options = new()
			{
				DefaultExcelLanguage = Language.English, // Default language
				LoadMultithreaded = true,
				CacheFileResources = true,
				PanicOnSheetChecksumMismatch = true,
			};

			s_luminaData = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\", options);

			// NOTE: The selection of excel header file is not that important
			// What IS important is that we target a lightweight (i.e., fewer columns) localized sheet
			var itemHeader = s_luminaData.GetFile<Lumina.Data.Files.Excel.ExcelHeaderFile>("exd/Achievement.exh");
			if (itemHeader != null && itemHeader.Languages != null && itemHeader.Languages.Length > 0)
			{
				// IMPORTANT: Global client is checked first as it contains languages for all other regions
				if (itemHeader.Languages.Contains(Language.English))
				{
					// Global (Square Enix / Steam / XIVLauncher)
					Region = ClientRegion.Global;
					s_luminaData.Options.DefaultExcelLanguage = GetLanguageForLocale(itemHeader);
				}
				else if (itemHeader.Languages.Contains(Language.ChineseSimplified))
				{
					// Mainland China (Shengqu / XIVLauncher-CN)
					Region = ClientRegion.Chinese;
					s_luminaData.Options.DefaultExcelLanguage = Language.ChineseSimplified;
				}
				else if (itemHeader.Languages.Contains(Language.Korean))
				{
					// Korean (Actoz)
					Region = ClientRegion.Korean;
					s_luminaData.Options.DefaultExcelLanguage = Language.Korean;
				}
				else if (itemHeader.Languages.Contains(Language.TraditionalChinese) || itemHeader.Languages.Contains(Language.ChineseTraditional))
				{
					// Taiwan (UserJoy)
					// NOTE: ChineseTraditional (0x06) appears to be unused
					Region = ClientRegion.Taiwan;
					s_luminaData.Options.DefaultExcelLanguage =
						itemHeader.Languages.Contains(Language.TraditionalChinese)
							? Language.TraditionalChinese
							: Language.ChineseTraditional;
				}
			}

			Log.Information($"Found game client region: {Region} (Language: {s_luminaData.Options.DefaultExcelLanguage})");

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

			ItemsByModel = Items.ToLookup(static i => i.Model, static i => i.RowId);
			ItemsBySubModel = Items.Where(static i => i.HasSubModel).ToLookup(static i => i.SubModel, static i => i.RowId);

			CustomizeOptionsCache.Build();
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
		}

		return base.Initialize();
	}
}
