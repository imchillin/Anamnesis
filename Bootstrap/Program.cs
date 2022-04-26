// © Anamnesis.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Reflection;
using Bootstrap;

const string ResourceName = "Bootstrap.Anamnesis.exe";
const string FileName = "AnamnesisLaunch.exe";

const string DotNetVersion = "6.0.4";
const string DotNetTempFile = "windowsdesktop-runtime-6.0.4-win-x64.exe";
const string DotNetDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/f13d7b5c-608f-432b-b7ec-8fe84f4030a1/5e06998f9ce23c620b9d6bac2dae6c1d/windowsdesktop-runtime-6.0.4-win-x64.exe";
const string DotNetDownloadChecksum = "209e596edd7ab022241ab378e66703912974e7aa6168f287c9ce036fb31e58029ad304c8182b4b62a08e8d5ae4db74de277e298ced6d2746ef08da2352a2a252";

const string DotNetPromptText = $"The .Net {DotNetVersion} Desktop runtime is not installed.\n\nInstall it now?";

try
{
	if (await CheckDotNet())
	{
		await Launch();
	}
}
catch (Exception ex)
{
	User32.MessageBox(ex.Message, "Anamnesis Bootstrap Error", User32.MessageBoxButtons.Ok, User32.MessageBoxIcon.Error);
}
finally
{
	if (File.Exists(FileName))
	{
		File.Delete(FileName);
	}
}

// Check dotnet is installed, and download it if not.
async Task<bool> CheckDotNet()
{
	if (!User32.MessageBox(DotNetPromptText, $"Install .Net {DotNetVersion}", User32.MessageBoxButtons.YesNo, User32.MessageBoxIcon.Information))
		return false;

	// Get DotNet
	{
		// Setup http client
		HttpClient httpClient = new HttpClient();

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
async Task Launch()
{
	if (File.Exists(FileName))
		File.Delete(FileName);

	Assembly assembly = Assembly.GetExecutingAssembly();
	using (Stream? resourceStream = assembly.GetManifestResourceStream(ResourceName))
	{
		if (resourceStream == null)
			throw new Exception($"Failed to load resource: {ResourceName}");

		using FileStream fileStream = new(FileName, FileMode.Create);
		resourceStream.CopyTo(fileStream);
	}

	ProcessStartInfo processStartInfo = new(FileName);
	processStartInfo.ErrorDialog = false;
	processStartInfo.UseShellExecute = true;
	processStartInfo.Verb = "runas";
	Process? process = Process.Start(processStartInfo);

	if (process == null)
		throw new Exception($"Failed to launch {FileName} process.");

	await process.WaitForExitAsync();
}