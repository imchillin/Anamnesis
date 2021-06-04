using System;
using System.IO;
using System.Diagnostics;

// read version
string text = File.ReadAllText("Version.txt");
string[] parts = text.Split(";", StringSplitOptions.RemoveEmptyEntries);
string newVersion = parts[0];

Console.WriteLine("New Version: " + newVersion);

Update("/Setup/Setup.vdproj", newVersion);
Update("/Anamnesis/Anamnesis.csproj", newVersion);

Console.WriteLine("Done");

private static void Update(string path, string version)
{
	string finalPath = Directory.GetCurrentDirectory() + path;

	Console.WriteLine("Updating " + finalPath);

	string text = File.ReadAllText(finalPath);
	string[] lines = text.Split("\n");
	text = string.Empty;

	foreach (string line in lines)
	{
		string newLine = line;

		if (newLine.Trim().StartsWith("\"ProductVersion\" = "))
			newLine = $"        \"ProductVersion\" = \"8:{version}\"";

		if (newLine.StartsWith("        \"ProductCode\" = "))
			newLine = $"        \"ProductCode\" = \"8:{{{Guid.NewGuid().ToString().ToUpper()}}}\"";

		if (newLine.Trim().StartsWith("<AssemblyVersion>"))
			newLine = $"		<AssemblyVersion>{version}.0</AssemblyVersion>";

		if (newLine.Trim().StartsWith("<Version>"))
			newLine = $"		<Version>{version}.0</Version>";

		if (newLine.Trim().StartsWith("<FileVersion>"))
			newLine = $"		<FileVersion>{version}.0</FileVersion>";

		text += newLine + "\n";
	}

	File.WriteAllText(finalPath, text);
}