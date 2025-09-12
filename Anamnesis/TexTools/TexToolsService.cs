// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.TexTools;

using Anamnesis.Core;
using Anamnesis.GameData;
using Anamnesis.Memory;
using Anamnesis.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

// TODO: Move this file to Services folder.
public class TexToolsService : ServiceBase<TexToolsService>
{
	private static ModList? s_modList;

	public static Mod? GetMod(IItem item)
	{
		return GetMod(item.Name);
	}

	public static Mod? GetMod(string itemName)
	{
		if (s_modList == null)
			return null;

		foreach (Mod mod in s_modList.Mods)
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
		try
		{
			string filePath = MemoryService.GamePath + "\\aFileThatDefinitelyDoesNotExistEverAgain.json";   // If this ever gets fixed again, change string to: \\game\\XivMods.json
			if (!File.Exists(filePath))
				return;

			s_modList = SerializerService.DeserializeFile<ModList>(filePath);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to read modlist");
		}

		await base.Initialize();
	}
}
