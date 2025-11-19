// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.Services;
using System;
using System.Collections.Generic;

public static class CustomizeOptionsCache
{
	public static Dictionary<DataPaths, List<byte>> ValidFaceIds { get; } = [];
	public static Dictionary<DataPaths, List<byte>> ValidHairIds { get; } = [];
	public static Dictionary<DataPaths, List<byte>> ValidTailEarIds { get; } = [];

	public static void Build()
	{
		foreach (DataPaths raceSex in Enum.GetValues<DataPaths>())
		{
			var faces = new List<byte>();
			var hairstyles = new List<byte>();
			var tailears = new List<byte>();

			// For non-NPC variants, collect NPC options as well
			var variants = new List<DataPaths> { raceSex };
			if (!raceSex.ToString().EndsWith("NPC"))
			{
				if (Enum.TryParse<DataPaths>($"{raceSex}NPC", out var npcVariant))
					variants.Add(npcVariant);
			}

			// Face
			for (byte i = 0; i < byte.MaxValue; ++i)
			{
				foreach (var variant in variants)
				{
					string path = DataPathResolver.ResolveFacePath(variant, i);
					if (GameDataService.FileExists(path) && !faces.Contains(i))
						faces.Add(i);
				}
			}

			ValidFaceIds[raceSex] = faces;

			// Hair
			for (byte i = 0; i < byte.MaxValue; ++i)
			{
				foreach (var variant in variants)
				{
					string path = DataPathResolver.ResolveHairPath(variant, i);
					if (GameDataService.FileExists(path) && !hairstyles.Contains(i))
						hairstyles.Add(i);
				}
			}

			ValidHairIds[raceSex] = hairstyles;

			// Tail/Ears
			for (byte i = 0; i < byte.MaxValue; ++i)
			{
				foreach (var variant in variants)
				{
					string path = DataPathResolver.ResolveTailEarsPath(variant, i);
					if (GameDataService.FileExists(path) && !tailears.Contains(i))
						tailears.Add(i);
				}
			}

			ValidTailEarIds[raceSex] = tailears;
		}
	}

	public static void Clear()
	{
		ValidFaceIds.Clear();
		ValidHairIds.Clear();
		ValidTailEarIds.Clear();
	}
}
