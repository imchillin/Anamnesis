// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra
{
	using System.Threading.Tasks;

	public static class Penumbra
	{
		public static async Task Redraw(string targetName)
		{
			RedrawData data = new();
			data.Name = targetName;
			data.Type = RedrawData.RedrawType.WithSettings;

			await PenumbraApi.Post("/redraw", data);

			await Task.Delay(500);
		}

		public class RedrawData
		{
			public enum RedrawType
			{
				WithoutSettings,
				WithSettings,
				OnlyWithSettings,
				Unload,
				RedrawWithoutSettings,
				RedrawWithSettings,
				AfterGPoseWithSettings,
				AfterGPoseWithoutSettings,
			}

			public string Name { get; set; } = string.Empty;
			public RedrawType Type { get; set; } = RedrawType.WithSettings;
		}
	}
}
