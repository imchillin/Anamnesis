// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Brio;

using System;
using System.Threading.Tasks;
using static Anamnesis.Penumbra.Penumbra.RedrawData;

public static class Brio
{
	public static async Task<string> Redraw(int targetIndex, RedrawType redrawType)
	{
		RedrawData data = new();
		data.ObjectIndex = targetIndex;
		data.RedrawType = redrawType;

		var result = await BrioApi.Post("/redraw", data);

		await Task.Delay(500);

		return result;
	}

	public static async Task<int> Spawn(SpawnOptions options)
	{
		SpawnRequest data = new();
		data.SpawnOptions = options;
		var resultRaw = await BrioApi.Post("/spawn", data);
		var resultId = int.Parse(resultRaw);
		await Task.Delay(500);
		return resultId;
	}

	public static async Task<bool> Despawn(int actorIndex)
	{
		DespawnData data = new();
		data.ObjectIndex = actorIndex;
		var resultRaw = await BrioApi.Post("/despawn", data);
		var result = bool.Parse(resultRaw);
		return result;
	}
}

[Flags]
public enum RedrawType
{
	None = 0,
	AllowOptimized = 1,
	AllowFull = 2,
	ForceRedrawWeaponsOnOptimized = 4,
	PreservePosition = 8,
	ForceAllowNPCAppearance = 16,

	All = AllowOptimized | AllowFull | ForceRedrawWeaponsOnOptimized | PreservePosition | ForceAllowNPCAppearance,
}

[Flags]
public enum SpawnOptions
{
	None = 0,
	ApplyModelPosition = 1,
}

public class RedrawData
{
	public int ObjectIndex { get; set; } = -1;
	public RedrawType? RedrawType { get; set; } = Anamnesis.Brio.RedrawType.All;
}

public class DespawnData
{
	public int ObjectIndex { get; set; } = -1;
}

public class SpawnRequest
{
	public SpawnOptions SpawnOptions { get; set; }
}
