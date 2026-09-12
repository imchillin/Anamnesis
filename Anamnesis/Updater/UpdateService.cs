// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Updater;

using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory.Exceptions;
using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Velopack;
using Velopack.Sources;
using XivToolsWpf;

public partial class UpdateService : ServiceBase<UpdateService>
{
	private const string REPOSITORY_NAME = "imchillin/Anamnesis";
	private const string MANIFEST_RAW_URL = $"https://raw.githubusercontent.com/{REPOSITORY_NAME}/master/channels.json";
	private const string GITHUB_REPO_URL = $"https://github.com/{REPOSITORY_NAME}";

	// GitHub API rate limits requests to 60/h for unauthenticated users.
	// The requests are associated with the originating IP address.
	// Choose a reasonable update interval to avoid hitting the limit.
	private const int UPDATE_INTERVAL_MINS = 10;

	private readonly HttpClient httpClient = new();
	private UpdateManager? updateManager;
	private UpdateInfo? pendingUpdate;

	public static ChannelsManifest? GlobalManifest { get; private set; }
	public static ChannelData? CurrentChannelData { get; private set; }

	private static string UpdateTempDir => Path.Combine(Path.GetTempPath(), "AnamnesisUpdateLatest");

	public override async Task Initialize()
	{
		await base.Initialize();

		bool skipTimeCheck = false;

		try
		{
			await this.FetchChannelsManifest();
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to fetch channels manifest during initialization.");
			return;
		}

		// Determine if this is a dev build
		if (VersionInfo.IsDevelopmentBuild)
		{
			// Don't show if there is a debugger attached
			if (Debugger.IsAttached)
				return;

			// Prompt the user
			var result = await GenericDialog.ShowLocalizedAsync("DevBuild_Body", "DevBuild_Title", System.Windows.MessageBoxButton.YesNo);
			if (result == true)
				return;

			// Always skip the time check if they say no
			skipTimeCheck = true;
		}

		DateTimeOffset lastCheck = SettingsService.Current.LastUpdateCheck;
		TimeSpan elapsed = DateTimeOffset.Now - lastCheck;

		if (elapsed.TotalMinutes < UPDATE_INTERVAL_MINS && !skipTimeCheck)
		{
			Log.Information($"Last update check was less than {UPDATE_INTERVAL_MINS} minutes ago. Skipping.");
			return;
		}

		bool updateTriggered = await this.CheckForUpdates();
		if (updateTriggered)
			throw new UpdateTriggeredException();
	}

