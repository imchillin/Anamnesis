// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Brio;

using System.Threading.Tasks;

public static class Brio
{
	public static async Task Redraw(int targetIndex)
	{
		RedrawData data = new();
		data.ObjectIndex = targetIndex;

		await BrioApi.Post("/redraw", data);

		await Task.Delay(500);
	}

	public static async Task<int> Spawn()
	{
		var resultRaw = await BrioApi.Post("/spawn", 0);
		var resultId = int.Parse(resultRaw);
		await Task.Delay(500);
		return resultId;
	}

	public class RedrawData
	{
		public int ObjectIndex { get; set; } = -1;
	}
}
