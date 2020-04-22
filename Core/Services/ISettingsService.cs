// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Threading.Tasks;

	#pragma warning disable SA1201

	public interface ISettingsService : IService
	{
		Task<T> Load<T>()
			where T : SettingsBase;

		Task Save(SettingsBase settings);
		event SettingsEvent SettingsSaved;
	}

	public delegate void SettingsEvent(SettingsBase settings);

	[Serializable]
	public abstract class SettingsBase
	{
		private ISettingsService settingsService;

		public virtual Task OnLoaded(ISettingsService settingsService)
		{
			this.settingsService = settingsService;
			return Task.CompletedTask;
		}

		public virtual Task OnSaving()
		{
			return Task.CompletedTask;
		}

		public void Save()
		{
			Task.Run(this.SaveAsync);
		}

		public Task SaveAsync()
		{
			return this.settingsService.Save(this);
		}
	}
}
