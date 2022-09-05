// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.Services;

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anamnesis.Files;
using Anamnesis.Serialization;

public class CustomBoneNameService : ServiceBase<CustomBoneNameService>
{
	private static readonly string SavePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/CustomBoneNames.json");
	private static Dictionary<string, string> customBoneNames = new Dictionary<string, string>();

	public static string? GetBoneName(string bone)
	{
		if (customBoneNames.TryGetValue(bone, out var customName))
			return customName;
		return null;
	}

	public static void SetBoneName(string bone, string? customName)
	{
		if (customBoneNames.ContainsKey(bone))
		{
			if (string.IsNullOrEmpty(customName))
			{
				customBoneNames.Remove(bone);
			}
			else
			{
				customBoneNames[bone] = customName;
			}
		}
		else
		{
			if (string.IsNullOrEmpty(customName))
				return;
			customBoneNames.Add(bone, customName);
		}

		Save();
	}

	public static void Save()
	{
		string json = SerializerService.Serialize(customBoneNames);
		File.WriteAllText(SavePath, json);
	}

	public override async Task Initialize()
	{
		await base.Initialize();
		try
		{
			if (File.Exists(SavePath))
			{
				string json = File.ReadAllText(SavePath);
				customBoneNames = SerializerService.Deserialize<Dictionary<string, string>>(json);
			}
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to load custom bone names.");
			customBoneNames = new Dictionary<string, string>();
			Save();
		}
	}

	public override Task Shutdown()
	{
		Save();
		return base.Shutdown();
	}
}
