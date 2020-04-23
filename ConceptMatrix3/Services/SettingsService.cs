// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Serialization;

	public class SettingsService : ISettingsService
	{
		public const string SettingsDirectory = "./Settings/";

		public event SettingsEvent SettingsSaved;

		public Task Initialize()
		{
			if (!Directory.Exists(SettingsDirectory))
				Directory.CreateDirectory(SettingsDirectory);

			return Task.CompletedTask;
		}

		public async Task<T> Load<T>()
			where T : SettingsBase, new()
		{
			string path = SettingsDirectory + typeof(T).Name + ".json";

			T settings;
			if (!File.Exists(path))
			{
				settings = Activator.CreateInstance<T>();
				await this.Save(settings);
			}
			else
			{
				string json = File.ReadAllText(path);
				settings = Serializer.Deserialize<T>(json);
			}

			await settings.OnLoaded(this);
			return settings;
		}

		public async Task Save(SettingsBase settings)
		{
			await settings.OnSaving();

			string path = SettingsDirectory + settings.GetType().Name + ".json";
			string json = Serializer.Serialize(settings);
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
	}
}
