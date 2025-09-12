// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Serialization;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// A services that manages user favourited elements, such as in-game items, dyes, NPCs, etc.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class FavoritesService : ServiceBase<FavoritesService>
{
	private static readonly string s_filePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Favorites.json");

	private HashSet<uint>? itemIds;
	private HashSet<uint>? dyeIds;
	private HashSet<uint>? modelIds;
	private HashSet<uint>? glassesIds;
	private HashSet<uint>? ownedIds;

	/// <summary>
	/// Retrieves a list of favourited colors.
	/// </summary>
	public static List<Color4>? Colors => Instance.Current?.Colors;

	/// <summary>
	/// The singleton instance of the <see cref="FavoritesService"/> class.
	/// </summary>
	public Favorites? Current { get; set; }

	/// <summary>
	/// Check if the target item is marked as owned.
	/// </summary>
	/// <param name="item">The item to check.</param>
	/// <returns>True if the item is owned, false otherwise.</returns>
	public static bool IsOwned(Item item)
	{
		if (Instance.Current == null)
			return false;

		return Instance.ownedIds != null && Instance.ownedIds.Contains(item.RowId);
	}

	/// <summary>
	/// Changes the owned status of the target item.
	/// </summary>
	/// <param name="item">The item to change the owned status of.</param>
	/// <param name="owned">The new owned status.</param>
	public static void SetOwned(Item item, bool owned)
	{
		if (Instance.Current == null)
			return;

		if (owned == IsOwned(item))
			return;

		if (owned)
		{
			Instance.Current.Owned.Add(item);
			Instance.ownedIds?.Add(item.RowId);
		}
		else
		{
			Instance.Current.Owned.Remove(item);
			Instance.ownedIds?.Remove(item.RowId);
		}

		Instance.RaisePropertyChanged(nameof(Favorites.Items));
		Save();
	}

	/// <summary>
	/// Checks if the target component is marked as a favorite.
	/// </summary>
	/// <typeparam name="T">The type of the item (e.g., item, dyes, NPC, etc.)</typeparam>
	/// <param name="item">The component to check.</param>
	/// <returns>True if the item is a favorite, false otherwise.</returns>
	public static bool IsFavorite<T>(T item)
	{
		HashSet<uint>? lookupTable = GetLookupTable(item);

		if (lookupTable == null)
			return false;

		if (item is Glasses glasses)
			return lookupTable.Contains(glasses.RowId);

		if (item is IDye iDye)
			return lookupTable.Contains(iDye.RowId);

		if (item is INpcBase iNpcBase)
			return lookupTable.Contains(iNpcBase.RowId);

		if (item is IItem iItem)
			return lookupTable.Contains(iItem.RowId);

		return false;
	}

	/// <summary>
	/// Changes the favorite status of the target item.
	/// </summary>
	/// <typeparam name="T">The type of the item (e.g., item, dyes, NPC, etc.)</typeparam>
	/// <param name="item">The component to change the favorite status of.</param>
	/// <param name="favorite">The new favorite status.</param>
	public static void SetFavorite<T>(T item, bool favorite)
	{
		List<T>? curInstCollection = GetInstanceCollection<T>(item);
		HashSet<uint>? lookupTable = GetLookupTable(item);

		if (curInstCollection == null || lookupTable == null)
			return;

		if (favorite == IsFavorite(item))
			return;

		if (favorite)
		{
			curInstCollection.Add(item);
			AddToLookupTable(item, lookupTable);
		}
		else
		{
			curInstCollection.Remove(item);
			RemoveFromLookupTable(item, lookupTable);
		}

		Instance.RaisePropertyChanged(GetFavoritesCollectionName(item));
		Save();
	}

	/// <summary>
	/// Saves the currently favorited components to the local file system.
	/// </summary>
	public static void Save()
	{
		if (Instance.Current == null)
			return;

		string json = SerializerService.Serialize(Instance.Current);
		File.WriteAllText(s_filePath, json);
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		await base.Initialize();

		if (!File.Exists(s_filePath))
		{
			this.Current = new Favorites();
			Save();
		}
		else
		{
			try
			{
				string json = File.ReadAllText(s_filePath);
				this.Current = SerializerService.Deserialize<Favorites>(json);
			}
			catch (Exception ex)
			{
				Log.Error(ex, LocalizationService.GetString("Error_FavoritesFail"));

				if (File.Exists(s_filePath))
					File.Copy(s_filePath, s_filePath + ".old", true);

				this.Current = new Favorites();
				Save();
			}
		}

		this.itemIds = this.Current.Items.Select(i => i.RowId).ToHashSet();
		this.dyeIds = this.Current.Dyes.Select(d => d.RowId).ToHashSet();
		this.modelIds = this.Current.Models.Select(m => m.RowId).ToHashSet();
		this.glassesIds = this.Current.Glasses.Select(g => g.RowId).ToHashSet();
		this.ownedIds = this.Current.Owned.Select(i => i.RowId).ToHashSet();
	}

	private static List<T>? GetInstanceCollection<T>(T item)
	{
		if (Instance.Current == null)
			return null;

		return item switch
		{
			Glasses => Instance.Current.Glasses as List<T>,
			IDye => Instance.Current.Dyes as List<T>,
			INpcBase => Instance.Current.Models as List<T>,
			IItem => Instance.Current.Items as List<T>,
			_ => null,
		};
	}

	private static HashSet<uint>? GetLookupTable<T>(T item)
	{
		if (Instance.Current == null)
			return null;

		return item switch
		{
			Glasses => Instance.glassesIds,
			IDye _ => Instance.dyeIds,
			INpcBase _ => Instance.modelIds,
			IItem => Instance.itemIds,
			_ => null,
		};
	}

	private static void AddToLookupTable<T>(T item, HashSet<uint> lookupTable)
	{
		switch (item)
		{
			case IItem iItem:
				lookupTable.Add(iItem.RowId);
				break;
			case IDye iDye:
				lookupTable.Add(iDye.RowId);
				break;
			case INpcBase iNpc:
				lookupTable.Add(iNpc.RowId);
				break;
			case Glasses glasses:
				lookupTable.Add(glasses.RowId);
				break;
			default:
				throw new NotImplementedException();
		}
	}

	private static void RemoveFromLookupTable<T>(T item, HashSet<uint> lookupTable)
	{
		switch (item)
		{
			case IItem iItem:
				lookupTable.Remove(iItem.RowId);
				break;
			case IDye iDye:
				lookupTable.Remove(iDye.RowId);
				break;
			case INpcBase iNpc:
				lookupTable.Remove(iNpc.RowId);
				break;
			case Glasses glasses:
				lookupTable.Remove(glasses.RowId);
				break;
			default:
				throw new NotImplementedException();
		}
	}

	private static string GetFavoritesCollectionName<T>(T item)
	{
		return item switch
		{
			Glasses => nameof(Favorites.Glasses),
			IDye => nameof(Favorites.Dyes),
			INpcBase => nameof(Favorites.Models),
			IItem => nameof(Favorites.Items),
			_ => throw new NotImplementedException("Unsupported favorites type"),
		};
	}

	/// <summary>
	/// A class that represents the user's favorited components (e.g., items, dyes, NPCs, etc.).
	/// </summary>
	[Serializable]
	public class Favorites
	{
		/// <summary>
		/// Gets or sets the list of favorited items (e.g., equipment, weapons, accessories, etc.).
		/// </summary>
		public List<IItem> Items { get; set; } = [];

		/// <summary>
		/// Gets or sets the list of favorited dyes.
		/// </summary>
		public List<IDye> Dyes { get; set; } = [];

		/// <summary>
		/// Gets or sets the list of favorited custom colors.
		/// </summary>
		public List<Color4> Colors { get; set; } = [];

		/// <summary>
		/// Gets or sets the list of favorited models (e.g., NPCs, mounts, minions, etc.).
		/// </summary>
		public List<INpcBase> Models { get; set; } = [];

		/// <summary>
		/// Gets or sets the list of owned items.
		/// </summary>
		public List<IItem> Owned { get; set; } = [];

		/// <summary>
		/// Gets or sets the list of favorited glasses (accessory).
		/// </summary>
		public List<Glasses> Glasses { get; set; } = [];
	}
}
