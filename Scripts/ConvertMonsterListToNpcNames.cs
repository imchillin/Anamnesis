// © Anamnesis.
// Licensed under the MIT license.

using System.Text.Json;

namespace Scripts;

/*public class ConvertMonsterListToNpcNames : ScriptBase
{
	public override string Name { get; }

	public override Task Run()
	{
		Dictionary<string, uint> nameToNameId = new Dictionary<string, uint>();

		var sheet = this.lumina.GetExcelSheet<BNpcName>();
		foreach (var entry in sheet)
		{
			string name = entry.Singular;
			name = name.ToLower();
			name = name.Replace(" ", string.Empty);

			// Dont care about dupe names from the ffxiv files
			if (nameToNameId.ContainsKey(name))
				continue;

			nameToNameId.Add(name, entry.RowId);
		}

		Log.Information($"Got {nameToNameId.Count} names");

		List<Monster> monsters = SerializerService.DeserializeFile<List<Monster>>("Monsters.json");
		Dictionary<string, string> names = new Dictionary<string, string>();

		int missing = 0;

		foreach (Monster monster in monsters)
		{
			if (monster.Name == null)
			{
				Log.Warning($"No name on monster: {monster.ModelType}");
				continue;
			}

			string name = monster.Name;
			name = name.ToLower();
			name = name.Replace(" ", string.Empty);

			if (name == "player")
				continue;

			if (name == "none")
				continue;

			bool found = false;
			found |= Populate('B', BattleNPCs, monster, ref names);
			found |= Populate('E', EventNPCs, monster, ref names);

			if (!found)
			{
				Log.Information($"Could not lcoate an NPC for {monster.ModelType} {monster.Name}.");
				missing++;
			}
		}

		Log.Information($"Got {names.Count} names.");
		Log.Information($"Missing {missing} monsters.");

		foreach ((string id, string name) in names)
		{
			string name2 = name;
			name2 = name2.ToLower();
			name2 = name2.Replace(" ", string.Empty);

			if (nameToNameId.ContainsKey(name2))
			{
				name2 = $"B:{nameToNameId[name2]}";
				names[id] = name2;
			}
		}

		Dictionary<string, string> names2 = new Dictionary<string, string>();
		List<string> keys = new List<string>(names.Keys.ToArray());
		keys.Sort();
		foreach (string key in keys)
		{
			names2.Add(key, names[key]);
		}

		SerializerService.SerializeFile("NpcNames.json", names2);
	}

	private static bool Populate(char key, ISheet<INpcBase> npcs, Monster monster, ref Dictionary<string, string> names)
	{
		foreach (INpcBase npc in npcs)
		{
			if (CheckNpc(npc, monster))
			{
				string npcKey = $"{key}:{npc.Key}";
				if (names.ContainsKey(npcKey))
				{
					Log.Warning($"Duplicate NPC for monster: {monster.Name}: {npcKey} / {names[npcKey]}");
				}
				else
				{
					names.Add(npcKey, monster.Name);
				}

				return true;
			}
		}

		return false;
	}

	private static bool CheckNpc(INpcBase npc, Monster monster)
	{
		if (npc.ModelCharaRow != monster.ModelType)
			return false;

		INpcEquip eq = npc.NpcEquip;

		if (eq.Body.ModelBase != monster.Body)
			return false;

		if (eq.Body.ModelVariant != monster.BodyV)
			return false;

		if (eq.Head.ModelBase != monster.HeadB)
			return false;

		if (eq.Head.ModelVariant != monster.HeadV)
			return false;

		if (eq.Hands.ModelBase != monster.HandsB)
			return false;

		if (eq.Hands.ModelVariant != monster.HandsV)
			return false;

		if (eq.Legs.ModelBase != monster.LegsB)
			return false;

		if (eq.Legs.ModelVariant != monster.LegsV)
			return false;

		if (eq.Feet.ModelBase != monster.FeetB)
			return false;

		if (eq.Feet.ModelVariant != monster.FeetV)
			return false;

		if (eq.MainHand.ModelSet != monster.MainS)
			return false;

		if (eq.MainHand.ModelBase != monster.MainB)
			return false;

		if (eq.MainHand.ModelVariant != monster.MainV)
			return false;

		if (eq.OffHand.ModelSet != monster.OffS)
			return false;

		if (eq.OffHand.ModelBase != monster.OffB)
			return false;

		if (eq.OffHand.ModelVariant != monster.OffV)
			return false;

		foreach (INpcBase eventNpc in EventNPCs)
		{
			uint i = eventNpc.ModelCharaRow;

			if (modelTypeToNameId.ContainsKey(i))
			{
				names.Add($"E:{eventNpc.Key}", $"B:{modelTypeToNameId[i]}");
			}
			else if (modelTypeToName.ContainsKey(i))
			{
				names.Add($"E:{eventNpc.Key}", $"{modelTypeToName[i]}");
			}
		}

		return true;
	}

	public class Monster
	{
		public string? Name { get; set; }
		public uint ModelType { get; set; }
		public uint Body { get; set; }
		public uint BodyV { get; set; }
		public uint HeadB { get; set; }
		public uint HeadV { get; set; }
		public uint HandsB { get; set; }
		public uint HandsV { get; set; }
		public uint LegsB { get; set; }
		public uint LegsV { get; set; }
		public uint FeetB { get; set; }
		public uint FeetV { get; set; }
		public uint MainS { get; set; }
		public uint MainB { get; set; }
		public uint MainV { get; set; }
		public uint OffS { get; set; }
		public uint OffB { get; set; }
		public uint OffV { get; set; }
	}
}*/