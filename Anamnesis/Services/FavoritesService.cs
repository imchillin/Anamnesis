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

[AddINotifyPropertyChangedInterface]
public class FavoritesService : ServiceBase<FavoritesService>
{
	private static readonly string FilePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Favorites.json");

	public static List<Color4>? Colors => Instance.Current?.Colors;

	public Favorites? Current { get; set; }

	private HashSet<uint>? itemIds;
	private HashSet<uint>? dyeIds;
	private HashSet<uint>? modelIds;
	private HashSet<uint>? glassesIds;
	private HashSet<uint>? ownedIds;

	public static bool IsOwned(Item item)
	{
		if (Instance.Current == null)
			return false;

		return Instance.ownedIds != null && Instance.ownedIds.Contains(item.RowId);
	}

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

	public static void SetFavorite<T>(T item, string favsCollection, bool favorite)
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

		Instance.RaisePropertyChanged(favsCollection);
		Save();
	}

	public static void Save()
	{
		if (Instance.Current == null)
			return;

		string json = SerializerService.Serialize(Instance.Current);
		File.WriteAllText(FilePath, json);
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		if (!File.Exists(FilePath))
		{
			this.Current = new Favorites();
			Save();
		}
		else
		{
			try
			{
				string json = File.ReadAllText(FilePath);
				this.Current = SerializerService.Deserialize<Favorites>(json);
			}
			catch (Exception ex)
			{
				Log.Error(ex, LocalizationService.GetString("Error_FavoritesFail"));

				if (File.Exists(FilePath))
					File.Copy(FilePath, FilePath + ".old", true);

				this.Current = new Favorites();
				Save();
			}
		}

		this.itemIds = [.. this.Current.Items.Select(i => i.RowId)];
		this.dyeIds = [.. this.Current.Dyes.Select(d => d.RowId)];
		this.modelIds = [.. this.Current.Models.Select(m => m.RowId)];
		this.glassesIds = [.. this.Current.Glasses.Select(g => g.RowId)];
		this.ownedIds = [.. this.Current.Owned.Select(i => i.RowId)];

		await base.Initialize();
	}

	// TODO: Save on shutdown (similar to CustomBoneNameService)

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
			_ => null
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
			_ => null
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

	[Serializable]
	public class Favorites
	{
		public List<IItem> Items { get; set; } = [];
		public List<IDye> Dyes { get; set; } = [];
		public List<Color4> Colors { get; set; } = [];
		public List<INpcBase> Models { get; set; } = [];
		public List<IItem> Owned { get; set; } = [];
		public List<Glasses> Glasses { get; set; } = [];
	}
}
