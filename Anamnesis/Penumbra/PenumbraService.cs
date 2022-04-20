// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Services;
	using XivToolsWpf;

	public class PenumbraService : ServiceBase<PenumbraService>
	{
		public ObservableCollection<string> Collections { get; private set; } = new ObservableCollection<string>();
		public bool IsEnabled { get; set; }

		public static async Task Redraw(string targetName, string? characterCollection = null)
		{
			RedrawData data = new();
			data.Name = targetName;
			data.Type = RedrawData.RedrawType.WithSettings;
			data.CharacterCollection = characterCollection;

			await PenumbraApi.Post("/redraw", data);

			await Task.Delay(500);
		}

		public static async Task<List<string>> GetCollectionNames()
		{
			return await PenumbraApi.Get<List<string>>("/collections");
		}

		public override Task Start()
		{
			SettingsService.SettingsChanged += this.OnSettingsChanged;

			if (this.IsEnabled)
				Task.Run(this.Cache);

			return base.Start();
		}

		private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
		{
			this.IsEnabled = SettingsService.Current.EnablePenumbraApi;

			if (this.IsEnabled)
			{
				Task.Run(this.Cache);
			}
		}

		private async Task Cache()
		{
			List<string> collections;

			try
			{
				collections = await GetCollectionNames();
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to get collections from penumbra");
				await GenericDialog.ShowAsync("Failed to reach penumbra. (Have you enabled the HTTP API in penumbra settings?)", "Error", MessageBoxButton.OK);
				return;
			}

			await Dispatch.MainThread();

			this.Collections.Clear();
			foreach (string collectionName in collections)
			{
				this.Collections.Add(collectionName);
			}
		}

		public class RedrawData
		{
			public enum RedrawType
			{
				WithoutSettings,
				WithSettings,
				OnlyWithSettings,
				Unload,
				RedrawWithoutSettings,
				RedrawWithSettings,
				AfterGPoseWithSettings,
				AfterGPoseWithoutSettings,
			}

			public string Name { get; set; } = string.Empty;
			public RedrawType Type { get; set; } = RedrawType.WithSettings;
			public string? CharacterCollection { get; set; } = string.Empty;
		}
	}
}
