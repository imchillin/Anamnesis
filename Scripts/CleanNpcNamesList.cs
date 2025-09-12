// © Anamnesis.
// Licensed under the MIT license.

using System.Text.Json;

namespace Scripts;

public class CleanNpcNames : ScriptBase
{
	const string FILE_PATH = "../../../../Anamnesis/Data/NpcNames.json";

	public override string Name => "Clean NPC names";

	public override void Run()
	{
		var op = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			WriteIndented = true,
		};

		string json = File.ReadAllText(FILE_PATH);
		var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json, op);
		var results = new Dictionary<string, string>();
		var keys = new List<string>();

		if (entries == null)
			throw new Exception("Failed to deserialize npc names");

		// Add 0 padding to the name and values
		foreach ((string appearance, string name) in entries)
		{
			string appearanceValue = appearance;
			string nameValue = name;

			if (appearanceValue.Length != 9)
			{
				string[] parts = appearanceValue.Split(':');
				uint value = uint.Parse(parts[1]);
				appearanceValue = parts[0] + ":" + value.ToString("D7");
			}

			if (nameValue.Length != 9 && nameValue.StartsWith("N:"))
			{
				string[] parts = nameValue.Split(':');
				uint value = uint.Parse(parts[1]);
				nameValue = parts[0] + ":" + value.ToString("D7");
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

		json = JsonSerializer.Serialize(entries, op);
		File.WriteAllText(FILE_PATH, json);
	}

	public class Entry
	{
		public string? Appearance { get; set; }
		public string? Name { get; set; }
	}
}