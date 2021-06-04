// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Updater
{
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
	using Anamnesis.Services;

	public class UpdateService : ServiceBase<UpdateService>
	{
		public const string VersionFile = "Version.txt";

		private const string Repository = "imchillin/Anamnesis";

		private HttpClient httpClient = new HttpClient();
		private Release? currentRelease;

		public static Version? Version => System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
		public static string? SupportedGameVersion { get; private set; }

		public override async Task Initialize()
		{
			await base.Initialize();

			if (!File.Exists(VersionFile))
				throw new Exception("No version file found");

			string[] parts = File.ReadAllText(VersionFile).Split(";", StringSplitOptions.RemoveEmptyEntries);

			SupportedGameVersion = parts[1].Trim();

			DateTimeOffset lastCheck = SettingsService.Current.LastUpdateCheck;
			TimeSpan elapsed = DateTimeOffset.Now - lastCheck;
			if (elapsed.TotalHours < 6)
			{
				Log.Information("Last update check was less than 6 hours ago. Skipping.");
				return;
			}

			if (!this.httpClient.DefaultRequestHeaders.Contains("User-Agent"))
				this.httpClient.DefaultRequestHeaders.Add("User-Agent", "AutoUpdater");

			try
			{
				string url = $"https://api.github.com/repos/{Repository}/releases/latest";
				string result = await this.httpClient.GetStringAsync(url);
				this.currentRelease = JsonSerializer.Deserialize<Release>(result);

				if (this.currentRelease == null)
					throw new Exception("Failed to deserialize json response");

				if (this.currentRelease.Published == null)
					throw new Exception("No published timestamp in update json");

				// v1.0.0-beta
				string? tag = this.currentRelease.TagName;

				if (tag == null)
					throw new Exception("Publiched update has no tag");

				if (tag.Contains('-'))
					tag = tag.Split('-')[0];

				tag = tag.Trim('v');

				// ensuire this is a version tag (And not a date tag like v2021-05-05-beta)
				if (tag.Contains('.'))
				{
					Version newVersion = new Version(tag);

					if (this.currentRelease.Published != null && newVersion > Version)
					{
						await Dispatch.MainThread();

						UpdateDialog dlg = new UpdateDialog();
						dlg.Changes = this.currentRelease.Changes;
						await ViewService.ShowDialog<UpdateDialog, bool?>("Update", dlg);
					}
				}

				SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
				SettingsService.Save();
			}
			catch (HttpRequestException ex)
			{
				// 404 errors just mean there are no latest releases.
				if (ex.StatusCode == HttpStatusCode.NotFound)
				{
					SettingsService.Current.LastUpdateCheck = DateTimeOffset.Now;
					SettingsService.Save();
					return;
				}

				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to complete update check");
				throw;
			}
		}

		public async Task DoUpdate(Action<double>? updateProgress = null)
		{
			try
			{
				if (this.currentRelease == null)
					throw new Exception("No release to download");

				if (this.currentRelease.Assets == null)
					throw new Exception("No assets in release");

				Release.Asset? asset = null;
				foreach (Release.Asset tAsset in this.currentRelease.Assets)
				{
					if (tAsset.Name == null)
						continue;

					if (!tAsset.Name.EndsWith(".msi"))
						continue;

					asset = tAsset;
				}

				if (asset == null)
					throw new Exception("Failed to find msi asset for release");

				if (asset.Url == null)
					throw new Exception("Release asset has no url");

				// Download asset to temp file
				string installerFilePath = Path.GetTempFileName();
				installerFilePath = installerFilePath.Replace(".tmp", ".msi");
				using WebClient client = new WebClient();
				if (updateProgress != null)
				{
					client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
					{
						updateProgress.Invoke(e.ProgressPercentage / 100.0);
					};
				}

				await client.DownloadFileTaskAsync(asset.Url, installerFilePath);

				// Start the update extractor
				ProcessStartInfo start = new ProcessStartInfo(installerFilePath);
				start.Arguments = "/passive";
				start.UseShellExecute = true;
				Process.Start(start);

				// Shutdown anamnesis
				App.Current.Shutdown();
				return;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to perform update");
			}
		}

		public class Release
		{
			[JsonPropertyName("tag_name")]
			public string? TagName { get; set; }

			[JsonPropertyName("published_at")]
			public DateTimeOffset? Published { get; set; }

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
}
