// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.TexTools;

using System;
using System.IO;
using System.Threading.Tasks;
using Anamnesis.GameData;
using Anamnesis.Memory;
using Anamnesis.Serialization;

public class TexToolsService : ServiceBase<TexToolsService>
{
	private static ModList? modList;

	public static Mod? GetMod(IItem item)
	{
		return GetMod(item.Name);
	}

	public static Mod? GetMod(string itemName)
	{
		if (modList == null)
			return null;

		foreach (Mod mod in modList.Mods)
		{
			if (!mod.Enabled)
				continue;

			if (itemName == mod.TrimmedName)
			{
				return mod;
			}
		}

		return null;
	}

	public override async Task Initialize()
	{
		await base.Initialize();

		try
		{
			string filePath = MemoryService.GamePath + "\\game\\XivMods.json";
			if (!File.Exists(filePath))
				return;

			modList = SerializerService.DeserializeFile<ModList>(filePath);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to read modlist");
		}
	}
}
