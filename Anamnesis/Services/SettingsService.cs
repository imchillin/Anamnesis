// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
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
				settings.Save();
			}
			else
			{
				string json = File.ReadAllText(path);
				settings = SerializerService.Deserialize<T>(json);
			}

			return Task.FromResult(settings);
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
	}

	#pragma warning disable SA1402

	[Serializable]
	public abstract class SettingsBase
	{
		public event SettingsEvent? Changed;

		public void NotifyChanged()
		{
			this.Changed?.Invoke(this);
			this.Save();
		}

		public void Save()
		{
			string path = FileService.StoreDirectory + SettingsService.SettingsDirectory + this.GetType().Name + ".json";
			string json = SerializerService.Serialize(this);
			File.WriteAllText(path, json);
		}
	}
}
