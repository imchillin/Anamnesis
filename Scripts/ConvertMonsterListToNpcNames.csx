#pragma warning disable
		private void RunTest()
		{
			Dictionary<string, uint> nameToNameId = new Dictionary<string, uint>();

			var sheet = this.lumina.GetExcelSheet<BNpcName>();
			foreach (var entry in sheet)
			{
				string name = entry.Singular;
				name = name.ToLower();
				name = name.Replace(" ", string.Empty);

				if (nameToNameId.ContainsKey(name))
				{
					Log.Warning($"Duplicate name: {name}");
					continue;
				}

				nameToNameId.Add(name, entry.RowId);
			}

			Log.Information($"Got {nameToNameId.Count} names");

			List<Monster> monsters = SerializerService.DeserializeFile<List<Monster>>("Monsters.json");
			Dictionary<uint, uint> modelTypeToNameId = new Dictionary<uint, uint>();
			Dictionary<uint, string> modelTypeToName = new Dictionary<uint, string>();

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

				if (modelTypeToNameId.ContainsKey(monster.ModelType))
				{
					Log.Warning($"Duplicate model Type: {monster.ModelType}");
					continue;
				}

				if (nameToNameId.ContainsKey(name))
				{
					modelTypeToNameId.Add(monster.ModelType, nameToNameId[name]);
				}
				else
				{
					modelTypeToName.Add(monster.ModelType, monster.Name);
				}
			}

			Log.Information($"Got {modelTypeToNameId.Count} name mappings.");

			Dictionary<string, string> names = new Dictionary<string, string>();

			foreach (INpcBase battleNpc in BattleNPCs)
			{
				uint i = battleNpc.ModelCharaRow;

				if (modelTypeToNameId.ContainsKey(i))
				{
					names.Add($"B:{battleNpc.Key}", $"B:{modelTypeToNameId[i]}");
				}
				else if (modelTypeToName.ContainsKey(i))
				{
					names.Add($"B:{battleNpc.Key}", $"{modelTypeToName[i]}");
				}
			}

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

			SerializerService.SerializeFile("NpcNames.json", names);
		}

		public class Monster
		{
			public string Name { get; set; }
			public uint ModelType { get; set; }
		}