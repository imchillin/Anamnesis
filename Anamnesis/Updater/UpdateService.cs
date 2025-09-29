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
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public partial class UpdateService : ServiceBase<UpdateService>
{
	private const string REPOSITORY_NAME = "imchillin/Anamnesis";

	// GitHub API rate limits requests to 60/h for unauthenticated users.
	// The requests are associated with the originating IP address.
	// Choose a reasonable update interval to avoid hitting the limit.
	private const int UPDATE_INTERVAL_MINS = 10;

	private const int FORCE_SHUTDOWN_TIMEOUT = 10000; // ms (10 seconds)

	private readonly HttpClient httpClient = new();
	private Release? currentRelease;

	private static string UpdateTempDir => Path.Combine(Path.GetTempPath(), "AnamnesisUpdateLatest");

	public override async Task Initialize()
	{
		await base.Initialize();

		bool skipTimeCheck = false;

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

	public async Task<bool> CheckForUpdates()
	{
		if (Directory.Exists(UpdateTempDir))
		{
			var dirInfo = new DirectoryInfo(UpdateTempDir);
			FileService.SetAttributesNormal(dirInfo);
			Directory.Delete(UpdateTempDir, true);
		}

		if (!this.httpClient.DefaultRequestHeaders.Contains("User-Agent"))
			this.httpClient.DefaultRequestHeaders.Add("User-Agent", "AutoUpdater");

		try
		{
			string url = $"https://api.github.com/repos/{REPOSITORY_NAME}/releases/latest";
			string result = await this.httpClient.GetStringAsync(url);
			this.currentRelease = JsonSerializer.Deserialize<Release>(result);

			if (this.currentRelease == null)
				throw new Exception("Failed to deserialize GitHub API JSON response");

			if (this.currentRelease.TagName == null)
				throw new Exception("No tag name in GitHub API JSON response");

			bool update = false;

			// Check for old date-based tag format: yyyy-MM-dd(-h#)
			if (ReleaseTagRegex().IsMatch(this.currentRelease.TagName))
			{
				// Trigger an update if the tag is using the old date format.
				update = true;
			}
			else
			{
				if (!Version.TryParse(this.currentRelease.TagName.TrimStart('v'), out Version? latestReleaseVer))
					throw new Exception("Failed to parse version from tag name");

				update = latestReleaseVer > VersionInfo.ApplicationVersion;
			}

			if (update)
			{
				await Dispatch.MainThread();

				UpdateDialog dlg = new()
				{
					Changes = this.currentRelease.Changes,
				};
				await ViewService.ShowDialog<UpdateDialog, bool?>("Update", dlg);
			}

			SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
			SettingsService.Save();
			return update;
		}
		catch (HttpRequestException ex)
		{
			// 404 errors just mean there are no latest releases.
			if (ex.StatusCode == HttpStatusCode.NotFound)
			{
				SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
				SettingsService.Save();
				return false;
			}

			if (ex.StatusCode == HttpStatusCode.Forbidden || ex.StatusCode == HttpStatusCode.TooManyRequests)
			{
				await GenericDialog.ShowLocalizedAsync("Update_RateLimit", "Update_Check_Fail_Title", System.Windows.MessageBoxButton.OK);
				return false;
			}

			Log.Error(ex, "Failed to complete update check");
			return false;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to complete update check");
			return false;
		}
	}

	public async Task DoUpdate(Action<double>? updateProgress = null)
	{
		try
		{
			string? currentExePath = Environment.GetCommandLineArgs()[0];

			if (string.IsNullOrEmpty(currentExePath))
				throw new Exception("Unable to determine current assembly path");

			currentExePath = currentExePath.Replace(".dll", ".exe");
			if (!File.Exists(currentExePath))
				throw new Exception("Unable to determine current executable path");

			if (this.currentRelease == null)
				throw new Exception("No release to download");

			if (this.currentRelease.Assets == null)
				throw new Exception("No assets in release");

			Release.Asset? asset = null;
			foreach (Release.Asset tAsset in this.currentRelease.Assets)
			{
				if (tAsset.Name == null)
					continue;

				if (!tAsset.Name.EndsWith(".zip", StringComparison.InvariantCulture))
					continue;

				asset = tAsset;
			}

			if (asset == null)
				throw new Exception("Failed to find asset for release");

			if (asset.Url == null)
				throw new Exception("Release asset has no url");

			// Download asset to temp file
			string zipFilePath = Path.GetTempFileName();
			{
				using HttpResponseMessage response = await this.httpClient.GetAsync(asset.Url);
				await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
				await using FileStream fileStream = new(zipFilePath, FileMode.OpenOrCreate, FileAccess.Write);
				await streamToReadFrom.CopyToAsync(fileStream);
			}

			// TODO: Progress bar
			updateProgress?.Invoke(1.0);

			if (!Directory.Exists(UpdateTempDir))
			{
				Directory.CreateDirectory(UpdateTempDir);
			}

			{
				using FileStream zipFile = new(zipFilePath, FileMode.Open);
				using ZipArchive archive = new(zipFile, ZipArchiveMode.Read);
				archive.ExtractToDirectory(UpdateTempDir, true);
				archive.Dispose();
				await zipFile.DisposeAsync();
			}

			if (File.Exists(zipFilePath))
			{
				// Remove temp file
				var fileInfo = new FileInfo(zipFilePath)
				{
					Attributes = FileAttributes.Normal,
				};

				File.Delete(zipFilePath);
			}

			// While testing, do not copy the update files over our working files.
			if (Debugger.IsAttached)
			{
				var dirInfo = new DirectoryInfo(UpdateTempDir);
				FileService.SetAttributesNormal(dirInfo);
				Directory.Delete(UpdateTempDir, true);

				string[] paths = Directory.GetFiles(".", "*.*", SearchOption.AllDirectories);
				foreach (string path in paths)
				{
					string dest = path.Replace(".\\", UpdateTempDir + "\\");

					string? dir = Path.GetDirectoryName(dest);
					if (dir != null && !Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					File.Copy(path, dest, true);
				}
			}

			// Start the update extractor
			string currentDir = Directory.GetCurrentDirectory();
			string procName = Process.GetCurrentProcess().ProcessName;
			ProcessStartInfo start = new(Path.Combine(UpdateTempDir, "Updater", "UpdateExtractor.exe"), $"\"{currentDir}\" {procName}");
			Process.Start(start);

			// Shutdown anamnesis
			// Note: Ensure shutdown call takes place on the UI thread
			App.Current.Dispatcher.Invoke(() =>
			{
				// Application may not exit if any windows are still open
				foreach (Window window in App.Current.Windows)
				{
					window.Close();
				}

				Log.Information("Attempting graceful shutdown after update.");
				App.Current.Shutdown();

				// Force shutdown if application doesn't shutdown gracefully within the timeout.
				_ = Task.Run(async () =>
				{
					await Task.Delay(FORCE_SHUTDOWN_TIMEOUT);
					Log.Warning("Forceful shutdown triggered after update (timeout reached).");
					Environment.Exit(0);
				});
			});

			return;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to perform update");
		}
	}

	[System.Text.RegularExpressions.GeneratedRegex(@"^\d{4}-\d{2}-\d{2}(-h\d+)?$")]
	private static partial System.Text.RegularExpressions.Regex ReleaseTagRegex();

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
}
