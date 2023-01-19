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

	public class RedrawData
	{
		public int ObjectIndex { get; set; } = -1;
	}
}