	/// <summary>
	/// Check for updates and prompt the user if an update is available.
	/// </summary>
	/// <returns>
	/// True if an update is available, false otherwise.
	/// </returns>
	public async Task<bool> CheckForUpdates()
	{
		try
		{
			// Legacy: Cleanup any old temporary update directories
			if (Directory.Exists(UpdateTempDir))
			{
				var dirInfo = new DirectoryInfo(UpdateTempDir);
				FileService.SetAttributesNormal(dirInfo);
				Directory.Delete(UpdateTempDir, true);
			}

			await this.FetchChannelsManifest();
			if (GlobalManifest == null)
			{
				Log.Warning("Channels manifest is empty after retrieval. Aborting update check.");
				return false;
			}

			string activeChannelId = SettingsService.Current.UpdateChannel ?? string.Empty;
			if (string.IsNullOrEmpty(activeChannelId))
			{
				Log.Warning($"Could not resolve current update channel. Defaulting to '{Settings.DEFAULT_UPDATE_CHANNEL}'.");
				activeChannelId = Settings.DEFAULT_UPDATE_CHANNEL;
			}

			if (!GlobalManifest.Channels.TryGetValue(activeChannelId, out var activeChannelData))
				throw new Exception($"Current update channel '{activeChannelId}' not found in channels manifest.");

			CurrentChannelData = activeChannelData;
			var githubSource = new GithubSource(GITHUB_REPO_URL, accessToken: null, prerelease: true);

			if (IsChannelCompatible(activeChannelData))
			{
				string activeVelopackChannel = GetVelopackChannel(activeChannelId);
				var updateOpts = new UpdateOptions
				{
					ExplicitChannel = activeVelopackChannel,
					AllowVersionDowngrade = true, // Desirable as it allows users to switch channels without being blocked by version checks
				};

				this.updateManager = new UpdateManager(githubSource, updateOpts);

				if (!this.updateManager.IsInstalled)
				{
					Log.Warning("The portable application does not support automatic updates.");
					return false;
				}

				this.pendingUpdate = await this.updateManager.CheckForUpdatesAsync();
				if (this.pendingUpdate != null)
				{
					bool update = await this.PromptUpdateConfirmation(this.pendingUpdate, activeChannelId);
					SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
					SettingsService.Save();
					return update;
				}

				// Active channel is compatible and up to date. Do not evaluate fallbacks.
				SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
				SettingsService.Save();
				return false;
			}

			Log.Information($"The channel '{activeChannelId}' is not compatible with game version '{VersionInfo.ValidatedGameVersion}'. Checking fallbacks...");

			var visitedChannels = new HashSet<string> { activeChannelId };
			string? nextFallbackId = activeChannelData.Fallback;

			while (!string.IsNullOrEmpty(nextFallbackId))
			{
				if (!visitedChannels.Add(nextFallbackId))
				{
					Log.Warning($"Cyclic fallback link detected on channel '{nextFallbackId}'. Aborting fallback evaluation.");
					break;
				}

				if (!GlobalManifest.Channels.TryGetValue(nextFallbackId, out var fallbackChannelData))
				{
					Log.Warning($"Fallback channel '{nextFallbackId}' specified in manifest but not defined.");
					break;
				}

				if (IsChannelCompatible(fallbackChannelData))
				{
					string fallbackVelopackChannel = GetVelopackChannel(nextFallbackId);
					var fallBackUpdateOpts = new UpdateOptions
					{
						ExplicitChannel = fallbackVelopackChannel,
						AllowVersionDowngrade = true,
					};

					this.updateManager = new UpdateManager(githubSource, fallBackUpdateOpts);
					this.pendingUpdate = await this.updateManager.CheckForUpdatesAsync();

					if (this.pendingUpdate != null)
					{
						await Dispatch.MainThread();

						bool? acceptSwitch = await GenericDialog.ShowAsync(
							LocalizationService.GetStringFormatted("Update_ChannelSwitch_FallbackAvailable_Body", activeChannelData.Name, fallbackChannelData.Name),
							LocalizationService.GetString("Update_ChannelSwitch_Title", true),
							System.Windows.MessageBoxButton.YesNo);

						if (acceptSwitch == true)
						{
							SettingsService.Current.UpdateChannel = nextFallbackId!;
							SettingsService.Save();

							CurrentChannelData = fallbackChannelData;
							bool update = await this.PromptUpdateConfirmation(this.pendingUpdate, nextFallbackId!);
							SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
							SettingsService.Save();
							return update;
						}

						break; // The user declined the channel switch
					}
				}

				nextFallbackId = fallbackChannelData.Fallback;
			}
		}
		catch (HttpRequestException ex)
		{
			if (ex.StatusCode == HttpStatusCode.Forbidden || ex.StatusCode == HttpStatusCode.TooManyRequests)
			{
				await GenericDialog.ShowLocalizedAsync("Update_RateLimit", "Update_Check_Fail_Title", System.Windows.MessageBoxButton.OK);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to carry out update check");
		}

		SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
		SettingsService.Save();
		return false;
	}

	/// <summary>
	/// Trigger the update process for a pending update.
	/// </summary>
	/// <param name="updateProgress">
	/// An optional callback to report download progress as a double between 0.0 and 100.0.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// </returns>
	public async Task DoUpdate(Action<double>? updateProgress = null)
	{
		if (this.updateManager == null || this.pendingUpdate == null)
		{
			Log.Warning("No pending update available to apply. Did you check for updates first?");
			return;
		}

		if (!this.updateManager.IsInstalled)
		{
			Log.Warning("The portable application does not support automatic updates.");
			return;
		}

		try
		{
			string channelName = CurrentChannelData?.Name ?? string.Empty;
			Log.Information($"Downloading update version {this.pendingUpdate.TargetFullRelease.Version} (Channel: {channelName})...");

			void ReflectVelopackProgress(int progress)
			{
				updateProgress?.Invoke((double)progress);
			}

			await this.updateManager.DownloadUpdatesAsync(this.pendingUpdate, ReflectVelopackProgress);

			Log.Information($"Applying update version {this.pendingUpdate.TargetFullRelease.Version} and restarting application...");

			this.updateManager.ApplyUpdatesAndRestart(this.pendingUpdate);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to download or apply application update.");
			throw;
		}
	}

	private static string GetVelopackChannel(string manifestChannelId)
	{
		// Based on the default implementation of Velopack's update manager
		string osShortName = VelopackRuntimeInfo.SystemOs.GetOsShortName();

		if (manifestChannelId.StartsWith($"{osShortName}-", StringComparison.OrdinalIgnoreCase))
			return manifestChannelId;

		return $"{osShortName}-{manifestChannelId}";
	}

	private static bool IsChannelCompatible(ChannelData channelData)
	{
		if (string.IsNullOrEmpty(channelData.ValidatedGameVersion))
			return false;

		return string.Equals(channelData.ValidatedGameVersion, VersionInfo.ValidatedGameVersion, StringComparison.OrdinalIgnoreCase);
	}

	private async Task<ChannelsManifest> FetchChannelsManifest()
	{
		if (!this.httpClient.DefaultRequestHeaders.Contains("User-Agent"))
			this.httpClient.DefaultRequestHeaders.Add("User-Agent", "AutoUpdater");

		string manifestJson = await this.httpClient.GetStringAsync(MANIFEST_RAW_URL);
		ChannelsManifest? manifest = JsonSerializer.Deserialize<ChannelsManifest>(manifestJson);

		if (manifest?.Channels == null)
			throw new Exception("Failed to deserialize channels manifest");

		GlobalManifest = manifest;
		return manifest;
	}

	private async Task<bool> PromptUpdateConfirmation(UpdateInfo updateInfo, string channelId)
	{
		string changelog = await this.FetchReleaseChangelog(updateInfo, channelId);

		await Dispatch.MainThread();
		UpdateDialog dlg = new()
		{
			Changes = changelog,
		};
		await ViewService.ShowDialog<UpdateDialog, bool?>("Update", dlg);
		return dlg.IsUpdating;
	}

	private async Task<string> FetchReleaseChangelog(UpdateInfo updateInfo, string channelId)
	{
		var version = updateInfo.TargetFullRelease.Version;
		try
		{
			if (!this.httpClient.DefaultRequestHeaders.Contains("User-Agent"))
				this.httpClient.DefaultRequestHeaders.Add("User-Agent", "AutoUpdater");

			string[] candidateTags = [
				$"v{version}-{channelId}",
				$"v{version}",
				$"{version}",
			];

			foreach (var candidateTag in candidateTags)
			{
				string tagUrl = $"https://api.github.com/repos/{REPOSITORY_NAME}/releases/tags/{candidateTag}";
				var response = await this.httpClient.GetAsync(tagUrl);

				if (response.IsSuccessStatusCode)
				{
					var release = await response.Content.ReadFromJsonAsync<Release>();
					if (!string.IsNullOrWhiteSpace(release?.Changes))
					{
						return release.Changes;
					}
				}
			}

			// Fallback: Check if the package has an embed release notes changelog
			if (updateInfo.TargetFullRelease.NotesMarkdown != null)
			{
				return updateInfo.TargetFullRelease.NotesMarkdown;
			}
		}
		catch (HttpRequestException)
		{
			throw; // Rethrown so that the exception is handled in the update check method
		}
		catch (Exception ex)
		{
			Log.Warning(ex, $"Failed to fetch release notes from GitHub for version {version}. Falling back to default message.");
		}

		return LocalizationService.GetString("Update_NoChangelog");
	}

	public class Release
	{
		[JsonPropertyName("tag_name")]
		public string? TagName { get; set; }

		[JsonPropertyName("body")]
		public string? Changes { get; set; }

		[JsonPropertyName("assets")]
		public List<Asset>? Assets { get; set; }

		public class Asset
		{
			[JsonPropertyName("browser_download_url")]
			public string? Url { get; set; }

			[JsonPropertyName("name")]
			public string? Name { get; set; }
		}
	}

	public class ChannelsManifest
	{
		[JsonPropertyName("channels")]
		public Dictionary<string, ChannelData> Channels { get; set; } = new();
	}

	public class ChannelData
	{
		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("validated_game_version")]
		public string ValidatedGameVersion { get; set; } = string.Empty;

		[JsonPropertyName("fallback")]
		public string? Fallback { get; set; } = string.Empty;
	}
}
