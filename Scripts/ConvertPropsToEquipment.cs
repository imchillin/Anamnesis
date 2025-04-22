// © Anamnesis.
// Licensed under the MIT license.

using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Serialization;

namespace Scripts;

public class ConvertPropsToEquipment : ScriptBase
{
	const string propsFilePath = "../../../../Anamnesis/Data/Props.json";
	const string equipmentFilePath = "../../../../Anamnesis/Data/Equipment.json";

	public override string Name => "Convert Props list to equipment";

	public override void Run()
	{
		string propsJson = File.ReadAllText(propsFilePath);
		Dictionary<string, string> props = SerializerService.Deserialize<Dictionary<string, string>>(propsJson);

		string equipmentJson = File.ReadAllText(equipmentFilePath);
		List<Equipment> equipment = SerializerService.Deserialize<List<Equipment>>(equipmentJson);

		foreach ((string key, string value) in props)
		{
			string[] parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries);

			string name = parts[0].Trim();
			string? desc = null;

			if (parts.Length == 2)
				desc = parts[1].Trim();

			Equipment eq = new()
			{
				Name = name,
				Description = desc,
				Id = key,
				Slot = ItemSlots.Weapons
			};

			equipment.Add(eq);
		}

		string json = SerializerService.Serialize(equipment);
		File.WriteAllText(equipmentFilePath, json);
	}

	public class Entry
	{
		public string? Appearance { get; set; }
		public string? Name { get; set; }
	}
}