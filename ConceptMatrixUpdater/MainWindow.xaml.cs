using Ionic.Zip;
using MahApps.Metro.Controls;
using Markdig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ConceptMatrixUpdater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow, INotifyPropertyChanged
	{
		// Properties for the UI.
		public string StatusLabel { get; set; }
		public string HTML { get; set; }
		public int ProgressValue { get; set; }
		public bool ButtonEnabled { get; set; } = true;

		private JObject json;
		private readonly string temp = Path.Combine(Path.GetTempPath(), App.ToolBin);
		public bool AlertUpToDate = true;
		public bool ForceCheckUpdate = false;

#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

		public MainWindow()
		{
			InitializeComponent();

			// Set the security protocol, mainly for Windows 7 users.
			ServicePointManager.SecurityProtocol = (ServicePointManager.SecurityProtocol & SecurityProtocolType.Ssl3) | (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

			// Set data context to this.
			DataContext = this;
		}

		public bool Initialize()
		{
			// Get the current version of the application.
			var result = Version.TryParse(FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, $"{App.ToolBin}.exe")).FileVersion, out Version CurrentVersion);
			if (!result)
			{
				MessageBox.Show(
					$"There was an error when trying to read the current version of {App.ToolName}, you will be prompted to download the latest version.",
					App.UpdaterName,
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
				// Force to check the update.
				ForceCheckUpdate = true;
			}

			// Create request for Github REST API for the latest release of tool.
			if (WebRequest.Create($"https://api.github.com/repos/{App.GithubRepo}/releases/latest") is HttpWebRequest request)
			{
				request.Method = "GET";
				request.UserAgent = App.ToolName;
				request.ServicePoint.Expect100Continue = false;

				try
				{
					using (var r = new StreamReader(request.GetResponse().GetResponseStream()))
					{
						// Get the JSON as a JObject to get the properties dynamically.
						json = JsonConvert.DeserializeObject<JObject>(r.ReadToEnd());

						// Get tag name and remove the v in front.
						var tag_name = json["tag_name"].Value<string>();
						// Form release version from this string.
						var releaseVersion = new Version(tag_name);

						// Check if the release is newer.
						if (releaseVersion > CurrentVersion || ForceCheckUpdate)
						{
							// Create HTML out of the markdown in body.
							var html = Markdown.ToHtml(json["body"].Value<string>());
							// Set the update string
							StatusLabel = $"{App.ToolName} {releaseVersion.VersionString()} is now available, you have {CurrentVersion.VersionString()}. Would you like to download it now?";
							// Set HTML in the window.
							HTML = "<style>body{font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Helvetica,Arial,sans-serif,Apple Color Emoji,Segoe UI Emoji;margin:8px 10px;padding:0;font-size:12px;}ul{margin:0;padding:0;list-style-position:inside;}</style>" + html;
						}
						else
						{
							// Alerts that you're up to date.
							if (AlertUpToDate)
								MessageBox.Show("You're up to date!", App.UpdaterName, MessageBoxButton.OK, MessageBoxImage.Information);

							// Do not show.
							return false;
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
					var response = MessageBox.Show(
						"Failed to fetch the latest version! Would you like to visit the page manually to check for the latest release manually?",
						App.UpdaterName,
						MessageBoxButton.YesNo,
						MessageBoxImage.Error
					);
					if (response == MessageBoxResult.Yes)
					{
						// Visit the latest releases page on GitHub to download the latest version.
						Process.Start($"https://github.com/{App.GithubRepo}/releases/latest");

						// Do not show.
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Ensure the temp path exists.
		/// </summary>
		private void ValidateTempPath()
		{
			// Create temp diretory if it doesn't exist.
			if (!Directory.Exists(temp))
				Directory.CreateDirectory(temp);
		}

		/// <summary>
		/// When clicking the install button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInstallClick(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				// Disable the buttons to install or not.
				ButtonEnabled = false;

				// Use web client to download the update.
				using (var wc = new WebClient())
				{
					// Ensure the temp path exists.
					ValidateTempPath();

					// Temporary zip path.
					var tZip = Path.Combine(temp, App.ZipName);

					// Delete existing zip file.
					if (File.Exists(tZip))
						File.Delete(tZip);

					// Update status label.
					StatusLabel = "Downloading update...";

					// Download the file. 
					wc.DownloadFileAsync(new Uri(json["assets"][0]["browser_download_url"].Value<string>()), tZip);

					// When the download changes.
					wc.DownloadProgressChanged += UpdateDownloadProgressChanged;
					wc.DownloadFileCompleted += DownloadFileCompleted;
				}
			}),
			System.Windows.Threading.DispatcherPriority.ContextIdle);
		}

		/// <summary>
		/// Download of the zip file is completed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs _)
		{
			// Set status label to inform the download is complete.
			StatusLabel = "Unzipping files...";

			// Temporary zip path.
			var tZip = Path.Combine(temp, App.ZipName);

			// Create a background worker.
#pragma warning disable IDE0067 // Dispose objects before losing scope
			var bw = new BackgroundWorker() { WorkerReportsProgress = true };
#pragma warning restore IDE0067 // Dispose objects before losing scope

			// Reset the download progress.
			DownloadProgress.Value = 0;
			// Add the work loop.
			bw.DoWork += UnzipWorker;
			// Add the progress changed listener.
			bw.ProgressChanged += (__, e) => Dispatcher.Invoke(() => DownloadProgress.Value = e.ProgressPercentage);

			try
			{
				// Get any tools open.
				var procs = Process.GetProcessesByName(App.ToolBin);
				// Iterate over each.
				foreach (var p in procs)
				{
					// Kill it with fire and wait for it to close.
					p.Kill();
					p.WaitForExit(5000);
				}
			}
			catch (Exception) { }

			// Old updater path.
			var tempFile = Path.Combine(Environment.CurrentDirectory, $"{App.UpdaterBin}.exe.old");

			// Delete potential existing old updater.
			if (File.Exists(tempFile))
				File.Delete(tempFile);

			// Remove any existing old updater.
			// Move the updater (this executable) into the temp folder.
			File.Move(Path.Combine(Environment.CurrentDirectory, $"{App.UpdaterBin}.exe"), tempFile);

			// Run the worker.
			bw.RunWorkerAsync(tZip);

			// Worker is completed.
			bw.RunWorkerCompleted += async (__, ___) =>
			{
				// Update label for the unzip completion and tool startup.
				StatusLabel = $"Update complete! Starting {App.ToolName}...";

				// Wait 5 seconds before doing anything.
				await Task.Delay(5000);

				// Start the tool.
				Process.Start(Path.Combine(Environment.CurrentDirectory, $"{App.ToolBin}.exe"));

				// Shutdown the application, we're done here.
				Close();
			};
		}

		/// <summary>
		/// Background worker for unzipping files.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UnzipWorker(object sender, DoWorkEventArgs e)
		{
			// Unzip and overwrite all files.
			using (var zip = ZipFile.Read(e.Argument as string))
			{
				for (var i = 0; i < zip.Count; i++)
				{
					// Extract the zip into the current directory.
					zip[i].Extract(Environment.CurrentDirectory, ExtractExistingFileAction.OverwriteSilently);
					// Report progress of the unzip.
					(sender as BackgroundWorker).ReportProgress((i + 1) / zip.Count * 100);
				}
			}
		}

		/// <summary>
		/// Progress from webclient updates.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UpdateDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) => DownloadProgress.Value = e.ProgressPercentage;

		/// <summary>
		/// Clicking the no button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNoClick(object sender, RoutedEventArgs e) => Close();
	}
}
