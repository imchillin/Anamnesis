// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Serialization;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class FavoritesService : ServiceBase<FavoritesService>
{
	private static readonly string FilePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Favorites.json");

	public static List<Color4>? Colors => Instance.Current?.Colors;

	public Favorites? Current { get; set; }

	public static bool IsOwned(Item item)
	{
		if (Instance.Current == null)
			return false;

		return Instance.Current.Owned.Contains(item);
	}

	public static void SetOwned(Item item, bool favorite)
	{
		if (Instance.Current == null)
			return;

		bool isOwned = IsOwned(item);

		if (favorite == isOwned)
			return;

		if (favorite)
		{
			Instance.Current.Owned.Add(item);
		}
		else
		{
			Instance.Current.Owned.Remove(item);
		}

		Instance.RaisePropertyChanged(nameof(Favorites.Items));
		Save();
	}

	public static bool IsFavorite<T>(T item)
	{
		List<T>? curInstCollection = GetInstanceCollection(item);

		if (curInstCollection == null)
			return false;

		return curInstCollection.Contains(item);
	}

	public static void SetFavorite<T>(T item, string favsCollection, bool favorite)
	{
		List<T>? curInstCollection = GetInstanceCollection<T>(item);

		if (curInstCollection == null)
			return;

		bool isFavorite = IsFavorite<T>(item);

		if (favorite == isFavorite)
			return;

		if (favorite)
		{
			curInstCollection.Add(item);
		}
		else
		{
			curInstCollection.Remove(item);
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

	public override async Task Initialize()
	{
		await base.Initialize();

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
	}

	private static List<T>? GetInstanceCollection<T>(T item)
	{
		if (Instance.Current == null)
			return null;

		if (item is Glasses)
		{
			return Instance.Current.Glasses as List<T>;
		}
		else if (item is IDye)
		{
			return Instance.Current.Dyes as List<T>;
		}
		else if (item is INpcBase)
		{
			return Instance.Current.Models as List<T>;
		}
		else if (item is IItem)
		{
			return Instance.Current.Items as List<T>;
		}
		else
		{
			return null;
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
