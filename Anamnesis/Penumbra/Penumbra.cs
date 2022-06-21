// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra;

using System.Threading.Tasks;

public static class Penumbra
{
	public static async Task Redraw(int targetIndex)
	{
		RedrawData data = new();
		data.ObjectTableIndex = targetIndex;
		data.Type = RedrawData.RedrawType.Redraw;

		await PenumbraApi.Post("/redraw", data);

		await Task.Delay(500);
	}

	public class RedrawData
	{
		public enum RedrawType
		{
			Redraw,
			AfterGPose,
		}

		public string Name { get; set; } = string.Empty;
		public int ObjectTableIndex { get; set; } = -1;
		public RedrawType Type { get; set; } = RedrawType.Redraw;
	}
}
