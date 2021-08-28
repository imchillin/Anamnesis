// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Serialization;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class OwnedService : ServiceBase<OwnedService>
	{
		private static readonly string FilePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/Owned.json");

		public Owned? Current { get; set; }

		public static bool IsOwned(IItem item)
		{
			if (Instance.Current == null)
				return false;

			return Instance.Current.Items.Contains(item);
		}

		public static void SetOwned(IItem item, bool owned)
		{
			if (Instance.Current == null)
				return;

			bool isOwned = IsOwned(item);

			if (owned == isOwned)
				return;

			if (owned)
			{
				Instance.Current.Items.Add(item);
			}
			else
			{
				Instance.Current.Items.Remove(item);
			}

			Instance.RaisePropertyChanged(nameof(Owned.Items));
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
				this.Current = new Owned();
				Save();
			}
			else
			{
				try
				{
					string json = File.ReadAllText(FilePath);
					this.Current = SerializerService.Deserialize<Owned>(json);
				}
				catch (Exception)
				{
					await GenericDialog.Show("Failed to load owned items. Your owned items have been reset.", "Error", MessageBoxButton.OK);
					Save();
				}
			}
		}

		[Serializable]
		public class Owned
		{
			public List<IItem> Items { get; set; } = new List<IItem>();
		}
	}
}
