// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis;

	public class SettingsService : ISettingsService
	{
		public const string SettingsDirectory = "Settings/";

		private readonly Dictionary<SettingsBase, SaveJob> jobs = new Dictionary<SettingsBase, SaveJob>();
		private ISerializerService serializer = Services.Get<ISerializerService>();

		public event SettingsEvent? SettingsSaved;

		public static void ShowDirectory()
		{
			string? dir = Path.GetDirectoryName(FileService.StoreDirectory + SettingsDirectory);
			Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", dir);
		}

		public Task Initialize()
		{
			string path = FileService.StoreDirectory + SettingsDirectory;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			return Task.CompletedTask;
		}

		public Task<T> Load<T>()
			where T : SettingsBase, new()
		{
			string path = FileService.StoreDirectory + SettingsDirectory + typeof(T).Name + ".json";

			T settings;
			if (!File.Exists(path))
			{
				settings = Activator.CreateInstance<T>();
				this.SaveImmediate(settings);
			}
			else
			{
				string json = File.ReadAllText(path);
				settings = this.serializer.Deserialize<T>(json);
			}

			if (!this.jobs.ContainsKey(settings))
				this.jobs.Add(settings, new SaveJob(settings, this));

			return Task.FromResult(settings);
		}

		public void Save(SettingsBase settings)
		{
			if (!this.jobs.ContainsKey(settings))
				this.jobs.Add(settings, new SaveJob(settings, this));

			this.jobs[settings].ResetTimer();
		}

		public void SaveImmediate(SettingsBase settings)
		{
			string path = FileService.StoreDirectory + SettingsDirectory + settings.GetType().Name + ".json";
			string json = this.serializer.Serialize(settings);
			File.WriteAllText(path, json);
			this.SettingsSaved?.Invoke(settings);
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private class SaveJob
		{
			private const int SaveDelay = 500;
			private int saveCountdown = 0;

			private Task? task;
			private SettingsBase settings;
			private SettingsService service;

			public SaveJob(SettingsBase settings, SettingsService service)
			{
				this.settings = settings;
				this.service = service;

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

					this.service.SaveImmediate(this.settings);
				}
			}
		}
	}
}
