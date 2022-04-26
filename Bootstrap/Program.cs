// © Anamnesis.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Reflection;
using Bootstrap;

const string ResourceName = "Bootstrap.Anamnesis.exe";
const string FileName = "AnamnesisLaunch.exe";

try
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

	process.WaitForExit();
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