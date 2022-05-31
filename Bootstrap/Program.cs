// © Anamnesis.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Reflection;
using Bootstrap;

const string ApplicationName = "Anamnesis";
const string BootstrapName = "Anamnesis Setup";
const string ResourceName = "Bootstrap.Anamnesis.exe";
const string FileName = "Anamnesis.exe";

const string DotNetMatchVersion = "^6.0.4"; 
const string DotNetVersion = "6.0.5";
const string DotNetTempFile = "windowsdesktop-runtime-6.0.5-win-x64.exe";
const string DotNetDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/5681bdf9-0a48-45ac-b7bf-21b7b61657aa/bbdc43bc7bf0d15b97c1a98ae2e82ec0/windowsdesktop-runtime-6.0.5-win-x64.exe";
const string DotNetDownloadChecksum = "5eb1537295cdb513197419c311777229fd43af6cea0ef6134f9990b32b8ac26aa51139f2c0b63d9cdfb6d753dd9db6f243b887ec511f15866157aa9e127b5cea";

const string DotNetPromptText = $"The .Net {DotNetVersion} Desktop runtime is not installed.\n\nInstall it now?";
const string AlreadyExtracted = $"{ApplicationName} is already extracted. Do you want to overwrite it?";

try
{
	if (CheckRunning())
		return;

	if (await CheckDotNet())
	{
		Launch();
	}
}
catch (Exception ex)
{
	User32.MessageBox(ex.Message, $"{BootstrapName} Error", User32.MessageBoxButtons.Ok, User32.MessageBoxIcon.Error);
}

bool CheckRunning()
{
	Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(FileName));
	if (procs.Length == 0)
		return false;

	User32.MessageBox($"Multiple {ApplicationName} processes found. Please close all other instances.", $"{ApplicationName} is running", User32.MessageBoxButtons.Ok, User32.MessageBoxIcon.Information);
	return true;
}

// Check dotnet is installed, and download it if not.
async Task<bool> CheckDotNet()
{
	// Check current dotnet installed versions
	{
		Process p = new Process();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.CreateNoWindow = true;
		p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		p.StartInfo.FileName = "dotnet";
		p.StartInfo.Arguments = " --list-runtimes";
		p.Start();
		
		p.WaitForExit();
		string? line;

		// Example: Microsoft.WindowsDesktop.App 6.0.5 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
		SemanticVersioning.Range matchRange = new(DotNetMatchVersion);
		while ((line = p.StandardOutput.ReadLine()) != null)
		{
			if (line.Contains($"Microsoft.WindowsDesktop.App"))
			{
				string versionStr = line.Split(" ")[1];
				SemanticVersioning.Version version = new(versionStr);
				if(matchRange.IsSatisfied(version))
				{
					return true;
				}
			}
		}
	}

	if (!User32.MessageBox(DotNetPromptText, $"Install .Net {DotNetVersion}", User32.MessageBoxButtons.YesNo, User32.MessageBoxIcon.Information))
		return false;

	// Get DotNet
	{
		// Setup http client
		HttpClient httpClient = new HttpClient();
		httpClient.Timeout = TimeSpan.FromMinutes(60);

		if (!httpClient.DefaultRequestHeaders.Contains("User-Agent"))
			httpClient.DefaultRequestHeaders.Add("User-Agent", "AutoUpdater");

		// Download installer
		using HttpResponseMessage response = await httpClient.GetAsync(DotNetDownloadUrl);
		using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
		using FileStream fileStream = new FileStream(DotNetTempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);

		// TODO: update progress somewhere somehow?
		// https://docs.microsoft.com/en-us/windows/win32/controls/create-progress-bar-controls
		await streamToReadFrom.CopyToAsync(fileStream);

		// Check hash
		string hash = Hash.Compute(fileStream);
		if (hash.ToUpperInvariant() != DotNetDownloadChecksum.ToUpperInvariant())
		{
			throw new Exception(".Net installer hash check failed");
		}
	}

	// Launch the installer
	ProcessStartInfo processStartInfo = new(DotNetTempFile);
	processStartInfo.ErrorDialog = false;
	processStartInfo.UseShellExecute = true;
	processStartInfo.Verb = "runas";
	Process? process = Process.Start(processStartInfo);

	if (process == null)
		throw new Exception($"Failed to launch {DotNetTempFile} process.");

	await process.WaitForExitAsync();

	// Ready to go!
	return true;
}

// Launch Anamnesis
void Launch()
{
	if (File.Exists(FileName))
	{
		if (!User32.MessageBox(AlreadyExtracted, $"{BootstrapName}", User32.MessageBoxButtons.YesNo, User32.MessageBoxIcon.Information))
			return;

		File.Delete(FileName);
	}

	if (!File.Exists(FileName))
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		using Stream? resourceStream = assembly.GetManifestResourceStream(ResourceName);
		if (resourceStream == null)
			throw new Exception($"Failed to load resource: {ResourceName}");

		using FileStream fileStream = new(FileName, FileMode.Create);
		resourceStream.CopyTo(fileStream);
	}

	ProcessStartInfo processStartInfo = new(FileName);
	Process? process = Process.Start(processStartInfo);

	if (process == null)
	{
		throw new Exception($"Failed to launch {FileName} process.");
	}
}