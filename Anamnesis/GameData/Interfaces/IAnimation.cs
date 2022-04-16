// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Interfaces
{
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Sheets;

	public interface IAnimation
    {
		public string? DisplayName { get; }
		public ImageReference? Icon { get; }
		public ActionTimeline? Timeline { get; }
	}
}
