// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.Serialization;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class FavoritesService : ServiceBase<FavoritesService>
	{
		private static string filePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Favorites.json");

		public static List<Color4>? Colors => Instance.Current?.Colors;

		public Favorites? Current { get; set; }

		public static bool IsFavorite(IItem item)
		{
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

		public static void Save()
		{
			if (Instance.Current == null)
				return;

			string json = SerializerService.Serialize(Instance.Current);
			File.WriteAllText(filePath, json);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			if (!File.Exists(filePath))
			{
				this.Current = new Favorites();
				Save();
			}
			else
			{
				try
				{
					string json = File.ReadAllText(filePath);
					this.Current = SerializerService.Deserialize<Favorites>(json);
				}
				catch (Exception)
				{
					await GenericDialog.Show("Failed to load favorites. Your favorites have been reset.", "Error", MessageBoxButton.OK);
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
		}
	}
}
