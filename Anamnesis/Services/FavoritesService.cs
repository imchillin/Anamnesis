// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Serialization;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class FavoritesService : ServiceBase<FavoritesService>
{
	private static readonly string FilePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Favorites.json");

	public static List<Color4>? Colors => Instance.Current?.Colors;

	public Favorites? Current { get; set; }

	public static bool IsFavorite(IItem item)
	{
		if (!Exists)
			return false;

		if (Instance.Current == null)
			return false;

		return Instance.Current.Items.Contains(item);
	}

	public static void SetFavorite(IItem item, bool favorite)
	{
		if (Instance.Current == null)
			return;

		bool isFavorite = IsFavorite(item);

		if (favorite == isFavorite)
			return;

		if (favorite)
		{
			Instance.Current.Items.Add(item);
		}
		else
		{
			Instance.Current.Items.Remove(item);
		}

		Instance.RaisePropertyChanged(nameof(Favorites.Items));
		Save();
	}

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

	public static bool IsFavorite(IDye item)
	{
		if (Instance.Current == null)
			return false;

		return Instance.Current.Dyes.Contains(item);
	}

	public static void SetFavorite(IDye item, bool favorite)
	{
		if (Instance.Current == null)
			return;

		bool isFavorite = IsFavorite(item);

		if (favorite == isFavorite)
			return;

		if (favorite)
		{
			Instance.Current.Dyes.Add(item);
		}
		else
		{
			Instance.Current.Dyes.Remove(item);
		}

		Instance.RaisePropertyChanged(nameof(Favorites.Items));
		Save();
	}

	public static bool IsFavorite(INpcBase item)
	{
		if (Instance.Current == null)
			return false;

		return Instance.Current.Models.Contains(item);
	}

	public static void SetFavorite(INpcBase item, bool favorite)
	{
		if (Instance.Current == null)
			return;

		bool isFavorite = IsFavorite(item);

		if (favorite == isFavorite)
			return;

		if (favorite)
		{
			Instance.Current.Models.Add(item);
		}
		else
		{
			Instance.Current.Models.Remove(item);
		}

		Instance.RaisePropertyChanged(nameof(Favorites.Items));
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

	[Serializable]
	public class Favorites
	{
		public List<IItem> Items { get; set; } = new List<IItem>();
		public List<IDye> Dyes { get; set; } = new List<IDye>();
		public List<Color4> Colors { get; set; } = new List<Color4>();
		public List<INpcBase> Models { get; set; } = new List<INpcBase>();
		public List<IItem> Owned { get; set; } = new List<IItem>();
	}
}
