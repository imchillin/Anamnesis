// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// A service that manages custom names for posable bones in the game.
/// </summary>
public class CustomBoneNameService : ServiceBase<CustomBoneNameService>
{
	private static readonly string s_savePath = FileService.ParseToFilePath(FileService.StoreDirectory + "/CustomBoneNames.json");
	private static Dictionary<string, string> s_customBoneNames = [];

	/// <summary>
	/// Gets the custom name for a bone (if it exists).
	/// </summary>
	/// <param name="bone">The target bone, represented by its unique internal name.</param>
	/// <returns>The custom name for the bone if found, or null if no custom name exists.</returns>
	public static string? GetBoneName(string bone)
	{
		if (s_customBoneNames.TryGetValue(bone, out var customName))
			return customName;

		return null;
	}

	/// <summary>Sets a custom name for a bone.</summary>
	/// <remarks>
	/// If the custom name parameter is set to null or empty, the custom name will be removed.
	/// </remarks>
	/// <param name="bone">The target bone, represented by its unique internal name.</param>
	/// <param name="customName">The custom (display) name to set for the bone.</param>
	public static void SetBoneName(string bone, string? customName)
	{
		bool changed = false;

		if (string.IsNullOrEmpty(customName))
		{
			changed = s_customBoneNames.Remove(bone);
		}
		else
		{
			changed = !s_customBoneNames.TryGetValue(bone, out var existing) || existing != customName;
			if (changed)
				s_customBoneNames[bone] = customName;
		}

		if (changed)
			Save();
	}

	/// <summary>
	/// Saves the current custom bone settings to the local file system.
	/// </summary>
	public static void Save()
	{
		string json = SerializerService.Serialize(s_customBoneNames);
		File.WriteAllText(s_savePath, json);
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		try
		{
			if (File.Exists(s_savePath))
			{
				string json = File.ReadAllText(s_savePath);
				s_customBoneNames = SerializerService.Deserialize<Dictionary<string, string>>(json);
			}
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to load custom bone names.");
			s_customBoneNames = [];   // Reset to empty dictionary on error
			Save();                 // Wipe the file to avoid future errors
		}

		await base.Initialize();
	}
}
