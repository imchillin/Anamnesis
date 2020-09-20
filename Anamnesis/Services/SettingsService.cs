// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Files;
	using Anamnesis.Serialization;

	public delegate void SettingsEvent(SettingsBase settings);

	public class SettingsService : ServiceBase<SettingsService>
	{
		public const string SettingsDirectory = "Settings/";

		private static readonly Dictionary<SettingsBase, SaveJob> Jobs = new Dictionary<SettingsBase, SaveJob>();

		public static event SettingsEvent? SettingsSaved;

		public static void ShowDirectory()
		{
			string? dir = Path.GetDirectoryName(FileService.StoreDirectory + SettingsDirectory);
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", dir);
		}

		public static Task<T> Load<T>()
			where T : SettingsBase, new()
		{
			string path = FileService.StoreDirectory + SettingsDirectory + typeof(T).Name + ".json";

			T settings;
			if (!File.Exists(path))
			{
				settings = Activator.CreateInstance<T>();
				SaveImmediate(settings);
			}
			else
			{
				string json = File.ReadAllText(path);
				settings = SerializerService.Deserialize<T>(json);
			}

			if (!Jobs.ContainsKey(settings))
				Jobs.Add(settings, new SaveJob(settings));

			return Task.FromResult(settings);
		}

		public static void Save(SettingsBase settings)
		{
			if (!Jobs.ContainsKey(settings))
				Jobs.Add(settings, new SaveJob(settings));

			Jobs[settings].ResetTimer();
		}

		public static void SaveImmediate(SettingsBase settings)
		{
			string path = FileService.StoreDirectory + SettingsDirectory + settings.GetType().Name + ".json";
			string json = SerializerService.Serialize(settings);
			File.WriteAllText(path, json);
			SettingsSaved?.Invoke(settings);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			string path = FileService.StoreDirectory + SettingsDirectory;

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		private class SaveJob
		{
			private const int SaveDelay = 500;
			private int saveCountdown = 0;

			private Task? task;
			private SettingsBase settings;

			public SaveJob(SettingsBase settings)
			{
				this.settings = settings;

				INotifyPropertyChanged? propChanged = settings as INotifyPropertyChanged;

				if (propChanged == null)
					throw new Exception("Settings: " + settings.GetType() + " must implement INotifyPropertyChanged");

				propChanged.PropertyChanged += this.PropChanged_PropertyChanged;
				settings.Changed += this.OnSettingsChanged;
			}

			public void ResetTimer()
			{
				this.saveCountdown = SaveDelay;

				if (this.task == null || this.task.IsCompleted)
				{
					this.task = Task.Run(this.SaveAfterDelay);
				}
			}

			private void PropChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				this.settings.NotifyChanged();
			}

			private void OnSettingsChanged(SettingsBase settings)
			{
				this.ResetTimer();
			}

			private async Task SaveAfterDelay()
			{
				while (this.saveCountdown > 0)
				{
					while (this.saveCountdown > 0)
					{
						this.saveCountdown -= 50;
						await Task.Delay(50);
					}

					SettingsService.SaveImmediate(this.settings);
				}
			}
		}
	}

	#pragma warning disable SA1402

	[Serializable]
	public abstract class SettingsBase
	{
		public event SettingsEvent? Changed;

		public void NotifyChanged()
		{
			this.Changed?.Invoke(this);
		}
	}
}
