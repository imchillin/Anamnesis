// © Anamnesis.
// Licensed under the MIT license.

using System.Text.Json;

namespace Scripts;

public class CleanNpcNames : ScriptBase
{
	const string filePath = "../Anamnesis/Data/NpcNames.json";

	public override string Name => "Clean NPC names";

	public override void Run()
	{
		JsonSerializerOptions op = new();
		op.AllowTrailingCommas = true;
		op.WriteIndented = true;

		string json = File.ReadAllText(filePath);
		Dictionary<string, string>? entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json, op);
		Dictionary<string, string> results = new Dictionary<string, string>();
		List<string> keys = new List<string>();

		if (entries == null)
			throw new Exception("Failed to deserialize npc names");

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

		json = JsonSerializer.Serialize(entries, op);
		File.WriteAllText(filePath, json);
	}

	public class Entry
	{
		public string? Appearance { get; set; }
		public string? Name { get; set; }
	}
}