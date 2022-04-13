#r "nuget: Newtonsoft.Json, 13.0.1"

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Net;

public class Entry
{
	public string Appearance { get; set; }
	public string Name { get; set; }
}

const string filePath = "../Anamnesis/Data/NpcNames.json";

string json = File.ReadAllText(filePath);
Dictionary<string, string> entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
Dictionary<string, string> results = new Dictionary<string, string>();
List<string> keys = new List<string>();

// Add 0 padding to the name and values
foreach ((string appearance, string name) in entries)
{
	string appearanceValue = appearance;
	string nameValue = name;

	if (appearanceValue.Length != 7)
	{
		string[] parts = appearanceValue.Split(':');
		uint value = uint.Parse(parts[1]);
		appearanceValue = parts[0] + ":" + value.ToString("D5");
	}

	if (nameValue.Length != 7 && nameValue.StartsWith("B:"))
	{
		string[] parts = nameValue.Split(':');
		uint value = uint.Parse(parts[1]);
		nameValue = parts[0] + ":" + value.ToString("D5");
	}

	results.Add(appearanceValue, nameValue);
	keys.Add(appearanceValue);
}

keys.Sort((a, b) => a.CompareTo(b));

entries.Clear();
foreach (string key in keys)
{
	entries.Add(key, results[key]);
}

json = JsonConvert.SerializeObject(entries, Formatting.Indented);
File.WriteAllText(filePath, json);